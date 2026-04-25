using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class FireSource : MonoBehaviour
{
    private void OnTriggerEnter(Collider other)
    {
        Arrow arrow = other.GetComponent<Arrow>();
        if (arrow != null)
        {
            arrow.Ignite();
        }
    }
}