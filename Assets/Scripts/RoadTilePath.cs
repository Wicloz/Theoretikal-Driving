using UnityEngine;
using System.Collections.Generic;
using System;

[Serializable]
public class RoadTilePath {
    public direction entrance;
    public direction exit;
    public List<GameObject> ghosts;
}
