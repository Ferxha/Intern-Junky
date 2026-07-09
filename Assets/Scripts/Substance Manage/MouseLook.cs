using UnityEngine;
using UnityEngine.InputSystem;

public class MouseLook : MonoBehaviour
{
    // Configuración
    public float mouseSensitivity = 100f;
    public Transform playerBody;
    
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
        
        if (Cursor.lockState != CursorLockMode.Locked)
        {
            if (Mouse.current.leftButton.wasPressedThisFrame)
            {
                Cursor.lockState = CursorLockMode.Locked;
            }
            return;
        }

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
}
