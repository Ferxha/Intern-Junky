using UnityEditor;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerMovement : MonoBehaviour
{
    //Script movimiento y acciones del jugador
    [SerializeField] private float moveSpeed = 5f;

    public float cameraLookSensitivity = 100f;
    private float horizontalRotation = 0f;
    private float verticalRotation = 0f;
    [SerializeField] private float lookLimit = 90f;
    [SerializeField] private Transform playerCamera;

    [SerializeField] private float interactionDistance = 3f;
    [SerializeField] private Transform grabbedObjectPosition;

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
    
    
    void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = true;
    }

    void Update()
    {
        HandleCameraLookMovement();
        HandlePlayerInteraction();
    }

    //Se mueve movimiento a Fixed Update para mayor precisión
    void FixedUpdate()
    {
        HandlePlayerMovement();
    }

    //Método encargado del movimiento del jugador
    void HandlePlayerMovement()
    {
        Vector2 moveInput = controls.Player.Move.ReadValue<Vector2>();
        Vector3 moveDirection = moveInput.x * transform.right + moveInput.y * transform.forward;

        characterController.Move(moveDirection * moveSpeed * Time.fixedDeltaTime);
    }

    //Método encargado del movimiento de la cámara
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

    //Método encargado de las interacciones del jugador manda a llamar GrabObject y DropObject
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

    //GrabObject: Selecciona un objeto y lo anida al jugador
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

    //DropObject: Suelta el objeto, ya no es hijo del jugador y recupera sus características del rigidbody originales
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

    //Gizmos de control
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
