using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class QuestionHandler : MonoBehaviour {
    public static QuestionHandler _static = null;
    public GameObject questionObject;
    public List<GameObject> buttonObjects;
    public Color correctColour;
    public Color wrongColour;

    private int userAnswer = -1;
    private bool _questionActive = false;
    public bool questionActive {
        get {
            return _questionActive;
        }
    }

    void Awake () {
        if (_static == null)
            _static = this;
    }

    void Start () {
        GetAnswer();
    }

    void Update () {
        if (_questionActive) {
            if (Mathf.Round(Input.GetAxis("horizontal")) == -1) {
                userAnswer = 3;
                _questionActive = false;
            }
            if (Mathf.Round(Input.GetAxis("horizontal")) == 1) {
                userAnswer = 2;
                _questionActive = false;
            }
            if (Mathf.Round(Input.GetAxis("vertical")) == -1) {
                userAnswer = 1;
                _questionActive = false;
            }
            if (Mathf.Round(Input.GetAxis("vertical")) == 1) {
                userAnswer = 0;
                _questionActive = false;
            }

            if (userAnswer != -1) {
                buttonObjects[userAnswer].GetComponent<Button>().Select();
            }
        }
    }

    public void SetQuestion (QuestionMain question) {
        userAnswer = -1;
        questionObject.SetActive(true);
        questionObject.GetComponent<Text>().text = question.question;
        for (int i = 0; i < question.answers.Count; i++) {
            buttonObjects[i].SetActive(true);
            buttonObjects[i].transform.FindChild("Text").GetComponent<Text>().text = question.answers[i].answer;
            ColorBlock buttonColours = buttonObjects[i].GetComponent<Button>().colors;
            buttonColours.pressedColor = question.answers[i].correct ? correctColour : wrongColour;
            buttonObjects[i].GetComponent<Button>().colors = buttonColours;
        }
        _questionActive = true;
    }

    public int GetAnswer () {
        _questionActive = false;
        questionObject.SetActive(false);
        foreach (GameObject button in buttonObjects) {
            button.SetActive(false);
        }
        return userAnswer;
    }
}
