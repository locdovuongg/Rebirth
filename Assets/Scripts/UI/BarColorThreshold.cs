using UnityEngine;
using UnityEngine.UI;

public class BarColorThreshold : MonoBehaviour
{
    public Image barFill;

    public Color highColor = new Color(0.2f, 0.8f, 0.2f);   // xanh lá
    public Color midColor = new Color(0.9f, 0.8f, 0.1f);     // vàng
    public Color lowColor = new Color(0.9f, 0.1f, 0.1f);     // đỏ

    [Range(0f, 1f)] public float midThreshold = 0.5f;
    [Range(0f, 1f)] public float lowThreshold = 0.25f;

    void Update()
    {
        if (barFill == null) return;

        float fill = barFill.fillAmount;

        if (fill <= lowThreshold)
            barFill.color = lowColor;
        else if (fill <= midThreshold)
            barFill.color = midColor;
        else
            barFill.color = highColor;
    }
}