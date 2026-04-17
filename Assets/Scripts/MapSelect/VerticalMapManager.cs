using UnityEngine;
using UnityEngine.UI;

public class VerticalMapManager : MonoBehaviour
{
    public ChapterData chapter;

    [Header("UI")]
    public GameObject nodePrefab;
    public RectTransform content;
    public RectTransform viewport;          // kéo Viewport vào đây

    [Header("Layout")]
    public float offsetX = 140f;
    public float topPadding = 100f;
    public float bottomPadding = 100f;

    void Start()
    {
        Canvas.ForceUpdateCanvases();
        GenerateMap();
    }

    void GenerateMap()
    {
        int unlocked = GameManager.Instance != null
            ? GameManager.Instance.UnlockedLevel
            : PlayerPrefs.GetInt("level_unlock", 0);

        int total = chapter.enemies.Length;

        // Lấy chiều cao Content đã set sẵn trong Editor — KHÔNG thay đổi sizeDelta
        float contentHeight = content.rect.height;
        if (contentHeight <= 0f)
            contentHeight = viewport != null ? viewport.rect.height : 1920f;

        // Khoảng cách giữa các node, nằm gọn trong content
        float usableHeight = contentHeight - topPadding - bottomPadding;
        float spacingY = total > 1 ? usableHeight / (total - 1) : 0f;

        for (int i = 0; i < total; i++)
        {
            GameObject obj = Instantiate(nodePrefab, content);

            RectTransform rt = obj.GetComponent<RectTransform>();

            // Anchor ở top-center của content
            rt.anchorMin = new Vector2(0.5f, 1f);
            rt.anchorMax = new Vector2(0.5f, 1f);
            rt.pivot = new Vector2(0.5f, 0.5f);

            // zigzag trái phải
            float x = (i % 2 == 0) ? -offsetX : offsetX;

            // Node 0 ở đáy, node cuối ở đỉnh
            // Từ top anchor: y âm = đi xuống
            float y = -(topPadding + (total - 1 - i) * spacingY);

            rt.anchoredPosition = new Vector2(x, y);

            MapNode node = obj.GetComponent<MapNode>();

            // 0 = ẩn, 1 = hiện + click được
            int state = (i <= unlocked) ? 1 : 0;

            node.Setup(i, chapter, state);
        }

        // Scroll về đáy (thấy node đầu tiên)
        content.anchoredPosition = Vector2.zero;
    }
}