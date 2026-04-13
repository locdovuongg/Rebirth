using UnityEngine;
using UnityEngine.UI;

public class MapNode : MonoBehaviour
{
    public int levelIndex;
    public ChapterData chapter;

    [Header("UI")]
    public Button button;
    public GameObject lockIcon;
    public Image icon;

    public void Setup(int index, ChapterData chap, bool unlocked)
    {
        levelIndex = index;
        chapter = chap;

        button.interactable = unlocked;
        lockIcon.SetActive(!unlocked);

        // reset event tránh bị add nhiều lần
        button.onClick.RemoveAllListeners();
        button.onClick.AddListener(OnClick);

        SetupVisual();
    }

    void SetupVisual()
    {
        EnemyData data = chapter.enemies[levelIndex];

        // Hiện avatar enemy trên node
        if (icon != null && data.avatar != null)
        {
            icon.sprite = data.avatar;
            icon.preserveAspect = true;
        }

        // boss node to hơn + viền đỏ
        if (data.isBoss)
        {
            transform.localScale = Vector3.one * 1.3f;
            icon.color = Color.red;
        }
        else
        {
            transform.localScale = Vector3.one;
            icon.color = Color.white;
        }
    }

    void OnClick()
    {
        GameManager.Instance.StartLevel(chapter, levelIndex);
    }
}