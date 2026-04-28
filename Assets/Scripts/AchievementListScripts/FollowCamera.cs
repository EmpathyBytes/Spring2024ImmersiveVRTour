using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FollowCamera : MonoBehaviour
{
    [SerializeField] private float followSpeed = 5f; // Adjust for faster/slower follow
    [SerializeField] private float rotationSpeed = 10f; // Adjust for faster/slower rotation
    [SerializeField] private float distanceFromCamera = 2f; // How far in front of the camera the object stays
    [SerializeField] private float horizontalOffset = 0.5f; // positive = right, negative = left


    private Vector3 _velocity = Vector3.zero; // Used for SmoothDamp
    public bool _switch = false;
    public bool _switch2 = false;

    void LateUpdate()
    {   
        
        if (Camera.main == null) return;

        Transform camTransform = Camera.main.transform;

        // Calculate the target position (in front of the camera)
        // Calculate target position in front AND to the right
        Vector3 targetPosition = camTransform.position 
                                + camTransform.forward * distanceFromCamera 
                                + camTransform.right * horizontalOffset;


        if (_switch) {
            transform.position = targetPosition;
            _switch = false;
        }

        // Smoothly move towards the target position (with delay)
        transform.position = Vector3.SmoothDamp(
            transform.position,
            targetPosition,
            ref _velocity,
            followSpeed * Time.deltaTime
        );

        // Smoothly rotate to match the camera's rotation (with delay)
        Quaternion targetRotation = Quaternion.LookRotation(transform.position - camTransform.position);

        if (_switch2) {
            transform.rotation = targetRotation;
            _switch2 = false;
        }
        transform.rotation = Quaternion.Slerp(
            transform.rotation,
            targetRotation,
            rotationSpeed * Time.deltaTime
        );
    }
    public void setSwitch(bool _switch) {
        this._switch = _switch;
        this._switch2 = _switch;
    }
}
