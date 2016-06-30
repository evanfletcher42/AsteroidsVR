using UnityEngine;
using System.Collections;

public class AsteroidSpawner : MonoBehaviour {

    private static GameObject prefabAsteroid;


    // Use this for initialization
    void Start () {
        prefabAsteroid = Resources.Load("Asteroid") as GameObject;
        for (int i = 0; i < 30; i++)
        {
            GameObject asteroid = Instantiate(prefabAsteroid, Random.insideUnitSphere * 1.0f + new Vector3(0, 2.0f, 0), Quaternion.identity) as GameObject;
        }
	}
	
	// Update is called once per frame
	void Update () {
	
	}
}
