using System.Collections.Generic;
using UnityEngine;

public class EventMain : MonoBehaviour {
    protected QuestionMain question = null;
    protected bool hasQuestion {
        get {
            return question != null;
        }
    }
    protected RoadTreeNode roadNode;
    protected List<PathNode> userPathNodes = new List<PathNode>();
    protected int userAnswer = -1;
    protected bool stopAfterFail = true;
    public bool questionDone = false;

    public void SetUp (RoadTreeNode node) {
        roadNode = node;
    }

    public void StartQuestion () {
        if (hasQuestion && !questionDone) {
            QuestionHandler._static.SetQuestion(question);
            AfterQuestionStart();
        }
    }

    protected virtual void AfterQuestionStart () { }

    public void EndQuestion () {
        if (hasQuestion && !questionDone) {
            questionDone = true;
            userAnswer = QuestionHandler._static.GetAnswer();
            if (stopAfterFail && !AnswerCorrect(userAnswer))
                RoadController._static.gameRunning = false;
            AfterQuestionEnd();
        }
    }

    protected virtual void AfterQuestionEnd () { }

    public virtual void SetUserPath () {
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

    protected void ModifyPathSpeed (ref List<PathNode> path, int index, float newSpeed) {
        PathNode temp = path[index];
        temp.targetSpeed = newSpeed;
        path[index] = temp;
    }

    protected void ModifyUserPathSpeed (int index, float newSpeed) {
        ModifyPathSpeed(ref userPathNodes, index, newSpeed);
    }

    protected bool AnswerCorrect (int answer) {
        if (hasQuestion)
            return question.answers[answer].correct;
        return true;
    }
}
