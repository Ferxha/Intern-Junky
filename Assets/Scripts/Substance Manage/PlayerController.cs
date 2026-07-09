using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerController : MonoBehaviour
{
    // Configuración
    public float baseSpeed = 5f;
    public float gravity = -9.81f;
    public Transform groundCheck;
    public float groundDistance = 0.4f;
    public LayerMask groundMask;
    
    // Componentes
    public CharacterController controller;
    public SubstanceManager substanceManager;
    
    // Privados
    private Vector3 velocity;
    private bool isGrounded;
    private InputSystem_Actions inputActions;
    private float currentSpeedMultiplier = 1f;
    private Vector2 smoothedMoveInput;
    private float inputSmoothSpeed = 10f;

    void Awake()
    {
        inputActions = new InputSystem_Actions();
        
        if (controller == null)
            controller = GetComponent<CharacterController>();
        
        if (substanceManager == null)
            substanceManager = FindFirstObjectByType<SubstanceManager>();
    }

    void OnEnable()
    {
        inputActions.Player.Enable();
    }

    void OnDisable()
    {
        inputActions.Player.Disable();
    }

    void Update()
    {
        if (GameManager.Instance != null && !GameManager.Instance.IsGameActive)
            return;
        
        CheckGround();
        HandleMovement();
        ApplyGravity();
        UpdateSpeedMultiplier();
    }

    void CheckGround()
    {
        if (groundCheck != null)
        {
            isGrounded = Physics.CheckSphere(groundCheck.position, groundDistance, groundMask);
        }
        else
        {
            isGrounded = controller.isGrounded;
        }

        if (isGrounded && velocity.y < 0)
        {
            velocity.y = -2f;
        }
    }

    void HandleMovement()
    {
        Vector2 moveInput = inputActions.Player.Move.ReadValue<Vector2>();
        
        // Si está bajo efectos de Extasis, aplicar delay en el input
        if (substanceManager != null && substanceManager.GetCurrentSubstance() == SubstanceType.Extasis)
        {
            float smoothFactor = inputSmoothSpeed * 0.3f; // Más lento = más delay
            smoothedMoveInput = Vector2.Lerp(smoothedMoveInput, moveInput, smoothFactor * Time.deltaTime);
        }
        else
        {
            // Sin efectos o con Coca, input directo
            smoothedMoveInput = Vector2.Lerp(smoothedMoveInput, moveInput, inputSmoothSpeed * Time.deltaTime);
        }
        
        Vector3 move = transform.right * smoothedMoveInput.x + transform.forward * smoothedMoveInput.y;
        
        float finalSpeed = baseSpeed * currentSpeedMultiplier;
        controller.Move(move * finalSpeed * Time.deltaTime);
    }

    void ApplyGravity()
    {
        velocity.y += gravity * Time.deltaTime;
        controller.Move(velocity * Time.deltaTime);
    }

    void UpdateSpeedMultiplier()
    {
        if (substanceManager == null) return;

        SubstanceType activeSubstance = substanceManager.GetCurrentSubstance();
        
        switch (activeSubstance)
        {
            case SubstanceType.Coca:
                currentSpeedMultiplier = substanceManager.GetCocaSpeedMultiplier();
                break;
            
            case SubstanceType.Extasis:
                currentSpeedMultiplier = substanceManager.GetExtasisSpeedMultiplier();
                break;
            
            default:
                currentSpeedMultiplier = 1f;
                break;
        }
    }
}
