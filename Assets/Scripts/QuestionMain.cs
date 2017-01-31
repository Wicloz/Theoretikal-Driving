using System.Collections.Generic;
using UnityEngine;

public struct QuestionAnswer {
    public string answer;
    public bool correct;

    public QuestionAnswer (string answer, bool correct) {
        this.answer = answer;
        this.correct = correct;
    }
}

public class QuestionMain : MonoBehaviour {
    public string question = "";
    public List<QuestionAnswer> answers = new List<QuestionAnswer>();
}
