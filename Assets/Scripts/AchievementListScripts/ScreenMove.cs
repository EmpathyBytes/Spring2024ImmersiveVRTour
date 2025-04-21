using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ScreenMove : MonoBehaviour
{

    // Update is called once per frame
    void Update()
    {
        if(Input.GetKeyDown(KeyCode.W)){
            Application.LoadLevel("Music Room");
        }
        if(Input.GetKeyDown(KeyCode.A)){
            Application.LoadLevel("Freshman Dorm");
        }
    }
}
