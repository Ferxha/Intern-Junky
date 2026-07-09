using System.Collections;
using UnityEngine;

public class FadeController : MonoBehaviour
{
    private CanvasGroup canvasGroup;
    public float fadeToBlackDuration = 3f;
    public float fadeToClearDuration = 1f;

    private void Awake()
    {
        canvasGroup = GetComponent<CanvasGroup>();
        if (canvasGroup == null)
        {
            Debug.LogError("FadeController necesita un CanvasGroup en el mismo GameObject!");
        }
        else
        {
            // Empezar totalmente transparente y sin bloquear interacciones
            canvasGroup.alpha = 0;
            canvasGroup.blocksRaycasts = false;
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

    public void FadeToBlackInstant()
    {
        if (canvasGroup != null)
        {
            canvasGroup.alpha = 1;
        }
    }

    public void FadeToClearInstant()
    {
        if (canvasGroup != null)
        {
            canvasGroup.alpha = 0;
        }
    }

    private IEnumerator FadeToBlackRoutine(float duration)
    {
        if (canvasGroup == null) yield break;

        canvasGroup.blocksRaycasts = true;
        
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

    private IEnumerator FadeToClearRoutine(float duration)
    {
        if (canvasGroup == null) yield break;

        float elapsedTime = 0f;
        while (elapsedTime < duration)
        {
            elapsedTime += Time.deltaTime;
            canvasGroup.alpha = Mathf.Clamp01(1 - (elapsedTime / duration));
            yield return null;
        canvasGroup.blocksRaycasts = false;
        }
        canvasGroup.alpha = 0;
    }
}
