using System.Collections.Generic;
using UnityEngine;

public class EventMain : MonoBehaviour {
    protected QuestionMain question = null;
    protected bool hasQuestion {
        get {
            return question == null;
        }
    }
    protected RoadTreeNode roadNode;
    protected List<PathNode> userPathNodes = new List<PathNode>();

    public void SetUp (RoadTreeNode node) {
        roadNode = node;
    }

    public void StartQuestion () {
        if (hasQuestion)
            QuestionHandler._static.SetQuestion(question);
    }

    public void EndQuestion () {
        if (hasQuestion)
            QuestionHandler._static.GetAnswer();
    }

    public void SetUserPath () {
        // Get a random path
        RoadTilePath userPath = roadNode.tileScript.GetRandomPath(roadNode.tileScript.userEntrance.dir, roadNode.tileScript.trafficZone.MaxSpeed() / 3);
        // Set the exit for that path
        roadNode.tileScript.SetUserExit(userPath.exit);
        // Add the path to the user car
        foreach (GameObject item in userPath.ghosts) {
            PathNode node = new PathNode(roadNode.tileScript.trafficZone, item, Mathf.Min(userPath.maxSpeed, roadNode.tileScript.trafficZone.MaxSpeed()));
            userPathNodes.Add(node);
        }
        CarBehaviour._static.AddToPath(userPathNodes);
    }
}
