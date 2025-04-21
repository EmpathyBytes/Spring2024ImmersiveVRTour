using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;


public class AchievementManager : MonoBehaviour
{
    public Achievement1 a1;
    public Achievement2 a2;
    public Achievement3 a3;
    public string scene;
    public string currentScene;
    public string test;

    void Awake() {
        scene = SceneManager.GetActiveScene().name;;
    }   
    void Update() {
        currentScene = SceneManager.GetActiveScene().name;
        if (scene != currentScene) {
            UpdateAchievements();
            scene = currentScene;
        } else {
            UpdateAchievements();
        }
    }

    public void UpdateAchievements() {
        UpdateAchievement1();
        
        UpdateAchievement2();

        UpdateAchievement3();

        test = "working";
    }

    public void UpdateAchievement1() {
        int roomNum = -1;
        string floorName = SceneManager.GetActiveScene().name;
        switch(floorName){
            case "Freshman Dorm":
                roomNum = 0;
                break;
            case "Main Menu":
                break;
            case "Music Room":
                roomNum = 1;
                break;
            case "Reading Room":
                roomNum = 2;
                break;
            case "Paper Museum":
                roomNum = 3;
                break;
            default:
                break;
        
        }
        a1.incrementAchievement(roomNum);
        a1.UpdateAchievementText();
    }
    public void UpdateAchievement2() {
        a2.UpdateAchievementText();
    }

    public void UpdateAchievement3() {
        a3.UpdateAchievementText();
    }

}
