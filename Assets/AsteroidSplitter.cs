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
        //  - Count the number of faces in the left and right side meshes.  
        //      If it's < 3, kill the body entirely. 
        //  - Close the non-manifold edges by placing a vert at the average
        //      location of the non-manifold edges, then traverse around the
        //      non-manifold edge and fill with triangles.
        //
        // This method is expected to reduce the total volume, which is great for 
        // the goal/aesthetic of shooting a rock in Asteroids.

        // Step 1: Separate triangles that exist on different sides of the plane.
        //         Discard triangles that span across the plane.  

        int p1, p2, p3;
        bool side1, side2, side3;
        List<int> leftSideTris = new List<int>();
        List<Vector3> leftSideVerts = new List<Vector3>();
        List<int> rightSideTris = new List<int>();
        List<Vector3> rightSideVerts = new List<Vector3>();

        int nVertsLeft = 0;
        int nVertsRight = 0;

        for (int i = 0; i < baseMesh.triangles.Length; i+=3)
        {
            p1 = baseMesh.triangles[i];
            p2 = baseMesh.triangles[i + 1];
            p3 = baseMesh.triangles[i + 2];

            side1 = splitPlane.GetSide(baseMesh.vertices[p1]);
            side2 = splitPlane.GetSide(baseMesh.vertices[p2]);
            side3 = splitPlane.GetSide(baseMesh.vertices[p3]);

            if (side1 == side2 && side2 == side3)
            {
                if (side1)
                {
                    int nf1 = leftSideVerts.IndexOf(baseMesh.vertices[p1]);
                    int nf2 = leftSideVerts.IndexOf(baseMesh.vertices[p2]);
                    int nf3 = leftSideVerts.IndexOf(baseMesh.vertices[p3]);

                    //Debug.Log("nf1: " + nf1 + " nf2: " + nf2 + " nf3: " + nf3);

                    if(nf1 == -1) {
                        leftSideVerts.Add(baseMesh.vertices[p1]);
                        leftSideTris.Add(nVertsLeft++);
                    } else {
                        leftSideTris.Add(nf1);
                    }

                    if (nf2 == -1) {
                        leftSideVerts.Add(baseMesh.vertices[p2]);
                        leftSideTris.Add(nVertsLeft++);
                    } else {
                        leftSideTris.Add(nf2);
                    }

                    if (nf3 == -1) {
                        leftSideVerts.Add(baseMesh.vertices[p3]);
                        leftSideTris.Add(nVertsLeft++);
                    } else {
                        leftSideTris.Add(nf3);
                    }
                }
                else
                {
                    int nf1 = rightSideVerts.IndexOf(baseMesh.vertices[p1]);
                    int nf2 = rightSideVerts.IndexOf(baseMesh.vertices[p2]);
                    int nf3 = rightSideVerts.IndexOf(baseMesh.vertices[p3]);

                    if (nf1 == -1)
                    {
                        rightSideVerts.Add(baseMesh.vertices[p1]);
                        rightSideTris.Add(nVertsRight++);
                    }
                    else
                    {
                        rightSideTris.Add(nf1);
                    }

                    if (nf2 == -1)
                    {
                        rightSideVerts.Add(baseMesh.vertices[p2]);
                        rightSideTris.Add(nVertsRight++);
                    }
                    else
                    {
                        rightSideTris.Add(nf2);
                    }

                    if (nf3 == -1)
                    {
                        rightSideVerts.Add(baseMesh.vertices[p3]);
                        rightSideTris.Add(nVertsRight++);
                    }
                    else
                    {
                        rightSideTris.Add(nf3);
                    }
                }
            }
        }
        //Debug.Log("nLeft: " + nVertsLeft);
        //Debug.Log("nRight: " + nVertsRight);
        //Debug.Log("original mesh contained: " + baseMesh.vertexCount);

        // Build game objects (if they're not degenerate geometry).
        GameObject leftSideObject = null;
        GameObject rightSideObject = null;

        if(nVertsLeft >= 4)
        {
            leftSideObject = GameObject.Instantiate(prefabAsteroid) as GameObject;
            leftSideObject.transform.position = baseObj.transform.position;
            leftSideObject.transform.rotation = baseObj.transform.rotation;

            // Give it a bit of a push away from the other half.
            leftSideObject.GetComponent<Rigidbody>().velocity = baseObj.GetComponent<Rigidbody>().velocity + baseObj.transform.TransformDirection(planeNormal * 1.0f);

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
            rightSideObject.transform.position = baseObj.transform.position;
            rightSideObject.transform.rotation = baseObj.transform.rotation;

            // Give it a bit of a push away from the other half.
            rightSideObject.GetComponent<Rigidbody>().velocity = baseObj.GetComponent<Rigidbody>().velocity + baseObj.transform.TransformDirection( planeNormal * -1.0f);

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
