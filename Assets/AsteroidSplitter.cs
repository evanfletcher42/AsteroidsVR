using UnityEngine;
using System.Collections;
using System.Collections.Generic;

// Handles cutting an Asteroid mesh into two meshes (if there's enough vertices).
// This creates rough cuts.  Rather than creating new vertices on split edges,
// the script simply partitions the mesh in two and fills the resulting holes.

public class AsteroidSplitter {

    private static Plane splitPlane;
    private static Mesh baseMesh;

    private static GameObject prefabAsteroid = Resources.Load("Asteroid") as GameObject;

    public static GameObject[] Cut(GameObject baseObj, Vector3 planePt, Vector3 planeNormal)
    {
        //Debug.Log("Plane at" + planePt + " with normal " + planeNormal);

        // Create a plane in the base object's local coordinates.
        splitPlane = new Plane(planeNormal,
                                planePt);

        baseMesh = baseObj.GetComponent<MeshFilter>().mesh;

        // We split the mesh like this:
        //  - Remove all triangles that intersect with the plane.  
        //    This splits the mesh without removing vertices.
        //  - Tris are sorted into left-half and right-half of planes. 
        //      Here we assume that the mesh is generally convex, and a single cut
        //      won't create > 2 objects.  
        //  - Close the non-manifold edges by placing a vert at the average
        //      location of the non-manifold edges, then traverse around the
        //      non-manifold edge and fill with triangles.
        //
        // This method is expected to reduce the total volume, which is great for 
        // the goal/aesthetic of shooting a rock in Asteroids.

        // Step 1: Separate triangles that exist on different sides of the plane.
        //         Discard triangles that span across the plane.  

        int[] p = new int[3];
        bool[] side = new bool[3]; //on what sides of the plane do the 3 verts in a tri lie on?
        int[] nf = new int[3];  // Have we seen these before

        List<int> leftSideTris = new List<int>();
        List<Vector3> leftSideVerts = new List<Vector3>();
        List<int> leftSideNonManifold = new List<int>(); //subsequent pairs belong to the same edge; i.e leftSideNonManifold[0] and leftSideNonManifold[1]

        List<int> rightSideTris = new List<int>();
        List<Vector3> rightSideVerts = new List<Vector3>();
        List<int> rightSideNonManifold = new List<int>();

        int nVertsLeft = 0;
        int nVertsRight = 0;

        for (int i = 0; i < baseMesh.triangles.Length; i+=3)
        {
            for (int j = 0; j < 3; j++)
            {
                p[j] = baseMesh.triangles[i + j];
                side[j] = splitPlane.GetSide(baseMesh.vertices[p[j]]);
            }
          
            if (side[0] == side[1] && side[1] == side[2])
            {
                //If all lie on the same side, we should directly use
                //these tris in the resulting split rock.
                if (side[0])
                {
                    for (int j = 0; j < 3; j++)
                    {
                        nf[j] = leftSideVerts.IndexOf(baseMesh.vertices[p[j]]);
                        if (nf[j] == -1)
                        {
                            leftSideVerts.Add(baseMesh.vertices[p[j]]);
                            leftSideTris.Add(nVertsLeft++);
                        }
                        else
                        {
                            leftSideTris.Add(nf[j]);
                        }
                    }
                }
                else
                {
                    for (int j = 0; j < 3; j++)
                    {
                        nf[j] = rightSideVerts.IndexOf(baseMesh.vertices[p[j]]);
                        if (nf[j] == -1)
                        {
                            rightSideVerts.Add(baseMesh.vertices[p[j]]);
                            rightSideTris.Add(nVertsRight++);
                        }
                        else
                        {
                            rightSideTris.Add(nf[j]);
                        }
                    }
                }
            }
            else
            {
                // If this triangle spans across the plane, 
                // then the two points that lie on the same side of the plane mark a 
                // non-manifold edge, on which we must create a new triangle to fill the hole.
                // Note these verts may or may not already exist in the left or right object, so we check same as above.

                int numOnLeft = 0;
                for(int j=0; j < 3; j++)
                    numOnLeft += (side[j] ? 1 : 0);

                if(numOnLeft >= 2)
                {
                    //The manifold edge is on the left side of the plane.
                    for(int j=0; j < 3; j++)
                    {
                        if(side[j])
                        {
                            nf[j] = leftSideVerts.IndexOf(baseMesh.vertices[p[j]]);
                            if (nf[j] == -1)
                            {
                                leftSideVerts.Add(baseMesh.vertices[p[j]]);
                                leftSideNonManifold.Add(nVertsLeft++);
                            }
                            else
                            {
                                leftSideNonManifold.Add(nf[j]);
                            }
                        }
                    }
                }
                else
                {
                    //The manifold edge is on the right side of the plane.
                    for (int j = 0; j < 3; j++)
                    {
                        if (!side[j])
                        {
                            nf[j] = rightSideVerts.IndexOf(baseMesh.vertices[p[j]]);
                            if (nf[j] == -1)
                            {
                                rightSideVerts.Add(baseMesh.vertices[p[j]]);
                                rightSideNonManifold.Add(nVertsRight++);
                            }
                            else
                            {
                                rightSideNonManifold.Add(nf[j]);
                            }
                        }
                    }
                }
            }
        }

        // We now have two meshes, both with a big hole, and we know the verts/edges that are non-manifold.
        // Fill the holes by:
        //  - adding a vert at the mean position of all the non-manifold verts
        //  - make a face from every non-manifold edge to this new vert

        // ----- left side ----- 

        // find mean
        if (leftSideNonManifold.Count > 0)
        {
            //Debug.Log("----- Filling left -----");
            //Debug.Log("Finding Mean of " + leftSideNonManifold.Count + " Non-Manifold Points");
            Vector3 leftCtrPt = new Vector3(0, 0, 0);
            for (int i = 0; i < leftSideNonManifold.Count; i++)
            {
                leftCtrPt += leftSideVerts[leftSideNonManifold[i]];
                //Debug.Log("Add " + leftSideVerts[leftSideNonManifold[i]]);
            }
            leftCtrPt = leftCtrPt / (float)leftSideNonManifold.Count;
            //Debug.Log("Fan fill point is " + leftCtrPt);

            // add the new vert
            leftSideVerts.Add(leftCtrPt);
            int leftCtrIdx = nVertsLeft++;

            // make tris from all non-manifold edges
            for (int i = 0; i < leftSideNonManifold.Count; i += 2)
            {
                //view direction from left is opposite plane normal.  
                Vector3 viewDir = -planeNormal;
                //winding direction is normally [i] -> [i+1], but may need a flip.
                Vector3 sideA = leftSideVerts[leftSideNonManifold[i]] - leftCtrPt;
                Vector3 sideB = leftSideVerts[leftSideNonManifold[i + 1]] - leftCtrPt;
                Vector3 faceNormal = Vector3.Cross(sideA, sideB);

                bool swap = (Vector3.Dot(faceNormal, viewDir) < 0);


                leftSideTris.Add(leftCtrIdx);
                leftSideTris.Add(leftSideNonManifold[swap ? i + 1 : i]);
                leftSideTris.Add(leftSideNonManifold[swap ? i : i + 1]);
            }
        }

        // ----- right side ----- 

        if (rightSideNonManifold.Count > 0)
        {
            //Debug.Log("----- Filling right -----");
            //Debug.Log("Finding Mean of " + rightSideNonManifold.Count + " Non-Manifold Points");
            // find mean
            Vector3 rightCtrPt = new Vector3(0, 0, 0);
            for (int i = 0; i < rightSideNonManifold.Count; i++)
            {
                rightCtrPt += rightSideVerts[rightSideNonManifold[i]];
                //Debug.Log("Add " + rightSideVerts[rightSideNonManifold[i]]);

            }
            rightCtrPt = rightCtrPt / (float)rightSideNonManifold.Count;
            //Debug.Log("Fan fill point is " + rightCtrPt);

            // add the new vert
            rightSideVerts.Add(rightCtrPt);
            int rightCtrIdx = nVertsRight++;

            // make tris from all non-manifold edges
            for (int i = 0; i < rightSideNonManifold.Count; i += 2)
            {
                //view direction from right is  plane normal.  
                Vector3 viewDir = planeNormal;
                //winding direction is normally [i] -> [i+1], but may need a flip.
                Vector3 sideA = rightSideVerts[rightSideNonManifold[i]] - rightCtrPt;
                Vector3 sideB = rightSideVerts[rightSideNonManifold[i + 1]] - rightCtrPt;
                Vector3 faceNormal = Vector3.Cross(sideA, sideB);

                bool swap = (Vector3.Dot(faceNormal, viewDir) < 0);


                rightSideTris.Add(rightCtrIdx);
                rightSideTris.Add(rightSideNonManifold[swap ? i + 1 : i]);
                rightSideTris.Add(rightSideNonManifold[swap ? i : i + 1]);
            }
        }

        //Debug.Log("-----------");
        //Debug.Log("Left Side contains " + leftSideVerts.Count + " verts");

        // Re-center the meshes: Without this the center point will be outside of a split asteroid's volume.
        Vector3 leftMeanPt = new Vector3(0, 0, 0);
        for(int i = 0; i < leftSideVerts.Count; i++)
        {
            leftMeanPt += leftSideVerts[i];
            //Debug.Log("Add point " + leftSideVerts[i]);
        }
        leftMeanPt = leftMeanPt / leftSideVerts.Count;
        //Debug.Log("Mean point is " + leftMeanPt);

        for (int i = 0; i < leftSideVerts.Count; i++)
            leftSideVerts[i] -= leftMeanPt;

        //Debug.Log("Right Side contains " + rightSideVerts.Count + " verts");

        Vector3 rightMeanPt = new Vector3(0, 0, 0);
        for (int i = 0; i < rightSideVerts.Count; i++)
        {
            rightMeanPt += rightSideVerts[i];
            //Debug.Log("Add point " + rightSideVerts[i]);
        }
        rightMeanPt = rightMeanPt / rightSideVerts.Count;
        //Debug.Log("Mean point is " + rightMeanPt);
        for (int i = 0; i < rightSideVerts.Count; i++)
            rightSideVerts[i] -= rightMeanPt;

        // Build game objects (if they're not degenerate geometry).
        GameObject leftSideObject = null;
        GameObject rightSideObject = null;

        if(nVertsLeft >= 4)
        {
            leftSideObject = GameObject.Instantiate(prefabAsteroid) as GameObject;
            leftSideObject.transform.position = baseObj.transform.position + baseObj.transform.TransformDirection(leftMeanPt);
            leftSideObject.transform.rotation = baseObj.transform.rotation;

            // Give it a bit of a push away from the other half.
            leftSideObject.GetComponent<Rigidbody>().velocity = baseObj.GetComponent<Rigidbody>().velocity + baseObj.transform.TransformDirection(planeNormal * 0.50f);

            Vector3[] leftFinalVerts = leftSideVerts.ToArray();
            int[] leftFinalTris = leftSideTris.ToArray();

            Mesh m = leftSideObject.GetComponent<MeshFilter>().mesh;
            m.Clear();
            m.vertices = leftFinalVerts;
            m.triangles = leftFinalTris;

            m.RecalculateBounds();
            m.RecalculateNormals();

            leftSideObject.name = "Asteroid";

            leftSideObject.GetComponent<AsteroidController>().randomizeOnLoad = false;
        }


        if(nVertsRight >= 4)
        {
            rightSideObject = GameObject.Instantiate(prefabAsteroid) as GameObject;
            rightSideObject.transform.position = baseObj.transform.position + baseObj.transform.TransformDirection(rightMeanPt);
            rightSideObject.transform.rotation = baseObj.transform.rotation;

            // Give it a bit of a push away from the other half.
            rightSideObject.GetComponent<Rigidbody>().velocity = baseObj.GetComponent<Rigidbody>().velocity + baseObj.transform.TransformDirection( planeNormal * -0.50f);

            Vector3[] rightFinalVerts = rightSideVerts.ToArray();
            int[] rightFinalTris = rightSideTris.ToArray();

            Mesh m = rightSideObject.GetComponent<MeshFilter>().mesh;
            m.Clear();
            m.vertices = rightFinalVerts;
            m.triangles = rightFinalTris;

            m.RecalculateBounds();
            m.RecalculateNormals();
            rightSideObject.name = "Asteroid";

            rightSideObject.GetComponent<AsteroidController>().randomizeOnLoad = false;
        }

        return new GameObject[] { leftSideObject, rightSideObject};
    }

}
