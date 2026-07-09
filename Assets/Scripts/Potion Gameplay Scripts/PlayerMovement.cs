using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerMovement : MonoBehaviour
{
    [SerializeField] private float moveSpeed = 5f;
    [SerializeField] private float cameraLookSensitivity = 2f;
    [SerializeField] private float topLookLimit = -90f;
    [SerializeField] private float bottomLookLimit = 90f;
    [SerializeField] private Transform playerCamera;

    private float horizontalRotation = 0f;
    private float verticalRotation = 0f;
    private InputSystem_Actions controls;


    void Awake()
    {
        controls =  new InputSystem_Actions();
    }
    void OnEnable()
    {
        controls.Enable();
        Debug.Log(controls.Player.Move);
        Debug.Log(controls.Player.Look);
        Debug.Log(controls.Player.Interact);
    }
    
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    void FixedUpdate()
    {
        HandlePlayerMovement();
        HandleCameraLookMovement();
        
        // Get the interact input from the player
        bool interactInput = controls.Player.Interact.ReadValue<bool>();
        Debug.Log(interactInput);
        Vector2 mousePosition = Mouse.current.position.ReadValue();
        Ray ray = Camera.main.ScreenPointToRay(mousePosition);
        Plane groundPlane = new Plane(Vector3.up, Vector3.zero);

        if (groundPlane.Raycast(ray, out float rayLength))
        {
            Vector3 pointingDirection = ray.GetPoint(rayLength);
        }

    }

    void HandlePlayerMovement()
    {
        Vector2 moveInput = controls.Player.Move.ReadValue<Vector2>();
        Vector3 move = new Vector3(moveInput.x, 0f, moveInput.y);
        transform.Translate(move * moveSpeed * Time.fixedDeltaTime);
    }

    void HandleCameraLookMovement()
    {
        Vector2 lookInput = controls.Player.Look.ReadValue<Vector2>();
        horizontalRotation += lookInput.x * cameraLookSensitivity;
        verticalRotation -= lookInput.y * cameraLookSensitivity;
        verticalRotation = Mathf.Clamp(verticalRotation, topLookLimit, bottomLookLimit); //Limit how far the player can look up and down
        playerCamera.rotation = Quaternion.Euler(verticalRotation, horizontalRotation, 0f);
    }


    void OnDisable()
    {
        controls.Disable();
    }

}
