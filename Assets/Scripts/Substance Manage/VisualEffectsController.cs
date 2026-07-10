using UnityEngine;
using System.Collections;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

public class VisualEffectsController : MonoBehaviour
{
    // Referencias
    public Volume globalVolume;
    
    // Componentes de efectos
    private Bloom bloom;
    private ColorAdjustments colorAdjustments;
    private ChromaticAberration chromaticAberration;
    private WhiteBalance whiteBalance;
    private bool effectsInitialized = false;
    
    // Configuración de efectos Coca
    public float cocaBloomIntensity = 3f;
    public float cocaWhiteBalanceTemperature = 35f;
    public float cocaSaturation = 15f;
    
    // Configuración de efectos Éxtasis
    public float extasisSaturation = 40f;
    public float extasisChromaticAberration = 0.25f;
    public float extasisHueShift = 10f;
    
    // Configuración de intoxicación
    public float intoxicationChromaticAberration = 0.8f;
    public float intoxicationSaturation = -30f;
    
    void Awake()
    {
        if (globalVolume == null)
        {
            globalVolume = FindFirstObjectByType<Volume>();
            
            if (globalVolume == null)
            {
                Debug.LogError("No se encontró un Volume en la escena. Añade uno con un URP Volume Profile.");
                return;
            }
        }
        
        InitializeEffects();
    }

    void Start()
    {
        ResetEffects();
    }
    public void ApplyCocaEffect()
    {
        if (!effectsInitialized) return;
        
        Debug.Log("Aplicando efecto de Coca");
        ResetEffects();
        
        if (bloom != null)
            bloom.intensity.value = cocaBloomIntensity;
        
        if (whiteBalance != null)
            whiteBalance.temperature.value = cocaWhiteBalanceTemperature;
        
        if (colorAdjustments != null)
            colorAdjustments.saturation.value = cocaSaturation;
    }

    public void ApplyExtasisEffect()
    {
        if (!effectsInitialized) return;
        
        Debug.Log("Aplicando efecto de Éxtasis");
        ResetEffects();
        
        if (colorAdjustments != null)
        {
            colorAdjustments.saturation.value = extasisSaturation;
            colorAdjustments.hueShift.value = extasisHueShift;
        }
        
        if (chromaticAberration != null)
            chromaticAberration.intensity.value = extasisChromaticAberration;
    }

    public void ApplyIntoxicationEffect()
    {
        if (!effectsInitialized) return;
        
        Debug.Log("¡Aplicando efecto de INTOXICACIÓN!");
        
        // Aplicar solo distorsión cromática y desaturación
        if (chromaticAberration != null)
            chromaticAberration.intensity.value = intoxicationChromaticAberration;
        
        if (colorAdjustments != null)
            colorAdjustments.saturation.value = intoxicationSaturation;
            
    }

    public void ResetEffects()
    {
        if (!effectsInitialized) return;
        
        if (bloom != null)
            bloom.intensity.value = 0f;
        
        if (whiteBalance != null)
            whiteBalance.temperature.value = 0f;
        
        if (colorAdjustments != null)
        {
            colorAdjustments.saturation.value = 0f;
            colorAdjustments.hueShift.value = 0f;
        }        
        
        if (chromaticAberration != null)
            chromaticAberration.intensity.value = 0f;
    }
    private void InitializeEffects()
    {
        if (globalVolume == null || globalVolume.profile == null)
        {
            Debug.LogError("Global Volume o Profile no asignado!");
            return;
        }

        effectsInitialized = true;
        
        if (!globalVolume.profile.TryGet(out bloom))
        {
            Debug.LogWarning("Bloom no encontrado en el Volume Profile.");
            effectsInitialized = false;
        }

        if (!globalVolume.profile.TryGet(out colorAdjustments))
        {
            Debug.LogWarning("Color Adjustments no encontrado en el Volume Profile.");
            effectsInitialized = false;
        }

        if (!globalVolume.profile.TryGet(out chromaticAberration))
        {
            Debug.LogWarning("Chromatic Aberration no encontrado en el Volume Profile.");
        }

        if (!globalVolume.profile.TryGet(out whiteBalance))
        {
            Debug.LogWarning("White Balance no encontrado en el Volume Profile.");
        }
        
        if (effectsInitialized)
            Debug.Log("✓ Efectos visuales inicializados correctamente");
    }
}
