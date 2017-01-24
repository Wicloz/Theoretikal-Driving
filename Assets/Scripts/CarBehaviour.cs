using UnityEngine;
using System.Collections.Generic;
using UnityStandardAssets.Vehicles.Car;
using System;

[Serializable]
public struct PathNode {
    public GameObject ghost;
    public float targetspeed;
}

public class CarBehaviour : MonoBehaviour {
    private CarController carScript;
    private Rigidbody carRigid;
    public List<PathNode> path = new List<PathNode>();
    private bool breakToTarget = false;

	void Awake () {
        carScript = GetComponent<CarController>();
        carRigid = GetComponent<Rigidbody>();
    }

	void FixedUpdate () {
        Transform currentTransform = gameObject.transform;
        float currentSpeed = carRigid.velocity.magnitude;

        Transform targetTransform = currentTransform;
        float targetSpeed = 0;
        if (path.Count > 0) {
            targetTransform = path[0].ghost.transform;
            targetSpeed = path[0].targetspeed;
        }

        if (currentSpeed - targetSpeed >= 40)
            breakToTarget = true;
        if (currentSpeed <= targetSpeed)
            breakToTarget = false;

        GameObject look = new GameObject();
        look.transform.position = currentTransform.position;
        look.transform.rotation = currentTransform.rotation;
        look.transform.LookAt(targetTransform);
        float direction = look.transform.rotation.eulerAngles.y - currentTransform.transform.eulerAngles.y;
        float speed = targetSpeed - currentSpeed;

        carScript.Move(direction, Mathf.Max(0, speed), breakToTarget ? -1 : 0, 0);
    }

    void OnTriggerEnter (Collider other) {
        if (path.Count > 0 && other.gameObject == path[0].ghost) {
            path.RemoveAt(0);
        }
    }
}
