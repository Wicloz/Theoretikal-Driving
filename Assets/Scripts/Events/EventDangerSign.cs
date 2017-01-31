using System.Collections.Generic;
using UnityEngine;

public class EventDangerSign : EventSpeed {
    public static List<GameObject> dangerSigns = null;
    public List<GameObject> _dangerSigns = new List<GameObject>();

    public EventDangerSign () {
        if (dangerSigns == null)
            dangerSigns = _dangerSigns;
    }

    void Start () {
        roadNode.tileScript.SpawnTrafficSign(dangerSigns[Random.Range(0, dangerSigns.Count)], roadNode.tileScript.userEntrance.dir, direction.left);
    } 
}
