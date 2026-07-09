using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerMovement : MonoBehaviour
{
    [SerializeField] private float moveSpeed = 5f;

    [SerializeField] private float cameraLookSensitivity = 2f;
    [SerializeField] private float sideLookLimit = 90f;
    [SerializeField] private Transform playerCamera;

    [SerializeField] private float interactionDistance = 3f;
    [SerializeField] private Transform holdPosition;
    
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
        HandleCameraLookMovement();
        HandlePlayerInteraction();
    }

    void FixedUpdate()
    {
        HandlePlayerMovement();
    }

    void HandlePlayerMovement()
    {
        Vector2 moveInput = controls.Player.Move.ReadValue<Vector2>();
        Vector3 move = new Vector3(moveInput.x, 0f, moveInput.y);
        transform.Translate(move * moveSpeed * Time.fixedDeltaTime, Space.Self);
    }

    void HandleCameraLookMovement()
    {
        Vector2 lookInput = controls.Player.Look.ReadValue<Vector2>();
        
        horizontalRotation = lookInput.x * cameraLookSensitivity;
        transform.Rotate(Vector3.up * horizontalRotation);

        verticalRotation -= lookInput.y * cameraLookSensitivity;
        verticalRotation = Mathf.Clamp(verticalRotation, -sideLookLimit, sideLookLimit); //Limit how far the player can look up and down
        
        playerCamera.localRotation = Quaternion.Euler(verticalRotation, 0f, 0f);
    }

    void HandlePlayerInteraction()
    {
        // Lanzar rayo desde el centro exacto de la cámara hacia adelante
        Ray ray = new Ray(playerCamera.position, playerCamera.forward);
        RaycastHit hit;

        // Dibujar el rayo en la vista de escena para debuggear de forma visual
        Debug.DrawRay(playerCamera.position, playerCamera.forward * interactionDistance, Color.cyan);

        if (Physics.Raycast(ray, out hit, interactionDistance))
        {
            Debug.Log("Hit object: " + hit.collider.gameObject.name);
        }
    }

   

    void OnDisable()
    {
        controls.Disable();
    }

}
