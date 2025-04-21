using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;

public class Achievement3 : MonoBehaviour
{
    private string achievement;
    public TMP_Text text;
    public GameObject gameObject;
    public GameObject toggle;
    public objectCount objectCountComp;
    private bool[] getObjectsPicked;
    private bool[] objectsPicked;
    private int objectCountNum;
    private int goalCount;

    void Awake() {
        getObjectsPicked = new bool[20];
        objectsPicked = new bool[20];
    }
    public void UpdateAchievementText() {
        goalCount = 5;
        GameObject objectCount = GameObject.FindGameObjectWithTag("objectCount");
        if (objectCount != null) {
            objectCountComp = objectCount.GetComponent<objectCount>();
        }
       
        if (objectCount != null) {
            getObjectsPicked = objectCountComp.getObjectsPicked();
            for (int i = 0; i < getObjectsPicked.Length; i++) {
                if (getObjectsPicked[i] == true) {
                    objectsPicked[i] = true;
                }
            }
        }
        

        toggle = GameObject.FindWithTag("toggle3");
        gameObject = GameObject.FindWithTag("achievement3");
        //test = "test";
        if (gameObject!=null) {
            text = gameObject.GetComponent<TMP_Text>();
            objectCountNum = 0;
            for (int i = 0; i < objectsPicked.Length; i++) {
                if (objectsPicked[i] == true) {
                    objectCountNum++;
                }
            }
        
        achievement = "Pick up 5 items in the Reading Room: " + objectCountNum + " / " + goalCount;
        text.text = achievement;
        if (objectCountNum >= goalCount) {
            if (toggle != null) {
                toggle.GetComponent<Toggle>().isOn = true;
            }
        }
        }
        
    }
}
