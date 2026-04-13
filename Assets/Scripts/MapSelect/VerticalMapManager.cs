using UnityEngine;

public class VerticalMapManager : MonoBehaviour
{
    public ChapterData chapter;

    [Header("UI")]
    public GameObject nodePrefab;
    public RectTransform content;

    [Header("Layout")]
    public float spacingY = 220f;
    public float offsetX = 140f;

    void Start()
    {
        GenerateMap();
        FocusCurrentLevel();
    }

    void GenerateMap()
    {
        int unlocked = GameManager.Instance != null
            ? GameManager.Instance.UnlockedLevel
            : PlayerPrefs.GetInt("level_unlock", 0);

        int total = chapter.enemies.Length;

        for (int i = 0; i < total; i++)
        {
            GameObject obj = Instantiate(nodePrefab, content);

            RectTransform rt = obj.GetComponent<RectTransform>();

            // zigzag trái phải
            float x = (i % 2 == 0) ? -offsetX : offsetX;

            // Từ dưới lên: node 0 ở dưới cùng, node cuối ở trên
            float y = i * spacingY;

            rt.anchoredPosition = new Vector2(x, y);

            MapNode node = obj.GetComponent<MapNode>();

            bool isUnlocked = i <= unlocked;

            node.Setup(i, chapter, isUnlocked);
        }

        // set chiều cao content để scroll được
        float height = total * spacingY;
        content.sizeDelta = new Vector2(content.sizeDelta.x, height);

        // Đặt pivot/anchor của content ở dưới cùng
        content.pivot = new Vector2(0.5f, 0);
        content.anchorMin = new Vector2(0.5f, 0);
        content.anchorMax = new Vector2(0.5f, 0);
    }

    void FocusCurrentLevel()
    {
        int unlocked = GameManager.Instance != null
            ? GameManager.Instance.UnlockedLevel
            : PlayerPrefs.GetInt("level_unlock", 0);

        // Scroll lên tới level hiện tại
        float targetY = unlocked * spacingY;

        content.anchoredPosition = new Vector2(0, -targetY);
    }
}