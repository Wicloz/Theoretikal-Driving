using UnityEngine;
using System.Collections.Generic;

public enum direction {forward, back, left, right, center};

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
    public int maxSpeed;
}

[System.Serializable]
public struct RoadTileExit {
    public direction dir;
    public Vector3 rotation;
    public Vector3 offset;
    public List<GameObject> topShields;
}

[System.Serializable]
public class RoadTileEvent {
    public EventMain eventScript;
    [Range(0, 100)]
    public int chance = 50;
    public List<trafficzone> targetZones;
}

public class RoadTile : MonoBehaviour {
    public List<RoadTileOrientation> orientations = new List<RoadTileOrientation>();
    public List<RoadTilePath> paths = new List<RoadTilePath>();
    public List<RoadTileExit> exits = new List<RoadTileExit>();
    public List<RoadTileEvent> events = new List<RoadTileEvent>();

    private trafficzone _trafficZone;
    public trafficzone trafficZone {
        get {
            return _trafficZone;
        }
    }
    private RoadTileExit _userExit;
    public RoadTileExit userExit {
        get {
            return _userExit;
        }
    }
    private RoadTileExit _userEntrance;
    public RoadTileExit userEntrance {
        get {
            return _userEntrance;
        }
    }

    public List<GameObject> CapUnusedRoads () {
        List<GameObject> roads = new List<GameObject>();
        foreach (RoadTileExit item in exits) {
            if (item.dir != userExit.dir && item.dir != userEntrance.dir) {
                roads.Add(Instantiate(
                    RoadController._static.endRoad,
                    transform.position + item.offset.RotateOffset(transform.rotation.eulerAngles),
                    Quaternion.Euler(transform.rotation.eulerAngles + item.rotation)
                ));
            }
        }
        return roads;
    }

    private Vector3 GetWorldSignPos (RoadTileExit exit, direction side) {
        Vector3 position = transform.position + exit.offset.RotateOffset(transform.rotation.eulerAngles);
        if (side == direction.right)
            position += new Vector3(6, 0, 0).RotateOffset(exit.rotation + transform.rotation.eulerAngles);
        else if (side == direction.left)
            position += new Vector3(-6, 0, 0).RotateOffset(exit.rotation + transform.rotation.eulerAngles);
        return position;
    }

    private Quaternion GetWorldSignRot (RoadTileExit exit, direction side) {
        Vector3 rotation = transform.rotation.eulerAngles + exit.rotation + new Vector3(90, 0, 0);
        if (side == direction.left)
            rotation.y += 180;
        return Quaternion.Euler(rotation);
    }

    public void SpawnTrafficSign (GameObject sign, direction exit, direction side) {
        RoadTileExit betterExit = new RoadTileExit();
        if (exit == direction.center) {
            betterExit.offset = Vector3.zero;
            betterExit.rotation = Vector3.zero;
            if (userEntrance.dir == direction.forward) {
                switch (side) {
                    case direction.left:
                        side = direction.right;
                        break;
                    case direction.right:
                        side = direction.left;
                        break;
                }
            }
        }

        else {
            foreach (RoadTileExit item in exits) {
                if (exit == item.dir) {
                    betterExit = item;
                    break;
                }
            }
        }

        Instantiate(sign, GetWorldSignPos(betterExit, side), GetWorldSignRot(betterExit, side), transform);
    }

    public void SetUserExit (direction exit) {
        foreach (RoadTileExit item in exits) {
            if (item.dir == exit) {
                _userExit = item;
                foreach (GameObject shield in item.topShields) {
                    Destroy(shield);
                }
                break;
            }
        }
    }

    public void SetUserEntrance (direction entrance) {
        foreach (RoadTileExit item in exits) {
            if (item.dir == entrance) {
                _userEntrance = item;
                foreach (GameObject shield in item.topShields) {
                    Destroy(shield);
                }
                break;
            }
        }
    }

    public void SetTrafficZone (trafficzone trafficZone) {
        _trafficZone = trafficZone;
    }

    public RoadTilePath GetRandomPath (direction entrance, int speed) {
        List<RoadTilePath> validPaths = new List<RoadTilePath>();
        foreach (RoadTilePath item in paths) {
            if (item.entrance == entrance && item.maxSpeed >= speed)
                validPaths.Add(item);
        }
        return validPaths[Random.Range(0, validPaths.Count)];
    }

    public EventMain GetRandomEvent () {
        int totalChance = 0;
        foreach (RoadTileEvent item in events) {
            if (item.targetZones.Contains(trafficZone))
                totalChance += item.chance;
        }

        int randomChance = Random.Range(0, totalChance + 1);
        foreach (RoadTileEvent item in events) {
            if (item.targetZones.Contains(trafficZone)) {
                randomChance -= item.chance;
                if (randomChance <= 0)
                    return item.eventScript;
            }
        }

        return null;
    }
}
