using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class MapNode : MonoBehaviour
{
    public int levelIndex;
    public ChapterData chapter;

    [Header("UI")]
    public Button button;
    public GameObject lockIcon;
    public Image icon;
    public TextMeshProUGUI levelText;

    /// <summary>
    /// state: 0 = locked (ẩn), 1 = unlocked (hiện, click được)
    /// </summary>
    public void Setup(int index, ChapterData chap, int state)
    {
        levelIndex = index;
        chapter = chap;

        // Locked → ẩn hoàn toàn
        if (state == 0)
        {
            gameObject.SetActive(false);
            return;
        }

        gameObject.SetActive(true);

        // Số thứ tự
        if (levelText != null)
            levelText.text = (index + 1).ToString();

        SetupVisual();

        // Unlocked — hiện, click được
        button.interactable = true;
        lockIcon.SetActive(false);
        button.onClick.RemoveAllListeners();
        button.onClick.AddListener(OnClick);
    }

    void SetupVisual()
    {
        EnemyData data = chapter.enemies[levelIndex];

        // Hiện avatar enemy trên node — giữ tỉ lệ gốc, căn giữa
        if (icon != null && data.avatar != null)
        {
            icon.enabled = true;
            icon.sprite = data.avatar;
            icon.preserveAspect = true;
            icon.SetNativeSize();

            // Căn giữa node
            RectTransform iconRT = icon.GetComponent<RectTransform>();
            iconRT.anchorMin = new Vector2(0.5f, 0.5f);
            iconRT.anchorMax = new Vector2(0.5f, 0.5f);
            iconRT.pivot = new Vector2(0.5f, 0.5f);
            iconRT.anchoredPosition = Vector2.zero;
        }

        // boss node to hơn + viền đỏ
        if (data.isBoss)
        {
            transform.localScale = Vector3.one * 1.3f;
            if (icon != null) icon.color = Color.red;
        }
        else
        {
            transform.localScale = Vector3.one;
            if (icon != null) icon.color = Color.white;
        }
    }

    void OnClick()
    {
        GameManager.Instance.StartLevel(chapter, levelIndex);
    }
}