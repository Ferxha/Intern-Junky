using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.UI;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    [Header("Game Over")]
    [SerializeField] private float gameOverDelay = 1f;
    [SerializeField] private bool autoRestartOnGameOver = false;
    [SerializeField] private float intoxicationFadeDuration = 1f;

    [Header("UI")]
    [SerializeField] private FadeController fadeController;
    [SerializeField] private GameObject gameOverUI;
    [SerializeField] private GameObject winUI;
    [SerializeField] private GameObject titleScreen;
    [SerializeField] private GameObject hudPanel;
    [SerializeField] private TextMeshProUGUI orderCountText;
    [SerializeField] private TextMeshProUGUI CocaCountText;
    [SerializeField] private TextMeshProUGUI ExtasisCountText;

    [Header("Orders")]
    [SerializeField] private int orderCount = 5;
    [SerializeField] private float orderFadeToBlackDuration = 1.5f;
    [SerializeField] private float orderDisplayDuration = 3.5f;
    [SerializeField] private float orderFadeToClearDuration = 1f;

    [Header("Difficulty")]
    [SerializeField] private float gameDuration = 120f;
    [SerializeField] private float warningDisplayDuration = 4f;

    private int currentOrderCount = 0;
    private int targetOrderCount;
    private int refillEveryOrders = 3;
    private float remainingGameTime;
    private bool isGameActive;
    private bool isPresentingOrder;
    private string currentOrderName;
    private GameObject ordersPanel;
    private GameObject warningPanel;
    private TextMeshProUGUI orderText;
    private TextMeshProUGUI timerText;
    private static bool warningShownThisPlaySession;

    public bool IsGameActive => isGameActive;
    public bool IsGameplayInputEnabled => isGameActive && !isPresentingOrder;
    public FadeController FadeController => fadeController;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
            return;
        }

        if (fadeController == null)
        {
            fadeController = FindFirstObjectByType<FadeController>();
        }

        FindMissingUIReferences();
        EnsureEventSystemExists();
    }

    void Start()
    {
        targetOrderCount = orderCount;
        remainingGameTime = gameDuration;
        SetMenuCursor();
        HideOrdersPanel();
        ShowWarningOnce();
        UpdateSubstanceCounters();
        UpdateOrderCounter();
        UpdateTimerText();
    }
    // Asegúrate de que la instancia se borre cuando se destruya el GameManager.
    void OnDestroy()
    {
        if (Instance == this)
        {
            Instance = null;
        }
    }

    void Update()
    {
        if (isGameActive && !isPresentingOrder)
        {
            UpdateGameTimer();
        }

        if (!isGameActive && Keyboard.current != null && Keyboard.current.rKey.wasPressedThisFrame)
        {
            RestartGame();
        }
    }
    // Método público para iniciar el juego con un nivel de dificultad determinado
    public void StartGame(int difficulty)
    {
        if (isGameActive)
        {
            return;
        }

        CancelInvoke();
        Time.timeScale = 1f;
        ConfigureDifficulty(difficulty);
        currentOrderCount = 0;
        remainingGameTime = gameDuration;
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
        SetGameplayCursor();
        PresentCurrentOrder();
    }
    // Mostrar el HUD y actualizar los contadores de pedidos y de sustancias
    public void ShowHUD()
    {
        if (hudPanel != null)
        {
            hudPanel.SetActive(true);
        }

        UpdateOrderCounter();
        UpdateSubstanceCounters();
    }
    // Actualizar el contador de pedidos en la interfaz de usuario
    public void UpdateOrderCounter()
    {
        if (orderCountText != null)
        {
            orderCountText.text = $" {currentOrderCount}/{targetOrderCount}";
        }
    }
    // Actualizar los contadores de sustancias en la interfaz de usuario
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
    // Método público para establecer el pedido actual y actualizar la interfaz de usuario
    public void SpawnOrder()
    {
        CompleteCurrentOrder();
    }
    // Establece el nombre del pedido actual y actualiza la interfaz de usuario
    public void SetCurrentOrder(string orderName)
    {
        currentOrderName = orderName;

        if (orderText != null)
        {
            orderText.text = currentOrderName;
        }
    }
    // Completa el pedido actual, actualiza los contadores y maneja la finalización del juego si se alcanza el objetivo
    public bool CompleteCurrentOrder()
    {
        if (!isGameActive)
        {
            return false;
        }

        currentOrderCount++;
        UpdateOrderCounter();

        RefillSubstancesIfNeeded();

        if (currentOrderCount < targetOrderCount)
        {
            return true;
        }

        isGameActive = false;
        isPresentingOrder = false;
        HideOrdersPanel();

        if (hudPanel != null)
        {
            hudPanel.SetActive(false);
        }

        if (winUI != null)
        {
            winUI.SetActive(true);
        }

        SetMenuCursor();
        return false;
    }
    // Presenta el pedido actual en la interfaz de usuario si el juego está activo y hay un pedido válido
    public void PresentCurrentOrder()
    {
        if (!isGameActive || string.IsNullOrWhiteSpace(currentOrderName))
        {
            return;
        }

        StopCoroutine(nameof(PresentOrderRoutine));
        StartCoroutine(PresentOrderRoutine());
    }
    // Maneja el final del juego por intoxicación, mostrando la pantalla de fin de juego y aplicando un efecto de desvanecimiento
    public void GameOverIntoxication()
    {
        if (!isGameActive)
        {
            return;
        }

        isGameActive = false;
        isPresentingOrder = false;
        HideOrdersPanel();
        Debug.Log("GAME OVER: INTOXICACION. Mezclaste sustancias.");

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
    // Maneja el final del juego por abstinencia, mostrando la pantalla de fin de juego
    public void GameOverAbstinence()
    {
        if (!isGameActive)
        {
            return;
        }

        isGameActive = false;
        isPresentingOrder = false;
        HideOrdersPanel();
        Debug.Log("GAME OVER: ABSTINENCIA. Te desmayaste.");
        HandleGameOver();
    }
    // Maneja el final del juego por receta incorrecta, mostrando la pantalla de fin de juego y aplicando un efecto de desvanecimiento
    public void GameOverWrongRecipe()
    {
        if (!isGameActive)
        {
            return;
        }

        isGameActive = false;
        isPresentingOrder = false;
        HideOrdersPanel();
        Debug.Log("GAME OVER: RECETA INCORRECTA.");

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
    // Reinicia el juego cargando la escena actual y restableciendo la escala de tiempo
    public void RestartGame()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }
    // Maneja la lógica de finalización del juego, mostrando la interfaz de usuario correspondiente y configurando el cursor
    private void HandleGameOver()
    {
        SetMenuCursor();

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
    }
    // Muestra la interfaz de usuario de fin de juego y oculta el HUD
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
    // Rutina para presentar el pedido actual en la interfaz de usuario, incluyendo efectos de desvanecimiento y temporización
    private IEnumerator PresentOrderRoutine()
    {
        isPresentingOrder = true;
        SetGameplayCursor();

        if (orderText != null)
        {
            orderText.text = currentOrderName;
        }

        if (fadeController != null)
        {
            fadeController.FadeToBlack(orderFadeToBlackDuration);
            yield return new WaitForSeconds(orderFadeToBlackDuration);
        }

        ShowOrdersPanel();
        yield return new WaitForSeconds(orderDisplayDuration);
        HideOrdersPanel();

        if (fadeController != null)
        {
            fadeController.FadeToClear(orderFadeToClearDuration);
            yield return new WaitForSeconds(orderFadeToClearDuration);
        }

        isPresentingOrder = false;
        SetGameplayCursor();
    }
    // Busca referencias de UI faltantes y las asigna si es necesario
    private void FindMissingUIReferences()
    {
        if (ordersPanel == null)
        {
            ordersPanel = FindGameObjectByName("Orders Panel");
        }

        if (warningPanel == null)
        {
            warningPanel = FindGameObjectByName("Warning Panel");
        }

        if (orderText == null)
        {
            GameObject orderTextObject = FindGameObjectByName("Order Text");
            if (orderTextObject != null)
            {
                orderText = orderTextObject.GetComponent<TextMeshProUGUI>();
            }
        }

        if (orderCountText == null)
        {
            orderCountText = FindTextByName("Order Count Text");
        }

        if (CocaCountText == null)
        {
            CocaCountText = FindTextByName("Coca Count Text");
        }

        if (ExtasisCountText == null)
        {
            ExtasisCountText = FindTextByName("Extasis Count Text");
        }

        if (timerText == null)
        {
            timerText = FindTextByName("Timer Text");
        }
    }
    // Busca un objeto de texto por nombre y devuelve su componente TextMeshProUGUI si se encuentra
    private TextMeshProUGUI FindTextByName(string objectName)
    {
        GameObject textObject = FindGameObjectByName(objectName);
        return textObject != null ? textObject.GetComponent<TextMeshProUGUI>() : null;
    }

    private GameObject FindGameObjectByName(string objectName)
    {
        Transform[] transforms = FindObjectsByType<Transform>(FindObjectsInactive.Include, FindObjectsSortMode.None);
        foreach (Transform candidate in transforms)
        {
            if (candidate.name == objectName)
            {
                return candidate.gameObject;
            }
        }

        return null;
    }
    // Muestra el panel de pedidos y lo coloca al frente de la jerarquía de UI
    private void ShowOrdersPanel()
    {
        if (ordersPanel == null)
        {
            return;
        }

        ordersPanel.SetActive(true);
        ordersPanel.transform.SetAsLastSibling();
    }

    private void HideOrdersPanel()
    {
        if (ordersPanel != null)
        {
            ordersPanel.SetActive(false);
        }
    }

    private void ShowWarningOnce()
    {
        if (warningPanel == null)
        {
            return;
        }

        if (warningShownThisPlaySession)
        {
            warningPanel.SetActive(false);
            return;
        }

        warningShownThisPlaySession = true;
        warningPanel.SetActive(true);
        warningPanel.transform.SetAsLastSibling();
        StartCoroutine(HideWarningRoutine());
    }
    // Rutina para ocultar el panel de advertencia después de un tiempo determinado
    private IEnumerator HideWarningRoutine()
    {
        yield return new WaitForSeconds(warningDisplayDuration);

        if (warningPanel != null)
        {
            warningPanel.SetActive(false);
        }
    }

    private void EnsureEventSystemExists()
    {
        EventSystem eventSystem = FindFirstObjectByType<EventSystem>();

        if (eventSystem == null)
        {
            GameObject eventSystemObject = new GameObject("EventSystem");
            eventSystem = eventSystemObject.AddComponent<EventSystem>();
        }

        InputSystemUIInputModule inputModule = eventSystem.GetComponent<InputSystemUIInputModule>();
        if (inputModule == null)
        {
            inputModule = eventSystem.gameObject.AddComponent<InputSystemUIInputModule>();
        }

        if (inputModule.actionsAsset == null)
        {
            inputModule.AssignDefaultActions();
        }
    }
    // Configura el cursor para el modo de juego, bloqueándolo y haciéndolo invisible
    private void SetGameplayCursor()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }
    // Configura el cursor para el modo de menú, desbloqueándolo y haciéndolo visible
    private void SetMenuCursor()
    {
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }
    // Configura la dificultad del juego, ajustando el número de pedidos objetivo y la frecuencia de reabastecimiento de sustancias
    private void ConfigureDifficulty(int difficulty)
    {
        targetOrderCount = orderCount + difficulty;
        refillEveryOrders = difficulty switch
        {
            1 => 2,
            2 => 3,
            3 => 4,
            _ => 3
        };

        SubstanceManager substanceManager = SubstanceManager.Instance != null
            ? SubstanceManager.Instance
            : FindFirstObjectByType<SubstanceManager>();

        if (substanceManager != null)
        {
            substanceManager.ConfigureForDifficulty(difficulty);
        }
    }

    private void UpdateGameTimer()
    {
        remainingGameTime -= Time.deltaTime;
        UpdateTimerText();

        if (remainingGameTime <= 0f)
        {
            GameOverTimeExpired();
        }
    }

    private void UpdateTimerText()
    {
        if (timerText == null)
        {
            return;
        }

        int seconds = Mathf.CeilToInt(Mathf.Max(0f, remainingGameTime));
        int minutes = seconds / 60;
        int remainingSeconds = seconds % 60;
        timerText.text = $"{minutes:00}:{remainingSeconds:00}";
    }
    // Reabastece las sustancias si se ha alcanzado el número de pedidos necesario para reabastecer
    private void RefillSubstancesIfNeeded()
    {
        if (currentOrderCount <= 0 || currentOrderCount % refillEveryOrders != 0)
        {
            return;
        }

        SubstanceManager substanceManager = SubstanceManager.Instance != null
            ? SubstanceManager.Instance
            : FindFirstObjectByType<SubstanceManager>();

        if (substanceManager != null)
        {
            substanceManager.RefillInventory();
        }
    }
    // Maneja el final del juego cuando se agota el tiempo, mostrando la pantalla de fin de juego
    private void GameOverTimeExpired()
    {
        if (!isGameActive)
        {
            return;
        }

        isGameActive = false;
        isPresentingOrder = false;
        HideOrdersPanel();
        Debug.Log("GAME OVER: TIEMPO AGOTADO.");
        HandleGameOver();
    }
}
