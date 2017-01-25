using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class TileListItem {
    public GameObject tile;
    public RoadTile tileScript;
    [Range(0, 100)]
    public int chance = 50;
}

public class RoadTreeNode {
    public GameObject tile;
    public List<GameObject> children = new List<GameObject>();

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
    private int maxSpeed = 30;
    private GameObject lastTileHit = null;

    void Awake () {
        GameObject startRoad = GameObject.Find("StartRoad");
        startRoad.GetComponent<RoadTile>().SetUserExit(direction.forward);
        roadTree.Add(new RoadTreeNode(startRoad));
    }

    void Update () {
        if (!CarRayCaster._static.FrontViewFull())
            SpawnNextTile();

        GameObject hitTile = CarRayCaster._static.TileStraightBack();
        if (lastTileHit != null && lastTileHit != hitTile) {
            foreach (RoadTreeNode item in roadTree) {
                item.Remove();
                if (item.tile == lastTileHit)
                    break;
            }
        }
        lastTileHit = hitTile;
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
        AddToUserPath(userPath.ghosts, maxSpeed);
        nextTileScript.SetUserExit(userPath.exit);

        RoadTreeNode nextNode = new RoadTreeNode(nextTile);
        roadTree.Add(nextNode);
    }

    private void AddToUserPath(List<GameObject> ghosts, int speed) {
        List<PathNode> path = new List<PathNode>();
        foreach (GameObject item in ghosts) {
            PathNode node = new PathNode();
            node.ghost = item;
            node.targetspeed = speed;
            path.Add(node);
        }
        CarBehaviour._static.path.AddRange(path);
    }

    private Vector3 RotateOffset (Vector3 offset, Vector3 rotation) {
        float radians = Mathf.Deg2Rad * (rotation.y + rotation.z);
        return offset.RotateAroundY(radians);
    }
}
