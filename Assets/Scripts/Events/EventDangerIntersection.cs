using System.Collections.Generic;
using UnityEngine;

public class EventDangerIntersection : EventSpeed {
    public static GameObject dangerSign = null;
    public GameObject _dangerSign;

    public EventDangerIntersection () {
        if (dangerSign == null)
            dangerSign = _dangerSign;
    }

    void Start () {
        roadNode.tileScript.SpawnTrafficSign(dangerSign, roadNode.tileScript.userEntrance.dir, direction.left);
    }
}
