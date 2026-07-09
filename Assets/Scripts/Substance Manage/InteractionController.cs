using UnityEngine;
using UnityEngine.InputSystem;
using Mouse = UnityEngine.InputSystem.Mouse;

public class InteractionController : MonoBehaviour
{
    // Configuración
    public float interactionDistance = 5f;
    public LayerMask interactableLayer = ~0;
    public Camera playerCamera;
    public bool useScreenCenter = true;
    public SubstanceManager substanceManager;
    public bool showDebugRays = true;
    
    // Privados
    private InputSystem_Actions inputActions;
    private RaycastHit currentHit;
    private bool isProcessingInteraction = false;

    void Awake()
    {
        // Verificar si ya existe otro InteractionController
        InteractionController[] controllers = FindObjectsByType<InteractionController>(FindObjectsSortMode.None);
        if (controllers.Length > 1)
        {
            Debug.LogWarning($"¡ATENCIÓN! Hay {controllers.Length} InteractionControllers en la escena. Solo debería haber UNO.");
        }

        inputActions = new InputSystem_Actions();
        
        if (playerCamera == null)
            playerCamera = Camera.main;
        
        if (substanceManager == null)
            substanceManager = FindFirstObjectByType<SubstanceManager>();
    }

    void OnEnable()
    {
        inputActions.Player.Enable();
        inputActions.Player.Attack.performed += OnInteract;
    }

    void OnDisable()
    {
        inputActions.Player.Attack.performed -= OnInteract;
        inputActions.Player.Disable();
    }

    void Update()
    {
        if (showDebugRays)
        {
            UpdateRaycastPreview();
        }
    }

    private void OnInteract(InputAction.CallbackContext context)
    {
        if (isProcessingInteraction) return;
        
        if (GameManager.Instance != null && !GameManager.Instance.IsGameActive)
            return;
        
        PerformRaycast();
    }

    private void PerformRaycast()
    {
        if (playerCamera == null)
        {
            Debug.LogError("No hay cámara asignada!");
            return;
        }

        isProcessingInteraction = true;

        Vector2 screenPosition = GetScreenPosition();
        Ray ray = playerCamera.ScreenPointToRay(screenPosition);
        
        if (Physics.Raycast(ray, out currentHit, interactionDistance, interactableLayer))
        {
            ProcessInteraction(currentHit);
        }
        else
        {
            Debug.Log("No se detectó ningún objeto interactuable");
        }

        isProcessingInteraction = false;
    }

    private void ProcessInteraction(RaycastHit hit)
    {
        GameObject hitObject = hit.collider.gameObject;
        
        Debug.Log($"Interactuando con: {hitObject.name} (Tag: {hitObject.tag})");
        
        switch (hitObject.tag)
        {
            case "Coca":
                if (substanceManager.ConsumeCoca())
                {
                    Debug.Log($"<color=cyan>Consumiste Coca</color>");
                }
                break;
                
            case "Extasis":
                if (substanceManager.ConsumeExtasis())
                {
                    Debug.Log($"<color=magenta>Consumiste Éxtasis</color>");
                }
                break;
                
            default:
                Debug.Log($"Click en objeto no interactuable: {hitObject.name}");
                break;
        }
    }
    // }

    private void UpdateRaycastPreview()
    {
        if (playerCamera == null) return;
        
        Vector2 screenPosition = GetScreenPosition();
        Ray ray = playerCamera.ScreenPointToRay(screenPosition);
        
        if (Physics.Raycast(ray, out RaycastHit hit, interactionDistance, interactableLayer))
        {
            Debug.DrawRay(ray.origin, ray.direction * hit.distance, Color.green);
        }
        else
        {
            Debug.DrawRay(ray.origin, ray.direction * interactionDistance, Color.red);
        }
    }

    private Vector2 GetScreenPosition()
    {
        if (useScreenCenter)
        {
            return new Vector2(Screen.width * 0.5f, Screen.height * 0.5f);
        }
        else
        {
            if (Mouse.current != null)
            {
                return Mouse.current.position.ReadValue();
            }
            else
            {
                Debug.LogWarning("No se detectó mouse, usando centro de pantalla");
                return new Vector2(Screen.width * 0.5f, Screen.height * 0.5f);
            }
        }
    }

    public void SetSubstanceManager(SubstanceManager manager)
    {
        substanceManager = manager;
    }
}
