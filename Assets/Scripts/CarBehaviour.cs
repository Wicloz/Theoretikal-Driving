using UnityEngine;
using System.Collections.Generic;
using UnityStandardAssets.Vehicles.Car;

public class CarBehaviour : MonoBehaviour {
    private CarController carScript;
    public List<GameObject> path = new List<GameObject>();

	// Use this for initialization
	void Start () {
        carScript = this.GetComponent<CarController>();
	}
	
	// Update is called once per frame
	void Update () {
	
	}
}
