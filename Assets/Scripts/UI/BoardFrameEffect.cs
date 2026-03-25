using UnityEngine;
using UnityEngine.UI;

public class BoardFrameEffect : MonoBehaviour
{
    public Image frameImage;

    [Header("Combo Glow")]
    public Color normalColor = new Color(0.3f, 0.25f, 0.2f);
    public Color comboColor = new Color(0.9f, 0.7f, 0.1f);
    public float glowSpeed = 3f;

    float glowTimer;
    bool isGlowing;

    void Start()
    {
        if (frameImage != null)
            frameImage.color = normalColor;
    }

    /// <summary>
    /// Gọi khi combo xảy ra
    /// </summary>
    public void TriggerComboGlow(float duration = 1f)
    {
        glowTimer = duration;
        isGlowing = true;
    }

    void Update()
    {
        if (frameImage == null) return;

        if (isGlowing)
        {
            glowTimer -= Time.deltaTime;
            float t = Mathf.PingPong(Time.time * glowSpeed, 1f);
            frameImage.color = Color.Lerp(normalColor, comboColor, t);

            if (glowTimer <= 0f)
            {
                isGlowing = false;
                frameImage.color = normalColor;
            }
        }
    }
}