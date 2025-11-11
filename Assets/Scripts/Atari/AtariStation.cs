using UnityEngine;

public class AtariStation : MonoBehaviour
{
    public Vector3 CurrentDirection => currentDirection;
    public bool ButtonPressed => buttonPressed;

    [SerializeField]
    private bool startInDebug;

    [SerializeField]
    private GameObject screenTexture;

    [SerializeField]
    private Transform player;

    [SerializeField]
    private float playerMovementSpeed;

    [SerializeField]
    private AtariTitleScreen titleScreen;

    [SerializeField]
    private AtariRunningScreen runningScreen;

    [SerializeField]
    private AtariArcheryScreen archeryScreen;

    private Vector3 currentDirection;
    private bool buttonPressed;

    private void Start() => Reset();

    public void Reset()
    {
        titleScreen.gameObject.SetActive(true);
        buttonPressed = false;
    }

    public void LoadRunning()
    {
        titleScreen.gameObject.SetActive(false);
        runningScreen.gameObject.SetActive(true);
    }

    public void LoadArchery()
    {
        titleScreen.gameObject.SetActive(false);
        archeryScreen.gameObject.SetActive(true);
    }

    public void MoveJoystick(Vector3 direction) =>
        currentDirection = new Vector3(direction.x, 0, -direction.z);

    public void PressButton(bool pressed) => buttonPressed = pressed;

    public void Update()
    {
        if (startInDebug)
        {
            currentDirection = new(Input.GetAxisRaw("Horizontal"), 0, Input.GetAxisRaw("Vertical"));
            buttonPressed = Input.GetButton("Fire1");
        }
    }
}
