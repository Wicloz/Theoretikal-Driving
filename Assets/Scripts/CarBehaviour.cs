using UnityEngine;
using System.Collections.Generic;
using UnityStandardAssets.Vehicles.Car;
using UnityEngine.UI;

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
    private MeshRenderer carZoneHud;
    private Text carSpeedHud;
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
        carZoneHud = transform.FindChild("ZoneHud").GetComponent<MeshRenderer>();
        carSpeedHud = transform.FindChild("SpeedHud").FindChild("Text").GetComponent<Text>();
    }

    void Start () {
        SetZoneHud();
    }

    void FixedUpdate () {
        if (path.Count > 0) {
            Transform currentTransform = gameObject.transform;
            float currentSpeed = carRigid.velocity.magnitude / realSpeedMult;

            Transform targetTransform = path[0].ghost.transform;
            float targetSpeed = path[0].targetSpeed;
            for (int i = 1; i < 5; i++) {
                if (path.Count > i && Vector3.Distance(path[i].ghost.transform.position, currentTransform.position) < (currentSpeed - path[i].targetSpeed) * 3)
                    targetSpeed = Mathf.Min(targetSpeed, path[i].targetSpeed);
            }

            float acceleration = targetSpeed - currentSpeed;
            float deceleration = currentSpeed - targetSpeed;

            if (deceleration >= breakTreshhold || Vector3.Distance(currentTransform.position, targetTransform.position) < deceleration)
                breakToTarget = true;
            if (currentSpeed <= targetSpeed)
                breakToTarget = false;

            GameObject look = new GameObject();
            look.transform.position = currentTransform.position;
            look.transform.rotation = currentTransform.rotation;
            look.transform.LookAt(targetTransform);
            float direction = Mathf.Sin(Mathf.Deg2Rad * (look.transform.rotation.eulerAngles.y - currentTransform.rotation.eulerAngles.y));
            Destroy(look);

            carScript.Move(direction, acceleration, breakToTarget ? -1 : 0, 0);
        }

        else {
            carScript.Move(0, 0, 0, 1);
        }

        SetSpeedHud();
    }

    void OnTriggerEnter (Collider other) {
        if (path.Count > 0 && other.gameObject == path[0].ghost) {
            path.RemoveAt(0);
            SetZoneHud();
        }
    }

    private void SetZoneHud () {
        float maxSpeed = 0;
        if (path.Count > 0)
            maxSpeed = path[0].trafficZone.MaxSpeed();

        int bestSingIndex = -1;
        for (int i = 0; i < speedSigns.Count; i++) {
            if (speedSigns[i].maxSpeed >= maxSpeed && (bestSingIndex < 0 || speedSigns[i].maxSpeed < speedSigns[bestSingIndex].maxSpeed))
                bestSingIndex = i;
        }

        if (bestSingIndex >= 0) {
            carZoneHud.material = speedSigns[bestSingIndex].mat;
        }
    }

    private void SetSpeedHud () {
        carSpeedHud.text = Mathf.Round(carRigid.velocity.magnitude / realSpeedMult).ToString();
    }

    public void AddToPath (List<PathNode> newNodes) {
        path.AddRange(newNodes);
    }

    public void AddToPath (PathNode newNode) {
        path.Add(newNode);
    }
}
