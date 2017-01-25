using UnityEngine;
using System.Collections.Generic;
using UnityStandardAssets.Vehicles.Car;

[System.Serializable]
public struct PathNode {
    public GameObject ghost;
    public float targetspeed;
}

public class CarBehaviour : MonoBehaviour {
    public static CarBehaviour _static = null;
    private CarController carScript;
    private Rigidbody carRigid;
    public List<PathNode> path = new List<PathNode>();
    private bool breakToTarget = false;
    public float breakTreshhold = 42;

	void Awake () {
        if (_static == null)
            _static = this;
        carScript = GetComponent<CarController>();
        carRigid = GetComponent<Rigidbody>();
    }

	void FixedUpdate () {
        if (path.Count > 0) {
            Transform currentTransform = gameObject.transform;
            float currentSpeed = carRigid.velocity.magnitude;

            Transform targetTransform = path[0].ghost.transform;
            float targetSpeed = path[0].targetspeed;

            if (currentSpeed - targetSpeed >= breakTreshhold)
                breakToTarget = true;
            if (currentSpeed <= targetSpeed)
                breakToTarget = false;

            GameObject look = new GameObject();
            look.transform.position = currentTransform.position;
            look.transform.rotation = currentTransform.rotation;
            look.transform.LookAt(targetTransform);
            float direction = Mathf.Sin(Mathf.Deg2Rad * (look.transform.rotation.eulerAngles.y - currentTransform.rotation.eulerAngles.y));
            Destroy(look);
            float speed = targetSpeed - currentSpeed;

            carScript.Move(direction, Mathf.Max(0, speed), breakToTarget ? -1 : 0, 0);
        }

        else {
            carScript.Move(0, 0, 0, 1);
        }
    }

    void OnTriggerEnter (Collider other) {
        if (path.Count > 0 && other.gameObject == path[0].ghost) {
            path.RemoveAt(0);
        }
    }
}
