using System.Collections.Generic;
using UnityEngine;

public class EventSpeed : EventMain {
    public EventSpeed () {
        question = new QuestionMain();
        question.question = "Welke actie kan je het beste nemen?";
        question.answers.Add(new QuestionAnswer("Remmen", false));
        question.answers.Add(new QuestionAnswer("Gas Loslaten", false));
        question.answers.Add(new QuestionAnswer("Niets Doen", false));
    }

    protected override void AfterQuestionEnd () {
        float newSpeed = userPathNodes[0].targetSpeed;

        switch (userAnswer) {
            case 0:
                newSpeed *= 0.5f;
                CarBehaviour._static.Brake();
                break;
            case 1:
                newSpeed *= 0.7f;
                break;

            case -1:
                newSpeed *= 2;
                break;
        }

        for (int i = 0; i < Mathf.Max(1, userPathNodes.Count / 3); i++) {
            ModifyUserPathSpeed(i, newSpeed);
        }
    }
}
