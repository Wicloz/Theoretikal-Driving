using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class TileListItem {
    public GameObject tile;
    public RoadTile tileScript;
    [Range(0, 100)]
    public int chance = 50;
}

[System.Serializable]
public class RoadTileHit {
    private int initTimeout = 60;
    public GameObject tile;
    public int timeout;

    public RoadTileHit (GameObject tile) {
        this.tile = tile;
        ResetTimeout();
    }

    public void ResetTimeout () {
        timeout = initTimeout;
    }
}

public class RoadTreeNode {
    public GameObject tile;
    public List<GameObject> children = new List<GameObject>();
    private RoadTile _tileScript = null;
    public RoadTile tileScript {
        get {
            if (_tileScript == null)
                _tileScript = tile.GetComponent<RoadTile>();
            return _tileScript;
        }
    }

    public RoadTreeNode () { }
    public RoadTreeNode (GameObject tile) {
        this.tile = tile;
    }

    public void Remove () {
        foreach (GameObject child in children) {
            GameObject.Destroy(child);
        }
        GameObject.Destroy(tile);
    }
}

public class RoadController : MonoBehaviour {
    public List<TileListItem> prefabTileList = new List<TileListItem>();
    public int eventDelay = 5;
    public int zoneSwitchDelayMin = 6;
    public int zoneSwitchDelayMax = 14;

    private List<RoadTileHit> tilesHit = new List<RoadTileHit>();
    private List<RoadTreeNode> roadTree = new List<RoadTreeNode>();
    private int currentEventDelay;
    private int currentZoneSwitchDelay;
    private trafficzone currentTrafficZone = trafficzone.woonwijk;

    void Awake () {
        currentEventDelay = eventDelay;
        ResetZoneSwitchDelay();
        GameObject startRoad = GameObject.Find("StartRoad");
        RoadTile startRoadScript = startRoad.GetComponent<RoadTile>();
        startRoadScript.SetUserExit(direction.forward);
        AddToUserPath(startRoadScript.GetRandomPath(direction.back).ghosts);
        roadTree.Add(new RoadTreeNode(startRoad));
    }

    void Update () {
        if (!CarRayCaster._static.FrontViewFull())
            SpawnNextTile();

        GameObject hitTile = CarRayCaster._static.TileStraightBack();
        if (hitTile != null) {
            bool added = false;
            foreach (RoadTileHit item in tilesHit) {
                if (item.tile == hitTile) {
                    item.ResetTimeout();
                    added = true;
                    break;
                }
            }
            if (!added)
                tilesHit.Add(new RoadTileHit(hitTile));
        }

        bool deleted = false;
        for (int i = 0; i < tilesHit.Count; i++) {
            tilesHit[i].timeout--;
            if (!deleted && tilesHit[i].timeout < 0) {
                if (tilesHit[i].tile != null) {
                    foreach (RoadTreeNode node in roadTree) {
                        node.Remove();
                        if (node.tile == tilesHit[i].tile)
                            break;
                    }
                }
                tilesHit.RemoveAt(i);
                deleted = true;
            }
        }
    } 

    private void SpawnNextTile () {
        if (currentZoneSwitchDelay <= 0) {
            if (currentTrafficZone == trafficzone.woonwijk)
                currentTrafficZone++;
            else if (currentTrafficZone == trafficzone.onbebouwd)
                currentTrafficZone--;
            else {
                if (Random.Range(0, 2) == 0)
                    currentTrafficZone++;
                else
                    currentTrafficZone--;
            }
            ResetZoneSwitchDelay();
        } else {
            currentZoneSwitchDelay--;
        }

        GameObject lastTile = roadTree[roadTree.Count - 1].tile;
        RoadTile lastTileScript = lastTile.GetComponent<RoadTile>();

        int totalChance = 0;
        foreach (TileListItem item in prefabTileList) {
            totalChance += item.chance;
        }
        int randomChance = Random.Range(0, totalChance + 1);
        GameObject nextTile = null;
        RoadTile nextTileScript = null;
        foreach (TileListItem item in prefabTileList) {
            randomChance -= item.chance;
            if (randomChance <= 0) {
                nextTile = item.tile;
                nextTileScript = item.tileScript;
                break;
            }
        }
        RoadTileOrientation orientation = nextTileScript.orientations[Random.Range(0, nextTileScript.orientations.Count)];

        nextTile = Instantiate(
            nextTile, 
            lastTile.transform.position + RotateOffset(lastTileScript.userExit.offset, lastTile.transform.rotation.eulerAngles) + RotateOffset(orientation.offset, lastTile.transform.rotation.eulerAngles + lastTileScript.userExit.rotation), 
            Quaternion.Euler(lastTile.transform.rotation.eulerAngles + lastTileScript.userExit.rotation + orientation.rotation)
        );
        nextTileScript = nextTile.GetComponent<RoadTile>();

        nextTileScript.SetTrafficZone(currentTrafficZone);
        RoadTilePath userPath = nextTileScript.GetRandomPath(orientation.entance);
        AddToUserPath(userPath.ghosts);
        nextTileScript.SetUserExit(userPath.exit);

        RoadTreeNode nextNode = new RoadTreeNode(nextTile);
        roadTree.Add(nextNode);

        if ((nextNode.tileScript.eventMandatory || currentEventDelay <= 0) && nextNode.tileScript.events.Count > 0) {
            nextNode.tile.AddComponent(nextNode.tileScript.GetRandomEvent().GetType());
            currentEventDelay = eventDelay;
        } else {
            currentEventDelay--;
        }
    }

    private void AddToUserPath(List<GameObject> ghosts) {
        foreach (GameObject item in ghosts) {
            PathNode node = new PathNode(currentTrafficZone, item, currentTrafficZone.MaxSpeed());
            CarBehaviour._static.AddToPath(node);
        }
    }

    private void ResetZoneSwitchDelay () {
        currentZoneSwitchDelay = Random.Range(zoneSwitchDelayMin, zoneSwitchDelayMax + 1);
    }

    private Vector3 RotateOffset (Vector3 offset, Vector3 rotation) {
        float radians = Mathf.Deg2Rad * (rotation.y + rotation.z);
        return offset.RotateAroundY(radians);
    }
}
