using UnityEngine;
using UnityEngine.Events;
using System.Collections;

public enum SubstanceType
{
    None,
    Coca,
    Extasis
}

public class SubstanceManager : MonoBehaviour
{
    // Configuración inicial
    public int initialCocaAmount = 5;
    public int initialExtasisAmount = 5;
    public float effectDuration = 10f;
    public float intoxicationProbability = 0.3f;
    public float intoxicationDuration = 5f;
    public float cocaSpeedMultiplier = 1.5f;
    public float extasisSpeedMultiplier = 0.7f;
    public float abstinenceTime = 12f;
    public float abstinenceFadeDuration = 12f;
    
    // Referencias
    public VisualEffectsController visualEffects;
    public GameManager gameManager;
    
    // Eventos opcionales
    public UnityEvent<SubstanceType> OnSubstanceConsumed;
    public UnityEvent OnEffectEnded;
    public UnityEvent OnIntoxication;

    // Estado privado
    private SubstanceType currentActiveSubstance = SubstanceType.None;
    private float currentEffectTimer = 0f;
    private int cocaInventory;
    private int extasisInventory;
    private bool isIntoxicated = false;
    private float timeWithoutEffect = 0f;
    private bool abstinenceFadeStarted = false;

    void Awake()
    {
        cocaInventory = initialCocaAmount;
        extasisInventory = initialExtasisAmount;
        
        if (visualEffects == null)
            visualEffects = GetComponent<VisualEffectsController>();
        
        if (gameManager == null)
            gameManager = FindFirstObjectByType<GameManager>();
    }

    void Update()
    {
        if (gameManager != null && !gameManager.IsGameActive)
            return;

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

    private bool ConsumeSubstance(SubstanceType type)
    {
        if (GetInventoryAmount(type) <= 0)
        {
            Debug.LogWarning($"No hay {type} disponible en el inventario");
            return false;
        }

        if (isIntoxicated)
        {
            Debug.LogWarning("No puedes consumir más sustancias mientras estás intoxicado");
            return false;
        }

        if (currentActiveSubstance == SubstanceType.None)
        {
            ActivateEffect(type);
            DeductFromInventory(type);
        }
        else if (currentActiveSubstance == type)
        {
            ProlongEffect();
            DeductFromInventory(type);
            
            if (CheckIntoxicationRoll())
            {
                TriggerIntoxication();
            }
        }
        else
        {
            Debug.Log($"Mezclaste {currentActiveSubstance} con {type}!");
            TriggerIntoxication();
            DeductFromInventory(type);
        }
        
        return true;
    }

    private bool HasSubstanceInInventory(SubstanceType type)
    {
        return type switch
        {
            SubstanceType.Coca => cocaInventory > 0,
            SubstanceType.Extasis => extasisInventory > 0,
            _ => false
        };
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
            cocaInventory--;
        else if (type == SubstanceType.Extasis)
            extasisInventory--;
    }

    private void ActivateEffect(SubstanceType type)
    {
        currentActiveSubstance = type;
        currentEffectTimer = effectDuration;
        timeWithoutEffect = 0f;
        abstinenceFadeStarted = false;
        
        if (gameManager != null && gameManager.fadeController != null)
        {
            gameManager.fadeController.StopFade();
            gameManager.fadeController.FadeToClearInstant();
        }
        
        ApplyVisualEffects(type);
        OnSubstanceConsumed?.Invoke(type);
        
        Debug.Log($"Efecto de {type} activado por {effectDuration}s");
    }

    private void ProlongEffect()
    {
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
        if (isIntoxicated) return;
        
        isIntoxicated = true;
        Debug.Log("¡INTOXICACIÓN!");
        
        if (visualEffects != null)
            visualEffects.ApplyIntoxicationEffect();
        
        OnIntoxication?.Invoke();
        StartCoroutine(HandleIntoxication());
    }

    private IEnumerator HandleIntoxication()
    {
        yield return new WaitForSeconds(intoxicationDuration);
        
        if (gameManager != null)
            gameManager.GameOverIntoxication();
        else
            Debug.LogError("GameManager no asignado!");
    }

    private void EndEffect()
    {
        Debug.Log($"Efecto de {currentActiveSubstance} terminado");
        
        currentActiveSubstance = SubstanceType.None;
        currentEffectTimer = 0f;
        timeWithoutEffect = 0f;
        
        if (visualEffects != null)
            visualEffects.ResetEffects();
        
        OnEffectEnded?.Invoke();
    }

    private void StartAbstinenceFade()
    {
        abstinenceFadeStarted = true;
        
        if (gameManager != null && gameManager.fadeController != null)
        {
            Debug.Log("Iniciando fade de abstinencia...");
            gameManager.fadeController.FadeToBlack(abstinenceFadeDuration);
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
        if (visualEffects == null) return;

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
}
