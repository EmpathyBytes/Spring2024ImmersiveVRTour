using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class objectCount : MonoBehaviour
{
    private int totalObjects;
    private bool[] objectsPicked;

    void Awake(){
        totalObjects = 20;
        objectsPicked = new bool[totalObjects];
    }
    public void objectPickedUp(int num){
        objectsPicked[num] = true;
    }

    public bool[] getObjectsPicked(){
        return objectsPicked;
    }
}
