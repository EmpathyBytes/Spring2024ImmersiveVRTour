using UnityEngine;

public class DebugMovement : MonoBehaviour
{
    [SerializeField]
    private float speed;

    void Update()
    {
        if (Input.GetKey(KeyCode.W))
            transform.position += speed * Time.deltaTime * transform.forward;
        if (Input.GetKey(KeyCode.S))
            transform.position -= speed * Time.deltaTime * transform.forward;
        if (Input.GetKey(KeyCode.A))
            transform.position -= speed * Time.deltaTime * transform.right;
        if (Input.GetKey(KeyCode.D))
            transform.position += speed * Time.deltaTime * transform.right;

        if (Input.GetKey(KeyCode.Q))
            transform.localEulerAngles -= speed * 20 * Time.deltaTime * new Vector3(0, 1);
        if (Input.GetKey(KeyCode.E))
            transform.localEulerAngles += speed * 20 * Time.deltaTime * new Vector3(0, 1);

        if (Input.GetKey(KeyCode.Tab))
            transform.position += speed * Time.deltaTime * transform.up;
        if (Input.GetKey(KeyCode.LeftShift))
            transform.position -= speed * Time.deltaTime * transform.up;
    }
}
