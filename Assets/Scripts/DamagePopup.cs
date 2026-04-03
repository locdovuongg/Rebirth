using UnityEngine;
using TMPro;

public class DamagePopup : MonoBehaviour
{
    [Header("Animation")]
    public float riseDist = 1.2f;     // bay lên bao xa
    public float life = 1.2f;         // thời gian sống
    public float scaleStart = 0.6f;   // scale ban đầu
    public float scalePeak = 1.1f;    // scale lúc peak
    public float scaleEnd = 0.5f;     // scale lúc fade hết

    TextMeshPro tmp;
    Color startColor;
    Vector3 origin;
    float timer;

    void Awake()
    {
        tmp = GetComponent<TextMeshPro>();
    }

    public void Setup(string value, Color color)
    {
        if (tmp == null)
            tmp = GetComponent<TextMeshPro>();

        tmp.text = value;
        tmp.color = color;
        tmp.fontSize = 5f;
        tmp.alignment = TextAlignmentOptions.Center;
        tmp.sortingOrder = 100;
        tmp.textWrappingMode = TextWrappingModes.NoWrap;

        // outline cho dễ đọc
        tmp.fontMaterial.EnableKeyword("OUTLINE_ON");
        tmp.outlineWidth = 0.25f;
        tmp.outlineColor = new Color(0, 0, 0, 0.8f);

        startColor = color;
        origin = transform.position;
        timer = 0f;

        // random lệch ngang nhẹ để nhiều popup không chồng nhau
        origin += new Vector3(Random.Range(-0.4f, 0.4f), Random.Range(-0.15f, 0.15f), 0);
        transform.position = origin;
        transform.localScale = Vector3.one * scaleStart;

        Destroy(gameObject, life + 0.1f);
    }

    void Update()
    {
        if (tmp == null) return;

        timer += Time.deltaTime;
        float t = Mathf.Clamp01(timer / life);

        // --- Position: bay lên smooth ---
        float ease = 1f - (1f - t) * (1f - t); // ease out quad
        float y = origin.y + riseDist * ease;
        transform.position = new Vector3(origin.x, y, origin.z);

        // --- Scale: phình ra rồi thu nhỏ ---
        float scale;
        if (t < 0.2f)
        {
            // 0→0.2: scale up nhanh
            scale = Mathf.Lerp(scaleStart, scalePeak, t / 0.2f);
        }
        else
        {
            // 0.2→1: scale giảm dần
            scale = Mathf.Lerp(scalePeak, scaleEnd, (t - 0.2f) / 0.8f);
        }
        transform.localScale = Vector3.one * scale;

        // --- Fade: giữ nguyên 60% đầu, fade 40% cuối ---
        float alpha;
        if (t < 0.6f)
            alpha = 1f;
        else
            alpha = Mathf.Lerp(1f, 0f, (t - 0.6f) / 0.4f);

        tmp.color = new Color(startColor.r, startColor.g, startColor.b, alpha);
    }

    /// <summary>
    /// Tạo popup từ code, không cần prefab
    /// </summary>
    public static DamagePopup Create(string text, Color color, Vector3 worldPos)
    {
        var go = new GameObject("Popup_" + text);
        go.transform.position = worldPos;

        var tmp = go.AddComponent<TextMeshPro>();
        tmp.rectTransform.sizeDelta = new Vector2(6f, 2f);

        var popup = go.AddComponent<DamagePopup>();
        popup.Setup(text, color);

        return popup;
    }
}