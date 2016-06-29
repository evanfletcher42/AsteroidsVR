using UnityEngine;
using System.Collections;

//TODO: Break into smaller asteroids that move faster.  

public class AsteroidController : MonoBehaviour {

    Mesh mesh;
    Vector3[] verts;

    public bool randomizeOnLoad = true;

	// Use this for initialization
	void Start () {

        if (randomizeOnLoad)
        {
            mesh = GetComponent<MeshFilter>().mesh;
            verts = mesh.vertices;

            for (int i = 0; i < verts.Length; i++)
            {
                verts[i] = verts[i] + new Vector3(Random.Range(-0.03f, 0.03f), Random.Range(-0.03f, 0.03f), Random.Range(-0.03f, 0.03f));
            }
            mesh.vertices = verts;
            mesh.RecalculateBounds();
            mesh.RecalculateNormals();

            //launch the asteroid in some random direction with (small) velocity
            Rigidbody rb = GetComponent<Rigidbody>();
            rb.velocity = new Vector3(Random.Range(-0.1f, 0.1f), Random.Range(-0.1f, 0.1f), Random.Range(-0.1f, 0.1f));
        }
    }
	
	// Update is called once per frame
	void Update () {

    }

    // Called by an impacting projectile.
    public void Split(Vector3 worldContactPoint, Vector3 projectileDirection)
    {
        // get the normal of the plane formed by
        //  - the projectile direction @ the contact point
        //  - the local transform to the contact point

        Vector3 centerToLocalContact = transform.InverseTransformPoint(worldContactPoint);
        Vector3 localContactDirection = transform.InverseTransformDirection(projectileDirection);

        GameObject[] children = AsteroidSplitter.Cut(gameObject, centerToLocalContact, localContactDirection);
        Destroy(gameObject);
    }
}
