using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Arrow : MonoBehaviour
{
    public float speed = 10f;
    public Transform tip;

    private Rigidbody _rigidBody;
    private bool _inAir = false;
    private Vector3 _lastPosition = Vector3.zero;
    public GameObject floatingTextPrefab;
    

    private void Awake(){
        _rigidBody = GetComponent<Rigidbody>();
        PullInteraction.PullReleased += Release;

        Stop();
    }

    private void OnDestroy(){
        PullInteraction.PullReleased -= Release;
    }

    private void Release(float value) {
        
        PullInteraction.PullReleased -= Release;
        gameObject.transform.parent = null;

        _inAir = true;
        SetPhysics(true);

        Vector3 force = transform.forward * value * speed;
        _rigidBody.AddForce(force, ForceMode.Impulse);

        StartCoroutine(RotateWithVelocity());
        _lastPosition = tip.position;
    }

    private IEnumerator RotateWithVelocity(){
        yield return new WaitForFixedUpdate();
        while (_inAir){
            Quaternion newRotation = Quaternion.LookRotation(_rigidBody.velocity, transform.up);
            transform.rotation = newRotation;
            yield return null;
        }
    }
    void FixedUpdate(){
        if (_inAir){
            CheckCollision();
            _lastPosition = tip.position;
        }
    }

    private void CheckCollision(){
        if (Physics.Linecast(_lastPosition, tip.position, out RaycastHit hitInfo)){
            if ((hitInfo.transform.gameObject.layer != 0 )){
                if (hitInfo.transform.TryGetComponent(out Rigidbody body)){
                    _rigidBody.interpolation = RigidbodyInterpolation.None;
                    transform.parent = hitInfo.transform;
                    body.AddForce(_rigidBody.velocity, ForceMode.Impulse);
                }
                Stop();

                TargetPoints target = hitInfo.transform.GetComponent<TargetPoints>();
                if (target != null && floatingTextPrefab != null)
                    {
                        int points = target.pointAmount;
                        string message;
                        if (points != 10) {
                            message = "+" + points;
                        } else {
                            message = "Bullseye! +10";
                        }
                        target.controller.hitDecide(target);
                        Color color = target.getColor();
                        // hitInfo: from your linecast
                        Vector3 spawnPos = hitInfo.point + hitInfo.transform.forward * 0.1f + Vector3.up * 0.05f;


                        GameObject fx = Instantiate(floatingTextPrefab, spawnPos, Quaternion.identity);
                        fx.GetComponent<FloatingText3D>().SetText(message, color);
                    }
            }
        }
    }

    private void Stop() {
        _inAir = false;
        SetPhysics(false);
    }

    private void SetPhysics(bool usePhysics) {
        _rigidBody.useGravity = usePhysics;
        _rigidBody.isKinematic = !usePhysics;
    }

}
