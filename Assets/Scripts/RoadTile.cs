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

public class RoadTile : MonoBehaviour {
    public List<orientation> orientations = new List<orientation>();
    public List<RoadTilePath> paths = new List<RoadTilePath>();
    public List<direction> shape = new List<direction>();

    private int maxspeed;
    private Dictionary<direction, RoadTile> links = new Dictionary<direction, RoadTile>();

    // Use this for initialization
    void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
	
	}
}
