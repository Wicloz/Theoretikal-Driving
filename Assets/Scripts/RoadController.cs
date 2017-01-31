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
public class SpeedSignItem {
    public GameObject sign;
    public int speed;
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
    public EventMain eventScript = null;

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
    public List<SpeedSignItem> speedSignList = new List<SpeedSignItem>();
    public int eventDelay = 5;
    public int zoneSwitchDelay;
    public int zoneSwitchDelayFuzz;

    public static RoadController _static = null;
    private List<RoadTileHit> tilesHit = new List<RoadTileHit>();
    private List<RoadTreeNode> roadTree = new List<RoadTreeNode>();
    private int currentEventDelay;
    private int currentZoneSwitchDelay;
    private trafficzone currentTrafficZone = trafficzone.woonwijk;
    public bool gameRunning = true;

    void Awake () {
        if (_static == null)
            _static = this;
        currentEventDelay = eventDelay;
        ResetZoneSwitchDelay();
        GameObject startRoad = GameObject.Find("StartRoad");
        RoadTile startRoadScript = startRoad.GetComponent<RoadTile>();
        startRoadScript.SetUserExit(direction.forward);
        AddToUserPath(startRoadScript.GetRandomPath(direction.back, currentTrafficZone.MaxSpeed()));
        roadTree.Add(new RoadTreeNode(startRoad));
    }

    void Update () {
        if (gameRunning) {
            // Handle spawning tiles
            if (!CarRayCaster._static.FrontViewFull())
                SpawnNextTile();

            // Handle question things
            HandleQuestions();

            // Manage the tile hit list
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

            // Delete old tiles
            bool deleted = false;
            for (int i = 0; i < tilesHit.Count; i++) {
                tilesHit[i].timeout--;
                if (!deleted && tilesHit[i].timeout < 0) {
                    if (tilesHit[i].tile != null) {
                        for (int j = 0; j < roadTree.Count; j++) {
                            RoadTreeNode tempNode = roadTree[i];
                            roadTree[i].Remove();
                            roadTree.RemoveAt(i);
                            if (tempNode.tile == tilesHit[i].tile)
                                break;
                        }
                    }
                    tilesHit.RemoveAt(i);
                    deleted = true;
                }
            }
        }
    } 

    private void HandleQuestions () {
        int currentCarPos = GetCurrentCarPos();

        if (!QuestionHandler._static.questionActive) {
            for (int i = currentCarPos; i < Mathf.Min(currentCarPos + 3, roadTree.Count); i++) {
                if (roadTree[i].eventScript != null) {
                    roadTree[i].eventScript.StartQuestion();
                    break;
                }
            }
        }

        else if (roadTree.Count > currentCarPos + 1 && roadTree[currentCarPos + 1].eventScript != null && Vector3.Distance(CarBehaviour._static.transform.position, roadTree[currentCarPos + 1].tile.transform.position + roadTree[currentCarPos + 1].tileScript.userEntrance.offset.RotateOffset(roadTree[currentCarPos + 1].tile.transform.rotation.eulerAngles)) < 6)
            roadTree[currentCarPos + 1].eventScript.EndQuestion();
    }

    private void SpawnNextTile () {
        // Has event
        bool hasEvent = currentEventDelay <= 0;

        // Switch zones when needed
        bool zoneSwitched = false;
        if (currentZoneSwitchDelay <= 0 && !hasEvent) {
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
            zoneSwitched = true;
        } else {
            currentZoneSwitchDelay--;
        }

        // Find the previous tile
        GameObject lastTile = roadTree[roadTree.Count - 1].tile;
        RoadTile lastTileScript = lastTile.GetComponent<RoadTile>();

        // Select the next tile
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

        // Select an orientation
        RoadTileOrientation orientation = nextTileScript.orientations[Random.Range(0, nextTileScript.orientations.Count)];

        // Instantiate the next tile
        nextTile = Instantiate(
            nextTile, 
            lastTile.transform.position + lastTileScript.userExit.offset.RotateOffset(lastTile.transform.rotation.eulerAngles) + orientation.offset.RotateOffset(lastTile.transform.rotation.eulerAngles + lastTileScript.userExit.rotation), 
            Quaternion.Euler(lastTile.transform.rotation.eulerAngles + lastTileScript.userExit.rotation + orientation.rotation)
        );

        // Create and add a node for the tile
        RoadTreeNode nextNode = new RoadTreeNode(nextTile);
        roadTree.Add(nextNode);

        // Set variables on new tile
        nextNode.tileScript.SetTrafficZone(currentTrafficZone);
        nextNode.tileScript.SetUserEntrance(orientation.entance);

        // Create speed sign if needed
        if (zoneSwitched)
            nextNode.tileScript.SpawnTrafficSign(BestSpeedSign(currentTrafficZone.MaxSpeed()), orientation.entance, direction.left);

        // Set events and car paths
        SetTileEvent(nextNode, hasEvent);
    }

    private void SetTileEvent (RoadTreeNode node, bool hasEvent) {
        // Get event if needed
        EventMain tileEvent = null;
        if (hasEvent && node.tileScript.events.Count > 0) {
            EventMain prefab = node.tileScript.GetRandomEvent();
            node.tile.AddComponent(prefab.GetType());
            tileEvent = (EventMain) node.tile.GetComponent(prefab.GetType());
            tileEvent.SetUp(node);
            node.eventScript = tileEvent;
            currentEventDelay = eventDelay;
        }
        else {
            currentEventDelay--;
        }

        // Handle the user car path
        if (tileEvent != null)
            tileEvent.SetUserPath();
        else {
            RoadTilePath userPath = node.tileScript.GetRandomPath(node.tileScript.userEntrance.dir, currentTrafficZone.MaxSpeed() / 3);
            node.tileScript.SetUserExit(userPath.exit);
            AddToUserPath(userPath);
        }
    }

    private GameObject BestSpeedSign (int maxSpeed) {
        int bestSpeed = -1;
        for (int i = 0; i < speedSignList.Count; i++) {
            if (speedSignList[i].speed >= maxSpeed && (bestSpeed < 0 || speedSignList[i].speed < bestSpeed))
                bestSpeed = speedSignList[i].speed;
        }

        List<SpeedSignItem> bestSigns = new List<SpeedSignItem>();
        foreach (SpeedSignItem item in speedSignList) {
            if (item.speed == bestSpeed)
                bestSigns.Add(item);
        }
        return bestSigns[Random.Range(0, bestSigns.Count)].sign;
    }

    private void AddToUserPath(RoadTilePath path) {
        List<PathNode> thisPath = new List<PathNode>();
        foreach (GameObject item in path.ghosts) {
            PathNode node = new PathNode(currentTrafficZone, item, Mathf.Min(path.maxSpeed, currentTrafficZone.MaxSpeed()));
            thisPath.Add(node);
        }
        CarBehaviour._static.AddToPath(thisPath);
    }

    private void ResetZoneSwitchDelay () {
        currentZoneSwitchDelay = Mathf.RoundToInt(Random.Range(currentTrafficZone.MaxSpeed() / (zoneSwitchDelay - zoneSwitchDelayFuzz), currentTrafficZone.MaxSpeed() / (zoneSwitchDelay + zoneSwitchDelayFuzz)));
    }

    private int GetCurrentCarPos () {
        Vector3 carPosition = CarBehaviour._static.transform.position;

        int closestNode = -1;
        for (int i = 0; i < roadTree.Count; i++) {
            if (closestNode < 0 || Vector3.Distance(carPosition, roadTree[i].tile.transform.position) < Vector3.Distance(carPosition, roadTree[closestNode].tile.transform.position))
                closestNode = i;
        }

        return closestNode;
    }
}
