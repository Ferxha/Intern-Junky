using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.InputSystem;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    // Configuración
    public float gameOverDelay = 2f;
    public bool autoRestartOnGameOver = false;
    public FadeController fadeController;
    public float intoxicationFadeDuration = 3f;
    public GameObject gameOverUI;
    
    // Estado
    private bool isGameActive = true;
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
        
        if (fadeController != null)
        {
            fadeController.FadeToBlack(intoxicationFadeDuration);
        }
        
        HandleGameOver();
    }

    public void GameOverNoSubstances()
    {
        if (!isGameActive) return;
        
        isGameActive = false;
        Debug.Log("<color=orange>═══════════════════════════════</color>");
        Debug.Log("<color=orange>  GAME OVER: SIN SUSTANCIAS</color>");
        Debug.Log("<color=orange>  Te quedaste sin drogas!</color>");
        Debug.Log("<color=orange>═══════════════════════════════</color>");
        
        HandleGameOver();
    }

    private void HandleGameOver()
    {
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
        
        if (gameOverUI != null)
        {
            Invoke(nameof(ShowGameOverUI), gameOverDelay);
        }
        
        if (autoRestartOnGameOver)
        {
            Invoke(nameof(RestartGame), gameOverDelay + 3f);
        }
        else
        {
            Debug.Log("Click en Restart o presiona R para reiniciar");
        }
    }

    private void ShowGameOverUI()
    {
        if (gameOverUI != null)
        {
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
