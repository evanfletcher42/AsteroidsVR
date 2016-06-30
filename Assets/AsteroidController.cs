using UnityEngine;
using System.Collections;

//TODO: Break into smaller asteroids that move faster.  

public class AsteroidController : MonoBehaviour {

    Mesh mesh;
    Vector3[] verts;

    public bool randomizeOnLoad = true;

    // Temp bounds for wrapping asteroids.
    // TODO: this should be dynamically a bit bigger than the SteamVR play area
    private const float minX = -2, maxX = 2;
    private const float minY = 0.2f, maxY = 2;
    private const float minZ = -2, maxZ = 2;

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
            rb.velocity = Random.insideUnitSphere * 0.5f;
        }
    }
	
	// Update is called once per frame
	void Update () {

        Rigidbody rb = GetComponent<Rigidbody>();
        Vector3 position = rb.position;

        if (position.x < minX) position.x = maxX;
        if (position.x > maxX) position.x = minX;

        if (position.y < minY) position.y = maxY;
        if (position.y > maxY) position.y = minY;

        if (position.z < minZ) position.z = maxZ;
        if (position.z > maxZ) position.z = minZ;

        rb.position = position;

    }

    // Called by an impacting projectile.
    public void Split(Vector3 worldContactPoint, Vector3 projectileDirection)
    {
        // get the normal of the plane formed by
        //  - the projectile direction @ the contact point
        //  - the local transform to the contact point

        Vector3 centerToLocalContact = transform.InverseTransformPoint(worldContactPoint);
        Vector3 localContactDirection = transform.InverseTransformDirection(projectileDirection);
        Vector3 planeNormal = Vector3.Cross(centerToLocalContact, localContactDirection);

        GameObject[] children = AsteroidSplitter.Cut(gameObject, centerToLocalContact, planeNormal.normalized);
        Destroy(gameObject);
    }
}
