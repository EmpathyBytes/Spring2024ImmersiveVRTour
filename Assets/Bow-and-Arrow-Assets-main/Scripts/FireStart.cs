using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FireStart : MonoBehaviour
{
    public ParticleSystem fireParticles;
    public bool _isLit = false;

    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    private void OnTriggerEnter(Collider other)
    {
        Ignite(other);
    }

    public void Ignite(Collider other)
    {
        Arrow arrow = other.GetComponent<Arrow>();
        if (arrow != null){
            if (!_isLit && fireParticles != null && arrow.getIsLit())
            {
                _isLit = true;
                fireParticles.Play();
                Debug.Log("Arrow is now on fire!");
            }
        }
        
    }
    public void Extinguish()
    {
        if (_isLit && fireParticles != null)
        {
            _isLit = false;
            fireParticles.Stop();
        }
    }

    public bool getIsLit(){
        return _isLit;
    }
}
