using Oculus.Interaction.Input;
using Oculus.Interaction.PoseDetection; 
using System.Collections;
using UnityEngine.SceneManagement;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;

public class HandReferenceDetector : MonoBehaviour
{
    [SerializeField] private HandRef _handRef;
    [SerializeField] private FingerFeatureStateProvider _fingerFeatureProvider;

    public GameObject test;
    
    private void Awake()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        FindAndAssignHands();
    }

    private void FindAndAssignHands()
    {   
        GameObject handObject = GameObject.FindWithTag("RightHand");
        

        // Find the new hand in the scene
        IHand newHand = handObject.GetComponent<IHand>();
        
        if (newHand != null)
        {
            // Update HandRef
            if (_handRef != null)
            {
                _handRef.InjectHand(newHand);
            }
            
            // Update FingerFeatureStateProvider
            if (_fingerFeatureProvider != null)
            {
                _fingerFeatureProvider.InjectHand(newHand);
            }
        }
        else
        {
            
            Debug.LogWarning("No hand found in new scene");
        }
    }

    private void OnDestroy()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }
}
