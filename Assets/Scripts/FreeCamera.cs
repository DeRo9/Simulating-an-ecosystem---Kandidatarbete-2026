using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(Camera))]
public class FreeCamera : MonoBehaviour
{
    [Header("Movement")]
    public float slowSpeed = 2.0f;
    public float normalSpeed = 5.0f;
    public float fastSpeed = 10f;
    public float sensitivity = 1.0f;

    [Header("Zoom")]
    public float zoomSpeed = 10f;
    public float minFOV = 20f;
    public float maxFOV = 80f;

    [Header("Screen Edge Rotation")]
    public float screenEdgeC = 10f; // how many pixels from the edge does the cursor need to be so that the screen starts rotating
    public float edgerotation = 90f;

    private float currentSpeed;
    private Camera cam;
    private float targetFOV;

    private InputAction moveAction;
    private InputAction lookDeltaAction;
    private InputAction rmbAction;
    private InputAction sprintAction;
    private InputAction slowAction;
    private InputAction scrollAction;

    private void Awake()
    {
        cam = GetComponent<Camera>();
        targetFOV = cam.fieldOfView;

        moveAction = new InputAction("Move", InputActionType.Value, binding: "<Keyboard>/wasd");
        moveAction.AddCompositeBinding("2DVector").With("Up", "<Keyboard>/w").With("Down", "<Keyboard>/s").With("Left", "<Keyboard>/a").With("Right", "<Keyboard>/d");
        lookDeltaAction = new InputAction("LookDelta", InputActionType.Value, "<Mouse>/delta");
        rmbAction = new InputAction("RMB", InputActionType.Button, "<Mouse>/rightButton");
        sprintAction = new InputAction("Sprint", InputActionType.Button, "<Keyboard>/leftShift");
        slowAction = new InputAction("Slow", InputActionType.Button, "<Keyboard>/leftCtrl");
        scrollAction = new InputAction("Scroll", InputActionType.Value, "<Mouse>/scroll");
    }

    private void OnEnable()
    {
        moveAction.Enable();
        lookDeltaAction.Enable();
        rmbAction.Enable();
        sprintAction.Enable(); slowAction.Enable(); scrollAction.Enable();
    }

    private void OnDisable()
    {
        moveAction.Disable(); lookDeltaAction.Disable();
        rmbAction.Disable(); sprintAction.Disable(); slowAction.Disable(); scrollAction.Disable();
    }

    private void Update()
    {
        HandleCursorAndRotation();
        Movement();
        Zoom();
        ZeroRoll();
    }

    private void HandleCursorAndRotation()
    {
        bool rotation = rmbAction.IsPressed();
        if (rotation) { 
            Cursor.visible = false;
            Cursor.lockState = CursorLockMode.Locked;
        }
        else
        {
            Cursor.visible = true;
            Cursor.lockState = CursorLockMode.None;
        }
        Vector3 rotationInput = Vector3.zero;
        if (rotation) {
            Vector2 delta = lookDeltaAction.ReadValue<Vector2>();
            rotationInput = new Vector3(-delta.y, delta.x, 0f);
        }

        /*if (!rotation)
        {
            Vector2 mousePosition = Mouse.current?.position.ReadValue() ?? Vector2.zero;
            if (mousePosition.x <= screenEdgeC) { rotationInput.y = -1f; }
            else if (mousePosition.x >= Screen.width - screenEdgeC)
            {
                rotationInput.y = 1f;
            }
            if (mousePosition.y <= screenEdgeC) { rotationInput.x = 1f; }
            else if (mousePosition.y >= Screen.height - screenEdgeC)
            {
                rotationInput.x = -1f;
            }

        }*/

        //float multiplier = rotation ? sensitivity : (edgerotation * sensitivity);
        float multiplier = sensitivity;
        transform.Rotate(rotationInput * multiplier * Time.deltaTime,Space.Self);
    }
    private void Movement()
    {
        Vector2 move = moveAction.ReadValue<Vector2>();
        if (sprintAction.IsPressed())
        {
            currentSpeed = fastSpeed;
        }
        else if (slowAction.IsPressed())
        {
            currentSpeed = slowSpeed;
        }
        else
        {
            currentSpeed = normalSpeed;
        }
        Vector3 input = new Vector3(move.x, 0, move.y);
        transform.Translate(input*currentSpeed*Time.deltaTime, Space.Self);
    }
    private void Zoom()
    {
        Vector2 scroll = scrollAction.ReadValue<Vector2>();
        if (Mathf.Abs(scroll.y) > 0.01f)
        {
            targetFOV -= scroll.y * (zoomSpeed * 0.01f);
            targetFOV = Mathf.Clamp(targetFOV,minFOV,maxFOV);
        }
        cam.fieldOfView = Mathf.Lerp (cam.fieldOfView,targetFOV, Time.deltaTime * 10f);
    }

    private void ZeroRoll()
    {
        Vector3 euler = transform.rotation.eulerAngles;
        transform.rotation = Quaternion.Euler(euler.x, euler.y, 0);
    }

}