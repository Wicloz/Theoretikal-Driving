using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public struct QuestionAnswer {
    public string answer;
    public bool correct;

    public QuestionAnswer (string answer, bool correct) {
        this.answer = answer;
        this.correct = correct;
    }
}

[System.Serializable]
public class QuestionMain {
    public string question = "";
    public string explanation = "";
    public List<QuestionAnswer> answers = new List<QuestionAnswer>();
}
