using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ScreenMoveTest : MonoBehaviour
{

    // Update is called once per frame
    void Update()
    {
        if(Input.GetKeyDown(KeyCode.W)){
            Application.LoadLevel("Music Room");
        }
    }
}
