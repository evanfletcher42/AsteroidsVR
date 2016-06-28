using UnityEngine;
using System.Collections;

public class projectileController : MonoBehaviour {

	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
	
	}

    float lifetime = 0.5f;
    void Awake()
    {
        Destroy(gameObject, lifetime);
    }
}
