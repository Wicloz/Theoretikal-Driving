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
public struct RoadTreeNode {
    public GameObject tile;
    public List<GameObject> children;
}

public class RoadController : MonoBehaviour {
    public List<TileListItem> prefabTileList = new List<TileListItem>();
    public List<RoadTreeNode> roadTiles = new List<RoadTreeNode>();
    private int maxSpeed = 50;

    public void Update () {
        if (roadTiles.Count < 20)
            SpawnNextTile();
    } 

    private void SpawnNextTile () {
        GameObject lastTile = roadTiles[roadTiles.Count - 1].tile;
        RoadTile lastTileScript = lastTile.GetComponent<RoadTile>();

        int totalChance = 0;
        foreach (TileListItem item in prefabTileList) {
            totalChance += item.chance;
        }
        int randomChance = Random.Range(0, totalChance);
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
        Orientation orientation = nextTileScript.orientations[Random.Range(0, nextTileScript.orientations.Count - 1)];

        nextTile = Instantiate(
            nextTile, 
            lastTile.transform.position + RotateOffset(lastTileScript.userExit.offset, lastTile.transform.rotation.eulerAngles) + RotateOffset(orientation.offset, lastTile.transform.rotation.eulerAngles + lastTileScript.userExit.rotation), 
            Quaternion.Euler(lastTile.transform.rotation.eulerAngles + lastTileScript.userExit.rotation + orientation.rotation)
        );
        nextTileScript = nextTile.GetComponent<RoadTile>();

        RoadTilePath userPath = nextTileScript.GetRandomPath(orientation.entance);
        AddToUserPath(userPath.ghosts, maxSpeed);
        nextTileScript.SetUserExit(userPath.exit);

        RoadTreeNode nextNode = new RoadTreeNode();
        nextNode.tile = nextTile;
        roadTiles.Add(nextNode);
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
        return new Vector3(
            offset.x * Mathf.Cos(radians) + offset.z * Mathf.Sin(radians),
            offset.y,
            -1 * offset.x * Mathf.Sin(radians) + offset.z * Mathf.Cos(radians)
        );
    }
}
