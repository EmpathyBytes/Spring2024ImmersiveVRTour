using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FireworkStarter : MonoBehaviour
{
   public ParticleSystem fireworks;
   public FireStart f1;
   public FireStart f2;
   public FireStart f3;
   public FireStart f4;

    private bool hasPlayed = false;
    
    void Update()
    {
        //Debug.Log(f1.getIsLit() && f2.getIsLit() && f3.getIsLit() && f4.getIsLit());
        if (!hasPlayed && f1.getIsLit() && f2.getIsLit() && f3.getIsLit() && f4.getIsLit()){
            playFireWorks();
            hasPlayed = true;
        }
    }

    void playFireWorks(){
        if (fireworks != null) {
            fireworks.Play();
        }
        
    }
}
