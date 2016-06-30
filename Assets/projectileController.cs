using UnityEngine;
using System.Collections;

public class projectileController : MonoBehaviour {

    private GameObject prefabExplosion;

	// Use this for initialization
	void Start () {
        prefabExplosion = Resources.Load("explosion") as GameObject;
    }

    // Update is called once per frame
    void Update () {
	
	}

    float lifetime = 0.5f;
    void Awake()
    {
        Destroy(gameObject, lifetime);
    }

    private void OnCollisionEnter(Collision collision)
    {
        GameObject ex = Instantiate(prefabExplosion, collision.contacts[0].point, transform.rotation) as GameObject;
        Destroy(ex, 0.25f);
        Destroy(gameObject);

        if(collision.collider.CompareTag("Asteroids"))
        {
            collision.collider.GetComponent<AsteroidController>().Split(collision.contacts[0].point, GetComponent<Rigidbody>().velocity.normalized);
        }
    }
}
