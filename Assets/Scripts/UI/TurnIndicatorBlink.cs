using UnityEngine;
using UnityEngine.UI;

public class TurnIndicatorBlink : MonoBehaviour
{
    public Image borderImage;
    public float blinkSpeed = 2f;
    public Color blinkColor = Color.yellow;
    Color originalColor;

    void Start()
    {
        if (borderImage != null)
            originalColor = borderImage.color;
    }

    void Update()
    {
        if (borderImage == null) return;

        float t = (Mathf.Sin(Time.time * blinkSpeed * Mathf.PI) + 1f) / 2f;
        borderImage.color = Color.Lerp(originalColor, blinkColor, t);
    }

    void OnDisable()
    {
        if (borderImage != null)
            borderImage.color = originalColor;
    }
}