using UnityEditor;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerMovement : MonoBehaviour
{
    [SerializeField] private float moveSpeed = 5f;

    public float cameraLookSensitivity = 100f;
    [SerializeField] private float lookLimit = 90f;
    [SerializeField] private Transform playerCamera;

    [SerializeField] private float interactionDistance = 3f;
    [SerializeField] private Transform grabbedObjectPosition;
    private float horizontalRotation = 0f;
    private float verticalRotation = 0f;
    private GameObject grabbedObject = null;

    private CharacterController characterController;
    private InputSystem_Actions controls;


    void Awake()
    {
        controls =  new InputSystem_Actions();
        characterController = GetComponent<CharacterController>();
    }
    void OnEnable()
    {
        controls.Enable();
        Debug.Log(controls.Player.Move);
        Debug.Log(controls.Player.Look);
    }
    
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = true;

        horizontalRotation = 0f;
        verticalRotation = 0f;

        transform.rotation = Quaternion.Euler(0f, transform.rotation.eulerAngles.y, 0f);
        if (playerCamera != null)
        {
            playerCamera.localRotation = Quaternion.identity; // Esto es equivalente a (0, 0, 0)
        }
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
        Vector3 moveDirection = moveInput.x * transform.right + moveInput.y * transform.forward;

        characterController.Move(moveDirection * moveSpeed * Time.fixedDeltaTime);
    }

    void HandleCameraLookMovement()
    {
        Vector2 lookInput = controls.Player.Look.ReadValue<Vector2>();
        
        float mouseX = lookInput.x * cameraLookSensitivity * Time.deltaTime;
        float mouseY = lookInput.y * cameraLookSensitivity * Time.deltaTime;

        horizontalRotation = mouseX;
        transform.Rotate(Vector3.up * horizontalRotation);

        verticalRotation -= mouseY;
        verticalRotation = Mathf.Clamp(verticalRotation, -lookLimit, lookLimit);
        playerCamera.localRotation = Quaternion.Euler(verticalRotation, 0f, 0f);

    }

    void HandlePlayerInteraction()
    {
        if (grabbedObject != null)
        {
            if (Mouse.current.leftButton.wasPressedThisFrame)
            {
                DropObject();
            }
            return;
        }


        Ray ray = new Ray(playerCamera.position, playerCamera.forward);
        RaycastHit hit;

        if (Physics.Raycast(ray, out hit, interactionDistance))
        {
            Debug.Log("Hit object: " + hit.collider.gameObject.name);
            Ingredient ingredient = hit.collider.GetComponent<Ingredient>();

            if (ingredient != null)
            {
                // Handle ingredient interaction
                Debug.Log($"Interacting with ingredient: {ingredient.ingredientName}");
                if (Mouse.current.leftButton.wasPressedThisFrame)
                {
                    grabbedObject = hit.collider.gameObject;
                    GrabObject();
                }
            }
        }
    }

    void GrabObject()
    {
        Rigidbody rb = grabbedObject.GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.isKinematic = true;
            rb.useGravity = false;
        }
        grabbedObject.transform.position = grabbedObjectPosition.position;
        grabbedObject.transform.rotation = grabbedObjectPosition.rotation;

        grabbedObject.transform.SetParent(grabbedObjectPosition);
    }

    void DropObject()
    {
        grabbedObject.transform.SetParent(null);
        Rigidbody rb = grabbedObject.GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.isKinematic = false;
            rb.useGravity = true;
            rb.WakeUp();
        }
        Debug.Log("Dropped object");
        grabbedObject = null;
    }

    void OnDrawGizmos()
    {
        Gizmos.color = Color.cyan;
        Gizmos.DrawRay(playerCamera.position, playerCamera.forward * interactionDistance);
        Gizmos.color = Color.darkBlue;
        Gizmos.DrawWireSphere(grabbedObjectPosition.position, 1f);
    }

    void OnDisable()
    {
        controls.Disable();
    }

}
