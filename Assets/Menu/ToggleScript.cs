using System.Collections;
using System.Collections.Generic;
using UnityEditor.PackageManager.Requests;
using UnityEngine;
using UnityEngine.UI;

public class ToggleScript : MonoBehaviour
{
    [SerializeField] private GameObject ToggleAble;


    public void ToggleMenu()
    {
        if (ToggleAble.activeSelf)
        {
            ToggleAble.SetActive(false);
        }
        else
        {
            ToggleAble.SetActive(true);
        }
    }
}
