using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AcheivementToggle : MonoBehaviour
{
    
    public bool toggle1 = false;
    public bool toggle2 = false;
    public GameObject gameObject;
    public GameObject test;
    public FollowCamera text;
    
    public void toggle() {
        //test.SetActive(true);
        if (!toggle2) {
            toggle1 = true;
            gameObject.SetActive(true);
            text.setSwitch(true);
        }
        if (toggle2) {
            toggle1 = false;
            gameObject.SetActive(false);
            
        }

    }

    public void unToggle() {
        //test.SetActive(false);
        if (toggle1) {
            toggle2 = true;
        }
        if (!toggle1) {
            toggle2 = false;
        }
        
    }
}
