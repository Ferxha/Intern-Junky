using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerMovement : MonoBehaviour
{
    //Script movimiento y acciones del jugador
    [SerializeField] private float moveSpeed = 5f;
    [SerializeField] private float cameraLookSensitivity = 100f;
    [SerializeField] private float lookLimit = 90f;
    [SerializeField] private Transform playerCamera;
    [SerializeField] private float interactionDistance = 3f;
    [SerializeField] private Transform grabbedObjectPosition;
    [SerializeField] private float inputSmoothSpeed = 10f;

    private float verticalRotation = 0f;
    private GameObject grabbedObject = null;
    private CharacterController characterController;
    private InputSystem_Actions controls;
    private SubstanceManager substanceManager;
    private Vector2 smoothedMoveInput;
    private bool wasGameActive;
    private bool cursorStateInitialized;

    void Awake()
    {
        controls = new InputSystem_Actions();
        characterController = GetComponent<CharacterController>();
        substanceManager = FindFirstObjectByType<SubstanceManager>();
    }

    void OnEnable()
    {
        controls.Enable();
    }
    
    void Start()
    {
        UpdateCursorState();
    }

    void Update()
    {
        if (!ShouldHandleGameplay())
        {
            UpdateCursorState();
            return;
        }

        UpdateCursorState();
        HandleCameraLookMovement();
        HandlePlayerInteraction();
    }

    //Se mueve movimiento a Fixed Update para mayor precisión
    void FixedUpdate()
    {
        if (!ShouldHandleGameplay())
        {
            return;
        }

        HandlePlayerMovement();
    }

    //Método encargado del movimiento del jugador
    void HandlePlayerMovement()
    {
        Vector2 moveInput = controls.Player.Move.ReadValue<Vector2>();

        if (substanceManager != null && substanceManager.GetCurrentSubstance() == SubstanceType.Extasis)
        {
            smoothedMoveInput = Vector2.Lerp(smoothedMoveInput, moveInput, inputSmoothSpeed * 0.3f * Time.fixedDeltaTime);
        }
        else
        {
            smoothedMoveInput = Vector2.Lerp(smoothedMoveInput, moveInput, inputSmoothSpeed * Time.fixedDeltaTime);
        }

        Vector3 moveDirection = smoothedMoveInput.x * transform.right + smoothedMoveInput.y * transform.forward;

        characterController.Move(moveDirection * GetCurrentMoveSpeed() * Time.fixedDeltaTime);
    }

    //Método encargado del movimiento de la cámara
    void HandleCameraLookMovement()
    {
        Vector2 lookInput = controls.Player.Look.ReadValue<Vector2>();
        
        float mouseX = lookInput.x * cameraLookSensitivity * Time.deltaTime;
        float mouseY = lookInput.y * cameraLookSensitivity * Time.deltaTime;

        transform.Rotate(Vector3.up * mouseX);

        verticalRotation -= mouseY;
        verticalRotation = Mathf.Clamp(verticalRotation, -lookLimit, lookLimit);
        playerCamera.localRotation = Quaternion.Euler(verticalRotation, 0f, 0f);

    }

    //Método encargado de las interacciones del jugador manda a llamar GrabObject y DropObject
    void HandlePlayerInteraction()
    {
        if (grabbedObject != null)
        {
            if (Mouse.current != null && Mouse.current.leftButton.wasPressedThisFrame)
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

            SubstanceObject substanceObject = hit.collider.GetComponent<SubstanceObject>();
            if (substanceObject != null)
            {
                if (Mouse.current != null && Mouse.current.leftButton.wasPressedThisFrame)
                {
                    substanceObject.TryConsume();
                }
                return;
            }

            Ingredient ingredient = hit.collider.GetComponent<Ingredient>();

            if (ingredient != null)
            {
                // Handle ingredient interaction
                Debug.Log($"Interacting with ingredient: {ingredient.ingredientName}");
                if (Mouse.current != null && Mouse.current.leftButton.wasPressedThisFrame)
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

    private bool ShouldHandleGameplay()
    {
        if (GameManager.Instance == null)
        {
            return true;
        }

        return GameManager.Instance.IsGameplayInputEnabled;
    }

    private float GetCurrentMoveSpeed()
    {
        if (substanceManager == null)
        {
            substanceManager = SubstanceManager.Instance != null
                ? SubstanceManager.Instance
                : FindFirstObjectByType<SubstanceManager>();
        }

        if (substanceManager == null)
        {
            return moveSpeed;
        }

        return substanceManager.GetCurrentSubstance() switch
        {
            SubstanceType.Coca => moveSpeed * substanceManager.GetCocaSpeedMultiplier(),
            SubstanceType.Extasis => moveSpeed * substanceManager.GetExtasisSpeedMultiplier(),
            _ => moveSpeed
        };
    }

    private void UpdateCursorState()
    {
        bool isGameActive = ShouldHandleGameplay();

        if (cursorStateInitialized && wasGameActive == isGameActive)
        {
            return;
        }

        cursorStateInitialized = true;
        wasGameActive = isGameActive;
        Cursor.lockState = isGameActive ? CursorLockMode.Locked : CursorLockMode.None;
        Cursor.visible = !isGameActive;
    }
}
