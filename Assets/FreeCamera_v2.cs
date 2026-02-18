using UnityEngine;

public class FreeCamera_v2 : MonoBehaviour
{
    public float sensitivity;
    public float slowSpeed, normalSpeed, fastSpeed;
    public float zoomSpeed = 10f;
    float currentSpeed;
    Camera cam;
    public float minFOV = 20f;// These two can be adjusted so you can get more/less zoom with scroll
    public float maxFOV = 80f;
    float targetFOV;
    public float screenEdgeC;// Screen edge Closeness/ needs to be renamed
    public float edgeRotation;// how fast it rotates after you move mouse to the edge of the screeen

    
    void Awake()
    {
        cam = GetComponent<Camera>();
        targetFOV = cam.fieldOfView;
    }
    void Update()
    {
        Zoom();
        Movement();
        if (Input.GetMouseButton(1))
        {
            Cursor.visible = false;
            Cursor.lockState = CursorLockMode.Locked; //Could also add some specific cursor so the user knows that it is rotating
        }
        else
        {
            Cursor.visible = true;
            Cursor.lockState = CursorLockMode.None;
        }
        Rotation();
    }
    void Rotation() // It works well except when you go to the upper edge of the screen, need to figure out why the camera wont rotate upward
    {
        Vector3 rotationInput = Vector3.zero;
        if (Input.GetMouseButton(1))
        {
             rotationInput = new Vector3(-Input.GetAxis("Mouse Y"), Input.GetAxis("Mouse X"), 0);
        }
        //transform.Rotate(mouseInput * sensitivity * Time.deltaTime * 50);
        Vector3 mousePosition = Input.mousePosition;
        if (mousePosition.x <= screenEdgeC)
        {
            rotationInput.y = -1f;
        }
        if (mousePosition.x >= Screen.width - screenEdgeC)
        {
            rotationInput.y = 1f;
        }
        if (mousePosition.y <= screenEdgeC)
        {
            rotationInput.x = 1f;
        }

        if (mousePosition.y >= Screen.width - screenEdgeC)
        {
            rotationInput.x = -1f;
        }
        transform.Rotate(rotationInput * edgeRotation * Time.deltaTime* sensitivity);
        Vector3 eulerRotation = transform.rotation.eulerAngles;
        transform.rotation = Quaternion.Euler(eulerRotation.x, eulerRotation.y, 0);
    }
    void Movement()
    {
        Vector3 input = new Vector3(Input.GetAxis("Horizontal"), 0f, Input.GetAxis("Vertical"));
        if (Input.GetKey(KeyCode.LeftShift))
        {
            currentSpeed = fastSpeed;
        }
        else if (Input.GetKey(KeyCode.LeftControl))
        {
            currentSpeed = slowSpeed;
        }
        else
        {
            currentSpeed = normalSpeed;
        }
        transform.Translate(input * currentSpeed * Time.deltaTime);
    }
    void Zoom()
    {
        float scroll = Input.GetAxis("Mouse ScrollWheel");
        if (scroll != 0f)
        {
            targetFOV -= scroll * zoomSpeed;
            targetFOV = Mathf.Clamp(targetFOV, minFOV, maxFOV);

        }
        cam.fieldOfView = Mathf.Lerp(cam.fieldOfView, targetFOV, Time.deltaTime * 10f);
    }
}
