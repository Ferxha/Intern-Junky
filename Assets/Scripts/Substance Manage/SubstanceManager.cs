using System.Collections;
using UnityEngine;
using UnityEngine.Events;

public enum SubstanceType
{
    None,
    Coca,
    Extasis
}

public class SubstanceManager : MonoBehaviour
{
    public static SubstanceManager Instance { get; private set; }

    [Header("Inventory")]
    [SerializeField] private int initialCocaAmount = 5;
    [SerializeField] private int initialExtasisAmount = 5;

    [Header("Effect Timing")]
    [SerializeField] private float effectDuration = 18f;
    [SerializeField] private float abstinenceTime = 20f;
    [SerializeField] private float abstinenceFadeDuration = 20f;
    [SerializeField] private float consumptionFadeToBlackDuration = 1.25f;
    [SerializeField] private float consumptionFadeToClearDuration = 0.75f;

    [Header("Risk")]
    [SerializeField] private float intoxicationProbability = 0.3f;
    [SerializeField] private float intoxicationDuration = 5f;

    [Header("Movement")]
    [SerializeField] private float cocaSpeedMultiplier = 1.5f;
    [SerializeField] private float extasisSpeedMultiplier = 0.7f;

    [Header("References")]
    [SerializeField] private VisualEffectsController visualEffects;
    [SerializeField] private GameManager gameManager;

    [Header("Events")]
    [SerializeField] private UnityEvent<SubstanceType> OnSubstanceConsumed;
    [SerializeField] private UnityEvent OnEffectEnded;
    [SerializeField] private UnityEvent OnIntoxication;

    private SubstanceType currentActiveSubstance = SubstanceType.None;
    private float currentEffectTimer = 0f;
    private int cocaInventory;
    private int extasisInventory;
    private bool isIntoxicated = false;
    private float timeWithoutEffect = 0f;
    private bool abstinenceFadeStarted = false;
    private bool isActivatingSubstance = false;

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

        cocaInventory = initialCocaAmount;
        extasisInventory = initialExtasisAmount;

        if (visualEffects == null)
        {
            visualEffects = GetComponent<VisualEffectsController>();
        }

        if (gameManager == null)
        {
            gameManager = FindFirstObjectByType<GameManager>();
        }
    }

    void OnDestroy()
    {
        if (Instance == this)
        {
            Instance = null;
        }
    }

    void Update()
    {
        if (gameManager != null && !gameManager.IsGameplayInputEnabled)
        {
            return;
        }

        if (isActivatingSubstance)
        {
            return;
        }

        if (currentActiveSubstance != SubstanceType.None && !isIntoxicated)
        {
            currentEffectTimer -= Time.deltaTime;
            timeWithoutEffect = 0f;
            abstinenceFadeStarted = false;

            if (currentEffectTimer <= 0f)
            {
                EndEffect();
            }
        }
        else if (!isIntoxicated)
        {
            timeWithoutEffect += Time.deltaTime;

            if (!abstinenceFadeStarted && timeWithoutEffect >= 0.1f)
            {
                StartAbstinenceFade();
            }

            if (timeWithoutEffect >= abstinenceTime)
            {
                TriggerAbstinence();
            }
        }
    }

    public bool ConsumeCoca()
    {
        return ConsumeSubstance(SubstanceType.Coca);
    }

    public bool ConsumeExtasis()
    {
        return ConsumeSubstance(SubstanceType.Extasis);
    }

    public int GetCocaAmount() => cocaInventory;
    public int GetExtasisAmount() => extasisInventory;
    public SubstanceType GetCurrentSubstance() => currentActiveSubstance;
    public float GetEffectTimeRemaining() => currentEffectTimer;
    public bool IsIntoxicated() => isIntoxicated;
    public float GetCocaSpeedMultiplier() => cocaSpeedMultiplier;
    public float GetExtasisSpeedMultiplier() => extasisSpeedMultiplier;

    public void ConfigureForDifficulty(int difficulty)
    {
        ResetEffectState();
        RefillInventory();

        switch (difficulty)
        {
            case 1:
                effectDuration = 20f;
                abstinenceTime = 24f;
                abstinenceFadeDuration = 24f;
                intoxicationProbability = 0.2f;
                break;
            case 2:
                effectDuration = 18f;
                abstinenceTime = 21f;
                abstinenceFadeDuration = 21f;
                intoxicationProbability = 0.3f;
                break;
            case 3:
                effectDuration = 16f;
                abstinenceTime = 18f;
                abstinenceFadeDuration = 18f;
                intoxicationProbability = 0.4f;
                break;
        }

        consumptionFadeToBlackDuration = 1.25f;
        consumptionFadeToClearDuration = 0.75f;
    }

    public void RefillInventory()
    {
        cocaInventory = initialCocaAmount;
        extasisInventory = initialExtasisAmount;

        if (gameManager != null)
        {
            gameManager.UpdateSubstanceCounters();
        }

        RefreshSubstanceObjects();
    }

    private bool ConsumeSubstance(SubstanceType type)
    {
        if (GetInventoryAmount(type) <= 0)
        {
            Debug.LogWarning($"No hay {type} disponible en el inventario");
            return false;
        }

        if (isIntoxicated)
        {
            Debug.LogWarning("No puedes consumir mas sustancias mientras estas intoxicado");
            return false;
        }

        if (currentActiveSubstance == SubstanceType.None)
        {
            StartActivationAfterConsumptionFade(type);
            DeductFromInventory(type);
        }
        else if (currentActiveSubstance == type)
        {
            ProlongEffectAfterConsumptionFade();
            DeductFromInventory(type);
        }
        else
        {
            Debug.Log($"Mezclaste {currentActiveSubstance} con {type}!");
            TriggerIntoxication();
            DeductFromInventory(type);
        }

        return true;
    }

    private int GetInventoryAmount(SubstanceType type)
    {
        return type switch
        {
            SubstanceType.Coca => cocaInventory,
            SubstanceType.Extasis => extasisInventory,
            _ => 0
        };
    }

    private void DeductFromInventory(SubstanceType type)
    {
        if (type == SubstanceType.Coca)
        {
            cocaInventory--;
        }
        else if (type == SubstanceType.Extasis)
        {
            extasisInventory--;
        }

        if (gameManager != null)
        {
            gameManager.UpdateSubstanceCounters();
        }

        RefreshSubstanceObjects();
    }

    private void StartActivationAfterConsumptionFade(SubstanceType type)
    {
        isActivatingSubstance = true;
        timeWithoutEffect = 0f;
        abstinenceFadeStarted = false;

        if (gameManager != null && gameManager.FadeController != null)
        {
            gameManager.FadeController.StopFade();
            gameManager.FadeController.FadeToBlackAndClear(
                consumptionFadeToBlackDuration,
                consumptionFadeToClearDuration,
                () => ActivateEffect(type));
        }
        else
        {
            ActivateEffect(type);
        }
    }

    private void ActivateEffect(SubstanceType type)
    {
        isActivatingSubstance = false;
        currentActiveSubstance = type;
        currentEffectTimer = effectDuration;
        timeWithoutEffect = 0f;
        abstinenceFadeStarted = false;

        ApplyVisualEffects(type);
        OnSubstanceConsumed?.Invoke(type);

        Debug.Log($"Efecto de {type} activado por {effectDuration}s");
    }

    private void ProlongEffectAfterConsumptionFade()
    {
        isActivatingSubstance = true;
        timeWithoutEffect = 0f;
        abstinenceFadeStarted = false;

        if (gameManager != null && gameManager.FadeController != null)
        {
            gameManager.FadeController.StopFade();
            gameManager.FadeController.FadeToBlackAndClear(
                consumptionFadeToBlackDuration,
                consumptionFadeToClearDuration,
                () =>
                {
                    ProlongEffect();

                    if (CheckIntoxicationRoll())
                    {
                        TriggerIntoxication();
                    }
                });
        }
        else
        {
            ProlongEffect();

            if (CheckIntoxicationRoll())
            {
                TriggerIntoxication();
            }
        }
    }

    private void ProlongEffect()
    {
        isActivatingSubstance = false;
        currentEffectTimer += effectDuration;
        Debug.Log($"Efecto de {currentActiveSubstance} prolongado. Tiempo restante: {currentEffectTimer}s");
    }

    private bool CheckIntoxicationRoll()
    {
        float roll = Random.Range(0f, 1f);
        return roll < intoxicationProbability;
    }

    private void TriggerIntoxication()
    {
        if (isIntoxicated)
        {
            return;
        }

        isIntoxicated = true;
        Debug.Log("INTOXICACION!");

        if (visualEffects != null)
        {
            visualEffects.ApplyIntoxicationEffect();
        }

        OnIntoxication?.Invoke();
        StartCoroutine(HandleIntoxication());
    }

    private IEnumerator HandleIntoxication()
    {
        yield return new WaitForSeconds(intoxicationDuration);

        if (gameManager != null)
        {
            gameManager.GameOverIntoxication();
        }
        else
        {
            Debug.LogError("GameManager no asignado!");
        }
    }

    private void EndEffect()
    {
        Debug.Log($"Efecto de {currentActiveSubstance} terminado");

        currentActiveSubstance = SubstanceType.None;
        currentEffectTimer = 0f;
        timeWithoutEffect = 0f;

        if (visualEffects != null)
        {
            visualEffects.ResetEffects();
        }

        OnEffectEnded?.Invoke();
    }

    private void StartAbstinenceFade()
    {
        abstinenceFadeStarted = true;

        if (gameManager != null && gameManager.FadeController != null)
        {
            Debug.Log("Iniciando fade de abstinencia...");
            gameManager.FadeController.FadeToBlack(abstinenceFadeDuration);
        }
    }

    private void TriggerAbstinence()
    {
        if (gameManager != null)
        {
            gameManager.GameOverAbstinence();
        }
    }

    private void ApplyVisualEffects(SubstanceType type)
    {
        if (visualEffects == null)
        {
            return;
        }

        switch (type)
        {
            case SubstanceType.Coca:
                visualEffects.ApplyCocaEffect();
                break;
            case SubstanceType.Extasis:
                visualEffects.ApplyExtasisEffect();
                break;
        }
    }

    private void ResetEffectState()
    {
        StopAllCoroutines();
        currentActiveSubstance = SubstanceType.None;
        currentEffectTimer = 0f;
        timeWithoutEffect = 0f;
        abstinenceFadeStarted = false;
        isActivatingSubstance = false;
        isIntoxicated = false;

        if (visualEffects != null)
        {
            visualEffects.ResetEffects();
        }
    }

    private void RefreshSubstanceObjects()
    {
        SubstanceObject[] substanceObjects = FindObjectsByType<SubstanceObject>(FindObjectsInactive.Include, FindObjectsSortMode.None);
        foreach (SubstanceObject substanceObject in substanceObjects)
        {
            substanceObject.RefreshVisibility();
        }
    }
}
