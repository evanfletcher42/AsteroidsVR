using UnityEngine;
using System.Collections;

//TODO: Break into smaller asteroids that move faster.  

public class AsteroidController : MonoBehaviour {

    Mesh mesh;
    Vector3[] verts;

	// Use this for initialization
	void Start () {
        mesh = GetComponent<MeshFilter>().mesh;
        verts = mesh.vertices;

        Debug.Log("Mesh has " + mesh.vertexCount + " vertices.");

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
	
	// Update is called once per frame
	void Update () {

    }
}
