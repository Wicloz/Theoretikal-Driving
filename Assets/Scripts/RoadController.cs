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
    private int initTimeout = 30;
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
    private List<RoadTreeNode> roadTree = new List<RoadTreeNode>();
    public List<RoadTileHit> tilesHit = new List<RoadTileHit>();
    public int eventDelay = 5;
    private int currentEventDelay = 5;
    private trafficzone currentTrafficZone = trafficzone.woonwijk;

    void Awake () {
        GameObject startRoad = GameObject.Find("StartRoad");
        startRoad.GetComponent<RoadTile>().SetUserExit(direction.forward);
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

        RoadTilePath userPath = nextTileScript.GetRandomPath(orientation.entance);
        AddToUserPath(userPath.ghosts);
        nextTileScript.SetUserExit(userPath.exit);

        RoadTreeNode nextNode = new RoadTreeNode(nextTile);
        roadTree.Add(nextNode);

        if ((nextNode.tileScript.eventMandatory || currentEventDelay <= 0) && nextNode.tileScript.events.Count > 0) {
            HandleTileEvent(nextNode);
            currentEventDelay = eventDelay;
        } else {
            currentEventDelay--;
        }
    }

    private void HandleTileEvent (RoadTreeNode node) {
        node.tile.AddComponent(node.tileScript.GetRandomEvent().GetType());
    }

    private void AddToUserPath(List<GameObject> ghosts) {
        float targetspeed = currentTrafficZone.MaxSpeed();

        List<PathNode> path = new List<PathNode>();
        foreach (GameObject item in ghosts) {
            PathNode node = new PathNode(currentTrafficZone, item, targetspeed);
            path.Add(node);
        }
        CarBehaviour._static.path.AddRange(path);
    }

    private Vector3 RotateOffset (Vector3 offset, Vector3 rotation) {
        float radians = Mathf.Deg2Rad * (rotation.y + rotation.z);
        return offset.RotateAroundY(radians);
    }
}
