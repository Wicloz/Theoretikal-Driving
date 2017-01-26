using UnityEngine;
using System.Collections.Generic;
using UnityStandardAssets.Vehicles.Car;

public enum trafficzone {woonwijk, dertig, bebouwd, onbebouwd};

[System.Serializable]
public struct PathNode {
    public GameObject ghost;
    public float targetSpeed;
    public trafficzone trafficZone;

    public PathNode (trafficzone trafficZone, GameObject ghost, float targetSpeed) {
        this.trafficZone = trafficZone;
        this.ghost = ghost;
        this.targetSpeed = targetSpeed;
    }
}

[System.Serializable]
public struct SpeedMaterial {
    public Material mat;
    public int maxSpeed;
}

public class CarBehaviour : MonoBehaviour {
    public static CarBehaviour _static = null;
    private CarController carScript;
    private Rigidbody carRigid;
    private MeshRenderer carSpeedHud;

    private List<PathNode> path = new List<PathNode>();
    private bool breakToTarget = false;

    public float breakTreshhold = 42;
    public float realSpeedMult = 1;
    public List<SpeedMaterial> speedSigns = new List<SpeedMaterial>();

    void Awake () {
        if (_static == null)
            _static = this;
        carScript = GetComponent<CarController>();
        carRigid = GetComponent<Rigidbody>();
        carSpeedHud = transform.FindChild("SpeedHud").GetComponent<MeshRenderer>();
    }

    void Start () {
        SetSpeedHud();
    }

    void FixedUpdate () {
        if (path.Count > 0) {
            Transform currentTransform = gameObject.transform;
            float currentSpeed = carRigid.velocity.magnitude;

            Transform targetTransform = path[0].ghost.transform;
            float targetSpeed = path[0].targetSpeed * realSpeedMult;

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
            SetSpeedHud();
        }
    }

    public void AddToPath (List<PathNode> newNodes) {
        path.AddRange(newNodes);
    }

    public void AddToPath (PathNode newNode) {
        path.Add(newNode);
    }

    private void SetSpeedHud () {
        float maxSpeed = 0;
        if (path.Count > 0)
            maxSpeed = path[0].targetSpeed;

        int bestSingIndex = -1;
        for (int i = 0; i < speedSigns.Count; i++) {
            if (speedSigns[i].maxSpeed >= maxSpeed && (bestSingIndex < 0 || speedSigns[i].maxSpeed < speedSigns[bestSingIndex].maxSpeed))
                bestSingIndex = i;
        }

        if (bestSingIndex >= 0) {
            carSpeedHud.material = speedSigns[bestSingIndex].mat;
        }
    }
}
