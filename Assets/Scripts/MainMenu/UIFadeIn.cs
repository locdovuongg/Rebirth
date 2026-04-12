using UnityEngine;
using UnityEngine.UI;

public class UIFadeIn : MonoBehaviour
{
    public CanvasGroup canvasGroup;
    public float duration = 1f;

    void Start()
    {
        StartCoroutine(FadeIn());
    }

    System.Collections.IEnumerator FadeIn()
    {
        float t = 0;

        while (t < duration)
        {
            t += Time.deltaTime;
            canvasGroup.alpha = t / duration;
            yield return null;
        }

        canvasGroup.alpha = 1;
    }
}