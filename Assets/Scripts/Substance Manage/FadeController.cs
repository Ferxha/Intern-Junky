using System.Collections;
using System;
using UnityEngine;
using UnityEngine.UI;

public class FadeController : MonoBehaviour
{
    private CanvasGroup canvasGroup;
    private Image image;
    public float fadeToBlackDuration = 3f;
    public float fadeToClearDuration = 1f;

    private void Awake()
    {
        canvasGroup = GetComponent<CanvasGroup>();
        image = GetComponent<Image>();

        if (image != null)
        {
            image.raycastTarget = false;
        }

        if (canvasGroup == null)
        {
            Debug.LogError("FadeController necesita un CanvasGroup en el mismo GameObject!");
        }
        else
        {
            // Empezar totalmente transparente y sin bloquear interacciones
            canvasGroup.alpha = 0;
            canvasGroup.blocksRaycasts = false;
            canvasGroup.interactable = false;
            Debug.Log($"✓ FadeController inicializado. CanvasGroup encontrado. Alpha inicial: {canvasGroup.alpha}");
        }
    }

    public void FadeToBlack(float duration = -1f)
    {
        if (canvasGroup == null)
        {
            Debug.LogError("No se puede hacer fade: CanvasGroup no encontrado!");
            return;
        }

        if (duration < 0) duration = fadeToBlackDuration;
        
        Debug.Log($"Iniciando fade to black (duración: {duration}s)");
        StopAllCoroutines();
        StartCoroutine(FadeToBlackRoutine(duration));
    }

    public void StopFade()
    {
        StopAllCoroutines();
    }

    public void FadeToClear(float duration = -1f)
    {
        if (duration < 0) duration = fadeToClearDuration;
        StopAllCoroutines();
        StartCoroutine(FadeToClearRoutine(duration));
    }

    public void FadeToBlackAndClear(float fadeToBlackTime = 1f, float fadeToClearTime = -1f, Action onComplete = null)
    {
        if (canvasGroup == null)
        {
            Debug.LogError("No se puede hacer fade: CanvasGroup no encontrado!");
            onComplete?.Invoke();
            return;
        }

        if (fadeToClearTime < 0) fadeToClearTime = fadeToClearDuration;

        StopAllCoroutines();
        StartCoroutine(FadeToBlackAndClearRoutine(fadeToBlackTime, fadeToClearTime, onComplete));
    }

    public void FadeToBlackInstant()
    {
        if (canvasGroup != null)
        {
            canvasGroup.alpha = 1;
            canvasGroup.blocksRaycasts = false;
            canvasGroup.interactable = false;
        }
    }

    public void FadeToClearInstant()
    {
        if (canvasGroup != null)
        {
            canvasGroup.alpha = 0;
            canvasGroup.blocksRaycasts = false;
            canvasGroup.interactable = false;
        }
    }

    private IEnumerator FadeToBlackRoutine(float duration)
    {
        if (canvasGroup == null) yield break;

        canvasGroup.blocksRaycasts = false;
        canvasGroup.interactable = false;
        
        float elapsedTime = 0f;
        float startAlpha = canvasGroup.alpha;
        
        while (elapsedTime < duration)
        {
            elapsedTime += Time.deltaTime;
            canvasGroup.alpha = Mathf.Clamp01(elapsedTime / duration);
            yield return null;
        }
        canvasGroup.alpha = 1;
        Debug.Log($"Fade to black completado. Alpha final: {canvasGroup.alpha}");
    }

    private IEnumerator FadeToBlackAndClearRoutine(float fadeToBlackTime, float fadeToClearTime, Action onComplete)
    {
        yield return FadeToBlackRoutine(fadeToBlackTime);
        yield return FadeToClearRoutine(fadeToClearTime);
        onComplete?.Invoke();
    }

    private IEnumerator FadeToClearRoutine(float duration)
    {
        if (canvasGroup == null) yield break;

        float elapsedTime = 0f;
        float startAlpha = canvasGroup.alpha;
        while (elapsedTime < duration)
        {
            elapsedTime += Time.deltaTime;
            canvasGroup.alpha = Mathf.Lerp(startAlpha, 0f, elapsedTime / duration);
            yield return null;
        }
        canvasGroup.alpha = 0;
        canvasGroup.blocksRaycasts = false;
        canvasGroup.interactable = false;
    }
}
