using UnityEngine;
using System.Collections.Generic;

public enum direction {forward, back, left, right};

[System.Serializable]
public struct RoadTileOrientation {
    public direction entance;
    public Vector3 rotation;
    public Vector3 offset;
}

[System.Serializable]
public struct RoadTilePath {
    public direction entrance;
    public direction exit;
    public List<GameObject> ghosts;
}

[System.Serializable]
public struct RoadTileExit {
    public direction dir;
    public Vector3 rotation;
    public Vector3 offset;
}

public class RoadTile : MonoBehaviour {
    public List<RoadTileOrientation> orientations = new List<RoadTileOrientation>();
    public List<RoadTilePath> paths = new List<RoadTilePath>();
    public List<RoadTileExit> exits = new List<RoadTileExit>();
    public RoadTileExit userExit;

    public RoadTilePath GetRandomPath (direction entrance) {
        List<RoadTilePath> validPaths = new List<RoadTilePath>();
        foreach (RoadTilePath item in paths) {
            if (item.entrance == entrance)
                validPaths.Add(item);
        }
        return validPaths[Random.Range(0, validPaths.Count)];
    }

    public void SetUserExit (direction exit) {
        foreach (RoadTileExit item in exits) {
            if (item.dir == exit) {
                userExit = item;
                break;
            }
        }
    }
}
