using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;

public class Achievement1 : MonoBehaviour
{
    private string achievement;
    private int roomsExplored;
    public int roomsTotal;
    public bool[] roomExplored;
    public GameObject gameObject;
    public TMP_Text text;
    public GameObject toggle;
    //public string test;

    void Awake() {
        roomsTotal = 4;
        roomExplored = new bool[roomsTotal];
    }

    public void UpdateAchievementText() {
        toggle = GameObject.FindWithTag("toggle1");
        gameObject = GameObject.FindWithTag("achievement1");
        //test = "test";
        if (gameObject!=null) {
            text = gameObject.GetComponent<TMP_Text>();

        roomsExplored = 0;
        for (int i = 0; i < roomsTotal; i++){
            if (roomExplored[i] == true){
                roomsExplored++;
            }
        }
        achievement = "Explore each room: " + roomsExplored + " / " + roomsTotal ;
        text.text = achievement;
        if (roomsExplored == roomsTotal) {
            if (toggle != null) {
                toggle.GetComponent<Toggle>().isOn = true;
            }
        }
        }
        
    }

    public void incrementAchievement(int roomNum) {
        if (roomNum >= 0) {
            roomExplored[roomNum] = true;
        } 
    }
}
