using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class StatusEffectUI : MonoBehaviour
{
    public BattleEntity entity;
    public Transform iconContainer;     // HorizontalLayoutGroup

    GameObject[] iconSlots = new GameObject[4];
    TextMeshProUGUI[] iconTexts = new TextMeshProUGUI[4];

    void Start()
    {
        if (iconContainer == null)
            iconContainer = transform;

        // tạo 4 slot cho status icon
        for (int i = 0; i < 4; i++)
        {
            var slot = new GameObject($"StatusSlot_{i}");
            slot.transform.SetParent(iconContainer, false);

            var rt = slot.AddComponent<RectTransform>();
            rt.sizeDelta = new Vector2(40, 40);

            var bg = slot.AddComponent<Image>();
            bg.color = new Color(0, 0, 0, 0.5f);

            var textObj = new GameObject("Text");
            textObj.transform.SetParent(slot.transform, false);

            var textRT = textObj.AddComponent<RectTransform>();
            textRT.anchorMin = Vector2.zero;
            textRT.anchorMax = Vector2.one;
            textRT.offsetMin = Vector2.zero;
            textRT.offsetMax = Vector2.zero;

            var tmp = textObj.AddComponent<TextMeshProUGUI>();
            tmp.fontSize = 20;
            tmp.alignment = TextAlignmentOptions.Center;
            tmp.color = Color.white;

            iconSlots[i] = slot;
            iconTexts[i] = tmp;
            slot.SetActive(false);
        }
    }

    void Update()
    {
        if (entity == null) return;

        // ẩn tất cả
        for (int i = 0; i < 4; i++)
            iconSlots[i].SetActive(false);

        // hiện status hiện tại
        for (int i = 0; i < entity.statusEffects.Count && i < 4; i++)
        {
            var s = entity.statusEffects[i];

            iconSlots[i].SetActive(true);
            iconSlots[i].GetComponent<Image>().color = s.GetColor() * 0.5f;
            iconTexts[i].text = $"{s.GetIcon()}\n{s.remainingTurns}";
            iconTexts[i].color = s.GetColor();
        }
    }
}