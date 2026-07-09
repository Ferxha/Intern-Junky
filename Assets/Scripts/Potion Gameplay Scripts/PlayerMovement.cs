using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerMovement : MonoBehaviour
{
    [SerializeField] private float moveSpeed = 5f;

    [SerializeField] private float cameraLookSensitivity = 2f;
    [SerializeField] private float sideLookLimit = 90f;
    [SerializeField] private Transform playerCamera;

    [SerializeField] private float interactionDistance = 3f;
    [SerializeField] private Transform grabbedObjectPosition;

    
    private float horizontalRotation = 0f;
    private float verticalRotation = 0f;

    private GameObject grabbedObject = null;

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
        Cursor.visible = true;
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
        if (grabbedObject != null)
        {
            if (controls.Player.Interact.WasPressedThisFrame())
            {
                grabbedObject.transform.SetParent(null);

                Rigidbody rb = grabbedObject.GetComponent<Rigidbody>();
                if (rb != null)
                {
                    rb.isKinematic = false;
                    rb.useGravity = true;
                }
                return;
            }
            
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
                if (controls.Player.Interact.WasPressedThisFrame())
                {
                    grabbedObject = hit.collider.gameObject;
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
            }
        }
    }

    void OnDrawGizmos()
    {
        Gizmos.color = Color.cyan;
        Gizmos.DrawRay(playerCamera.position, playerCamera.forward * interactionDistance);
    }

    void OnDisable()
    {
        controls.Disable();
    }

}
