using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.InputSystem;
using TMPro;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    // Configuración
    public float gameOverDelay = 1f;
    public bool autoRestartOnGameOver = false;
    public FadeController fadeController;
    public float intoxicationFadeDuration = 1f;
    public GameObject gameOverUI;
    public GameObject winUI;
    public GameObject titleScreen;
    public int orderCount = 5;
    private int currentOrderCount = 0;
    public TextMeshProUGUI orderCountText;
    public TextMeshProUGUI CocaCountText;
    public TextMeshProUGUI ExtasisCountText;
    public GameObject hudPanel;
    
    // Estado
    private bool isGameActive;
    public bool IsGameActive => isGameActive;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
        
        if (fadeController == null)
            fadeController = FindFirstObjectByType<FadeController>();
    }

    void Start()
    {
        UpdateSubstanceCounters();
        UpdateOrderCounter();
    }

    void OnDestroy()
    {
        if (Instance == this)
        {
            Instance = null;
        }
    }

    public void StartGame(int difficulty)
    {
        CancelInvoke();
        Time.timeScale = 1f;
        orderCount += difficulty;
        currentOrderCount = 0;
        isGameActive = true;

        if (gameOverUI != null)
        {
            gameOverUI.SetActive(false);
        }

        if (winUI != null)
        {
            winUI.SetActive(false);
        }
        
        if (titleScreen != null)
        {
            titleScreen.SetActive(false);
        }
        
        ShowHUD();
        
        if (fadeController != null)
        {
            fadeController.FadeToClear();
        }

        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    public void ShowHUD()
    {
        if (hudPanel != null)
        {
            hudPanel.SetActive(true);
        }

        UpdateOrderCounter();
        UpdateSubstanceCounters();
    }

    public void UpdateOrderCounter()
    {
        if (orderCountText != null)
        {
            orderCountText.text = $"Orders: {currentOrderCount}/{orderCount}";
        }
    }

    public void UpdateSubstanceCounters()
    {
        SubstanceManager substanceManager = SubstanceManager.Instance != null
            ? SubstanceManager.Instance
            : FindFirstObjectByType<SubstanceManager>();

        if (substanceManager == null)
        {
            return;
        }

        if (CocaCountText != null)
        {
            CocaCountText.text = $"{substanceManager.GetCocaAmount()} Coca";
        }

        if (ExtasisCountText != null)
        {
            ExtasisCountText.text = $"{substanceManager.GetExtasisAmount()} Extasis";
        }
    }

    public void SpawnOrder()
    {
        while (isGameActive)
        {
            currentOrderCount++;
            if (currentOrderCount == orderCount)
            {
                // Debug.Log("¡Has completado todas las órdenes!");
                if (winUI != null)
                {
                    winUI.SetActive(true);
                }
                return;

            }
        }
    }

    public void GameOverIntoxication()
    {
        if (!isGameActive) return;
        
        isGameActive = false;
        Debug.Log("<color=red>═══════════════════════════════</color>");
        Debug.Log("<color=red>  GAME OVER: INTOXICACIÓN</color>");
        Debug.Log("<color=red>  Mezclaste sustancias!</color>");
        Debug.Log("<color=red>═══════════════════════════════</color>");
        
        if (fadeController != null)
        {
            fadeController.FadeToBlack(intoxicationFadeDuration);
        }
        else
        {
            Debug.LogWarning("FadeController no encontrado.");
        }
        
        Cursor.lockState = CursorLockMode.Locked;
        HandleGameOver();
    }

    public void GameOverAbstinence()
    {
        if (!isGameActive) return;
        
        isGameActive = false;
        Debug.Log("<color=yellow>═══════════════════════════════</color>");
        Debug.Log("<color=yellow>  GAME OVER: ABSTINENCIA</color>");
        Debug.Log("<color=yellow>  Te desmayaste!</color>");
        Debug.Log("<color=yellow>═══════════════════════════════</color>");        
        
        HandleGameOver();
    }

    private void HandleGameOver()
    {
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
        if (hudPanel != null)
        {
            hudPanel.SetActive(false);
        }
        
        if (gameOverUI != null)
        {
            Invoke(nameof(ShowGameOverUI), gameOverDelay);
        }
        
        if (autoRestartOnGameOver)
        {
            Invoke(nameof(RestartGame), gameOverDelay + 2f);
        }
        else
        {
            Debug.Log("Click en Restart o presiona R para reiniciar");
        }

        Cursor.lockState = CursorLockMode.None;
    }

    private void ShowGameOverUI()
    {
        if (gameOverUI != null)
        {
            gameOverUI.transform.SetAsLastSibling();
            if (hudPanel != null)
            {
                hudPanel.SetActive(false);
            }
            gameOverUI.SetActive(true);
        }
    }

    public void RestartGame()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    void Update()
    {
        if (!isGameActive && Keyboard.current != null && Keyboard.current.rKey.wasPressedThisFrame)
        {
            RestartGame();
        }
    }
}
