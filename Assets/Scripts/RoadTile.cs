using UnityEngine;
using System.Collections.Generic;
using System;

public enum direction {forward, back, left, right};

[Serializable]
public struct orientation {
    public direction entance;
    public Vector3 rotation;
    public Vector3 offset;
}

[Serializable]
public struct RoadTilePath {
    public direction entrance;
    public direction exit;
    public List<GameObject> ghosts;
}

public class RoadTile : MonoBehaviour {
    public List<orientation> orientations = new List<orientation>();
    public List<RoadTilePath> paths = new List<RoadTilePath>();
    public List<direction> shape = new List<direction>();

    private int maxspeed;
    private Dictionary<direction, RoadTile> links = new Dictionary<direction, RoadTile>();
}
