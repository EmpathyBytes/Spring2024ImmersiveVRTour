using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;

public class Achievement2 : MonoBehaviour
{
    private string achievement;
    public TMP_Text text;
    bool drumsPlayed;
    public GameObject gameObject;
    public GameObject toggle;
    public collisionHandler[] drumStick;

    void Awake(){
        drumStick = new collisionHandler[2];
    }

    public void UpdateAchievementText() {
        GameObject[] drumStickObject = GameObject.FindGameObjectsWithTag("drumstick");
        

        toggle = GameObject.FindWithTag("toggle2");
        gameObject = GameObject.FindWithTag("achievement2");
        //test = "test";
        if (gameObject!=null) {
            text = gameObject.GetComponent<TMP_Text>();
            for (int i = 0; i < drumStickObject.Length; i++) {
                if (drumStickObject[i] != null) {
                drumStick[i] = drumStickObject[i].GetComponent<collisionHandler>();
                }

                if (drumStick[i] != null) {
                    if (drumStick[i].getDrumPlayed() == true)
                    drumsPlayed = drumStick[i].getDrumPlayed();
                }
            }
        
        achievement = "Play the Drums in the Music Room:" ;
        text.text = achievement;
        if (drumsPlayed) {
            if (toggle != null) {
                toggle.GetComponent<Toggle>().isOn = true;
            }
        }
        }
        
    }
}
