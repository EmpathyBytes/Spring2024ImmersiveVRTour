using UnityEngine;

public class SimpleCarController : MonoBehaviour
{
    public float moveSpeed = 12f;
    public float turnSpeed = 80f;

    void Update()
    {
        float move = Input.GetAxis("Vertical") * moveSpeed * Time.deltaTime;
        float turn = Input.GetAxis("Horizontal") * turnSpeed * Time.deltaTime;

        transform.Translate(0, 0, move);
        transform.Rotate(0, turn, 0);
    }
}