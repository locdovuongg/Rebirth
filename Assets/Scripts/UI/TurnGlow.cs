using UnityEngine;
using UnityEngine.UI;

public class TurnGlow : MonoBehaviour
{
    public Image glowImage;
    public Color glowColorA = new Color(1f, 0.9f, 0.3f, 0.8f);
    public Color glowColorB = new Color(1f, 0.9f, 0.3f, 0.2f);
    public float speed = 2f;

    void Update()
    {
        if (glowImage == null) return;

        float t = (Mathf.Sin(Time.time * speed * Mathf.PI) + 1f) / 2f;
        glowImage.color = Color.Lerp(glowColorB, glowColorA, t);
    }

    void OnDisable()
    {
        if (glowImage != null)
            glowImage.color = new Color(1, 1, 1, 0);
    }
}