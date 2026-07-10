using UnityEngine;
using UnityEngine.InputSystem;

public class MouseLook : MonoBehaviour
{
    // Configuración
    public float mouseSensitivity = 100f;
    public Transform playerBody;
    public SubstanceManager substanceManager;
    
    // Privados
    private float xRotation = 0f;
    private InputSystem_Actions inputActions;

    void Awake()
    {
        inputActions = new InputSystem_Actions();
        
        if (playerBody == null)
        {
            playerBody = transform.parent;
            if (playerBody == null)
            {
                Debug.LogError("MouseLook necesita referencia al Player Body!");
            }
        }
        
        if (substanceManager == null)
            substanceManager = FindFirstObjectByType<SubstanceManager>();
    }

    void OnEnable()
    {
        inputActions.Player.Enable();
        // Cursor.lockState = CursorLockMode.Locked;
    }

    void OnDisable()
    {
        inputActions.Player.Disable();
    }

    void Update()
    {
        if (GameManager.Instance != null && !GameManager.Instance.IsGameActive)
            return;
        
        UpdateCursorState();
        
        // if (Cursor.lockState != CursorLockMode.Locked)
        //     return;

        Vector2 lookInput = inputActions.Player.Look.ReadValue<Vector2>();
        
        float mouseX = lookInput.x * mouseSensitivity * Time.deltaTime;
        float mouseY = lookInput.y * mouseSensitivity * Time.deltaTime;

        xRotation -= mouseY;
        xRotation = Mathf.Clamp(xRotation, -90f, 90f);

        transform.localRotation = Quaternion.Euler(xRotation, 0f, 0f);
        
        if (playerBody != null)
        {
            playerBody.Rotate(Vector3.up * mouseX);
        }
    }

    void UpdateCursorState()
    {
        if (substanceManager == null) return;
        
        bool hasActiveEffect = substanceManager.GetCurrentSubstance() != SubstanceType.None;
        
        if (hasActiveEffect)
        {
            // Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }
        else
        {
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }
    }
}
