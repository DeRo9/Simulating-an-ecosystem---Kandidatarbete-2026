using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;

[RequireComponent(typeof(Camera))]
public class FreeCamera : MonoBehaviour
{

    [Header("Default Camera")]
    public Vector3 defaultPosition = new Vector3(0, 20, -50);
    public Vector3 defaultRotation = new Vector3(45, 0, 0);

    private Transform selectedAnimal; // Selected but not yet followed
    private InputAction followKeyAction;
    private InputAction resetCameraAction;
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

    [Header("Follow")]
    public float followDistance = 10f;
    public float followHeight = 5f;
    public float followSmoothSpeed = 5f;
    private Transform followTarget;
    private float orbitAngle = 0f;

    [Header("Terrain")]
    public float minHeightAboveTerrain = 1.5f;
    private float currentSpeed;
    private Camera cam;
    private float targetFOV;
    private float screenEdgeRight;
    private float screenEdgeTop;

    // Bug where the raycaster is ghost hovering, causing glitches. So
    // then we disable whenever we are rightclicking to prevent this.
    private PhysicsRaycaster raycaster;
    private float raycasterCooldown = 0f;
    private float cooldownDuration = 0.1f;

    private InputAction moveAction;
    private InputAction lookDeltaAction;
    private InputAction rmbAction;
    private InputAction lmbAction; 
    private InputAction sprintAction;
    private InputAction slowAction;
    private InputAction scrollAction;
    private InputAction escapeAction;

    private void Awake()
    {
        cam = GetComponent<Camera>();
        targetFOV = cam.fieldOfView;
        screenEdgeRight = Screen.width - screenEdgeC;
        screenEdgeTop = Screen.height - screenEdgeC;

        raycaster = GetComponent<PhysicsRaycaster>();

        moveAction = new InputAction("Move", InputActionType.Value, binding: "<Keyboard>/wasd");
        moveAction.AddCompositeBinding("2DVector").With("Up", "<Keyboard>/w").With("Down", "<Keyboard>/s").With("Left", "<Keyboard>/a").With("Right", "<Keyboard>/d");
        lookDeltaAction = new InputAction("LookDelta", InputActionType.Value, "<Mouse>/delta");
        rmbAction = new InputAction("RMB", InputActionType.Button, "<Mouse>/rightButton");
        lmbAction = new InputAction("LMB",InputActionType.Button, "<Mouse>/leftButton");
        sprintAction = new InputAction("Sprint", InputActionType.Button, "<Keyboard>/leftShift");
        slowAction = new InputAction("Slow", InputActionType.Button, "<Keyboard>/leftCtrl");
        scrollAction = new InputAction("Scroll", InputActionType.Value, "<Mouse>/scroll");
        escapeAction = new InputAction("Escape",InputActionType.Button,"<Keyboard>/escape");
        followKeyAction = new InputAction("Follow", InputActionType.Button, "<Keyboard>/f");
        resetCameraAction = new InputAction("Reset", InputActionType.Button, "<Keyboard>/space");
    }

    private void OnEnable()
    {
        moveAction.Enable();
        lookDeltaAction.Enable();
        rmbAction.Enable();
        lmbAction.Enable();
        sprintAction.Enable(); slowAction.Enable(); scrollAction.Enable();
        escapeAction.Enable();
        followKeyAction.Enable();
        resetCameraAction.Enable();
    }

   private void OnDisable()
{
    moveAction.Disable(); 
    lookDeltaAction.Disable();
    rmbAction.Disable();
    lmbAction.Disable(); 
    sprintAction.Disable(); 
    slowAction.Disable(); 
    scrollAction.Disable();
    followKeyAction.Disable();
    resetCameraAction.Disable();
}
    /*private void Update()
    {
        HandleCursorAndRotation();
        Movement();
        Zoom();
        ZeroRoll();
    }*/

    private void Update()
{
    HandleAnimalClick();

    if (resetCameraAction.WasPressedThisFrame())
        ResetCamera();

    if (followKeyAction.WasPressedThisFrame())
{
    Debug.Log("F pressed. Selected animal: " + (selectedAnimal != null ? selectedAnimal.name : "NONE"));
    
    if (selectedAnimal != null)
    {
        followTarget = selectedAnimal;
        orbitAngle = followTarget.eulerAngles.y;
        Debug.Log("Now following " + followTarget.gameObject.name);
    }
}

    if (escapeAction.WasPressedThisFrame())
        StopFollowing();

    if (followTarget != null)
    {
        FollowAnimal();
    }
    else
    {
        HandleCursorAndRotation();
        Movement();
    }

    Zoom();
    clampToTerrain();
    ZeroRoll();
}
    private void HandleCursorAndRotation()
    {
        bool rotation = rmbAction.IsPressed();
        if (rotation) { 
            Cursor.visible = false;
            Cursor.lockState = CursorLockMode.Locked;
            raycasterCooldown = cooldownDuration;
            raycaster.enabled = false;
        }
        else
        {
            Cursor.visible = true;
            Cursor.lockState = CursorLockMode.None;
            raycasterCooldown -= Time.deltaTime;
            raycaster.enabled = raycasterCooldown <= 0f;
            
        }
        Vector3 rotationInput = Vector3.zero;
        if (rotation) {
            Vector2 delta = lookDeltaAction.ReadValue<Vector2>();
            rotationInput = new Vector3(-delta.y, delta.x, 0f);
        }

        if (!rotation)
        {
            Vector2 mousePosition = Mouse.current?.position.ReadValue() ?? Vector2.zero;
            if (mousePosition.x <= screenEdgeC) { rotationInput.y = -1f; }
            else if (mousePosition.x >= screenEdgeRight)
            {
                rotationInput.y = 1f;
            }
            if (mousePosition.y <= screenEdgeC) { rotationInput.x = 1f; }
            else if (mousePosition.y >= screenEdgeTop)
            {
                rotationInput.x = -1f;
            }

        }

        float multiplier = rotation ? sensitivity : (edgerotation * sensitivity);
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
    /*private void Zoom()
    {
        Vector2 scroll = scrollAction.ReadValue<Vector2>();
        if (Mathf.Abs(scroll.y) > 0.01f)
        {
            targetFOV -= scroll.y * (zoomSpeed * 0.01f);
            targetFOV = Mathf.Clamp(targetFOV,minFOV,maxFOV);
        }
        cam.fieldOfView = Mathf.Lerp (cam.fieldOfView,targetFOV, Time.deltaTime * 10f);
    }*/

    private void Zoom()
    {
        Vector2 scroll = scrollAction.ReadValue<Vector2>();
        if (Mathf.Abs(scroll.y) > 0.01f)
        {
            if (followTarget != null)
            {
                followDistance = Mathf.Clamp(followDistance - scroll.y * 0.5f, 3f, 30f);
            }
            else
            {
                targetFOV -= scroll.y * (zoomSpeed * 0.01f);
                targetFOV = Mathf.Clamp(targetFOV, minFOV, maxFOV);
            }
        }
        cam.fieldOfView = Mathf.Lerp(cam.fieldOfView, targetFOV, Time.deltaTime * 10f);
    }

    private void ZeroRoll()
    {
        Vector3 euler = transform.rotation.eulerAngles;
        if (euler.z != 0f)
        {
            transform.rotation = Quaternion.Euler(euler.x, euler.y, 0);
        }
    }

    /*private void HandleAnimalClick()
    {
        if (!lmbAction.WasPressedThisFrame()) return;
        if(EventSystem.current != null && EventSystem.current.IsPointerOverGameObject()) return;
        Ray ray = cam.ScreenPointToRay(Mouse.current.position.ReadValue());
        RaycastHit hit;

        if(Physics.Raycast(ray,out hit, 500f))
        {
            AnimalBehaviour animal = hit.collider.GetComponentInParent<AnimalBehaviour>();
            if(animal != null)
            {
                if(followTarget == animal.transform)
                {
                    StopFollowing();
                }
                else
                {
                    followTarget = animal.transform;
                    Debug.Log("Following" + animal.gameObject.name);
                }
            }
            else
            {
                StopFollowing();
            }
        }
    }*/

    private void HandleAnimalClick()
{
    if (!lmbAction.WasPressedThisFrame()) return;

    Ray ray = cam.ScreenPointToRay(Mouse.current.position.ReadValue());
    RaycastHit hit;

    if (Physics.Raycast(ray, out hit, 500f))
    {
        AnimalBehaviour animal = hit.collider.GetComponentInParent<AnimalBehaviour>();
        if (animal != null)
        {
            selectedAnimal = animal.transform;
            Debug.Log("Selected: " + animal.gameObject.name + " (press F to follow)");
        }
        else
        {
            selectedAnimal = null;
        }
    }
}



private void FollowAnimal()
{
    if (followTarget == null || followTarget.gameObject == null)
    {
        StopFollowing();
        return;
    }

    Vector3 offset = Quaternion.Euler(0, orbitAngle, 0) * Vector3.back * followDistance;
    Vector3 targetPosition = followTarget.position + offset + Vector3.up * followHeight;

    transform.position = Vector3.Lerp(transform.position, targetPosition, followSmoothSpeed * Time.deltaTime);
    transform.LookAt(followTarget.position + Vector3.up * 1.5f);

    if (rmbAction.IsPressed())
    {
        Vector2 delta = lookDeltaAction.ReadValue<Vector2>();
        followDistance = Mathf.Clamp(followDistance - delta.y * 0.1f, 3f, 30f);
        
        orbitAngle += delta.x * sensitivity * Time.deltaTime * 50f;
    }
}
    private void StopFollowing()
    {
        if(followTarget != null)
        {
            Debug.Log("Stopped following: "+ followTarget.gameObject.name);
        }
        followTarget = null;
    }

    private void clampToTerrain()
    {
        Terrain terrain = Terrain.activeTerrain;
        if(terrain == null) return;
        float terrainHeight = terrain.SampleHeight(transform.position) + terrain.transform.position.y;
        float minHeight = terrainHeight + minHeightAboveTerrain;

        if (transform.position.y < minHeight)
        {
            transform.position = new Vector3(transform.position.x, minHeight, transform.position.z);
        }
    }

    private void ResetCamera()
{
    StopFollowing();
    transform.position = new Vector3(transform.position.x, 20, transform.position.z);
    Debug.Log("Camera height reset");
}

}