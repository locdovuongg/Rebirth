using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// Gắn vào 1 GameObject trong Canvas.
/// Tự tạo skill buttons khi Start.
/// Chỉ cần kéo SkillUser vào Inspector.
/// </summary>
public class SkillUI : MonoBehaviour
{
    [Header("References")]
    public SkillUser playerSkills;

    [Header("Layout")]
    public float buttonWidth = 100f;
    public float buttonHeight = 120f;
    public float spacing = 8f;

    [Header("Colors")]
    public Color normalColor = new Color(0.2f, 0.18f, 0.28f);
    public Color disabledColor = new Color(0.12f, 0.1f, 0.15f);
    public Color cooldownColor = new Color(0.4f, 0.12f, 0.12f);
    public Color manaOkColor = new Color(0.5f, 0.75f, 1f);
    public Color manaLowColor = new Color(1f, 0.3f, 0.3f);

    List<SkillSlot> slots = new();

    void Start()
    {
        if (playerSkills == null)
            playerSkills = FindFirstObjectByType<SkillUser>();

        if (playerSkills == null)
        {
            Debug.LogWarning("[SkillUI] No SkillUser found!");
            return;
        }

        // thêm HorizontalLayoutGroup nếu chưa có
        var layout = GetComponent<HorizontalLayoutGroup>();
        if (layout == null)
        {
            layout = gameObject.AddComponent<HorizontalLayoutGroup>();
            layout.spacing = spacing;
            layout.childAlignment = TextAnchor.MiddleCenter;
            layout.childControlWidth = false;
            layout.childControlHeight = false;
            layout.childForceExpandWidth = false;
            layout.childForceExpandHeight = false;
            layout.padding = new RectOffset(10, 10, 5, 5);
        }

        // Delay build để đợi BattleManager load skills xong
        Invoke(nameof(DelayedBuild), 0.1f);
    }

    void DelayedBuild()
    {
        // Xóa buttons cũ nếu có
        foreach (var slot in slots)
        {
            if (slot.button != null)
                Destroy(slot.button.gameObject);
        }
        slots.Clear();

        if (playerSkills != null && playerSkills.skills.Count > 0)
            BuildButtons();
        else
            Debug.LogWarning("[SkillUI] No skills to build buttons for!");
    }

    /// <summary>
    /// Gọi từ bên ngoài nếu skills thay đổi runtime
    /// </summary>
    public void RebuildSkillButtons()
    {
        DelayedBuild();
    }

    void BuildButtons()
    {
        for (int i = 0; i < playerSkills.skills.Count; i++)
        {
            Skill skill = playerSkills.skills[i];
            int index = i;

            // ===== Button Root =====
            GameObject btnObj = new GameObject($"Skill_{skill.skillName}");
            btnObj.transform.SetParent(transform, false);

            var rt = btnObj.AddComponent<RectTransform>();
            rt.sizeDelta = new Vector2(buttonWidth, buttonHeight);

            var bgImg = btnObj.AddComponent<Image>();
            bgImg.color = normalColor;

            var btn = btnObj.AddComponent<Button>();
            btn.targetGraphic = bgImg;
            btn.onClick.AddListener(() => OnSkillClick(index));

            // ===== Icon =====
            if (skill.icon != null)
            {
                GameObject iconObj = new GameObject("Icon");
                iconObj.transform.SetParent(btnObj.transform, false);

                var iconRT = iconObj.AddComponent<RectTransform>();
                iconRT.anchorMin = new Vector2(0.1f, 0.3f);
                iconRT.anchorMax = new Vector2(0.9f, 0.95f);
                iconRT.offsetMin = Vector2.zero;
                iconRT.offsetMax = Vector2.zero;

                var iconImg = iconObj.AddComponent<Image>();
                iconImg.sprite = skill.icon;
                iconImg.preserveAspect = true;
                iconImg.raycastTarget = false;
            }

            // ===== Name =====
            GameObject nameObj = new GameObject("Name");
            nameObj.transform.SetParent(btnObj.transform, false);

            var nameRT = nameObj.AddComponent<RectTransform>();
            nameRT.anchorMin = new Vector2(0, 0);
            nameRT.anchorMax = new Vector2(1, 0.28f);
            nameRT.offsetMin = new Vector2(2, 0);
            nameRT.offsetMax = new Vector2(-2, 0);

            var nameTMP = nameObj.AddComponent<TextMeshProUGUI>();
            nameTMP.text = skill.skillName;
            nameTMP.fontSize = 12;
            nameTMP.alignment = TextAlignmentOptions.Center;
            nameTMP.color = Color.white;
            nameTMP.textWrappingMode = TextWrappingModes.NoWrap;
            nameTMP.overflowMode = TextOverflowModes.Ellipsis;
            nameTMP.raycastTarget = false;

            // ===== Mana Cost (top-right) =====
            GameObject costObj = new GameObject("Cost");
            costObj.transform.SetParent(btnObj.transform, false);

            var costRT = costObj.AddComponent<RectTransform>();
            costRT.anchorMin = new Vector2(0.6f, 0.8f);
            costRT.anchorMax = new Vector2(1f, 1f);
            costRT.offsetMin = Vector2.zero;
            costRT.offsetMax = Vector2.zero;

            var costBG = costObj.AddComponent<Image>();
            costBG.color = new Color(0.08f, 0.2f, 0.5f, 0.85f);
            costBG.raycastTarget = false;

            GameObject costTextObj = new GameObject("CostText");
            costTextObj.transform.SetParent(costObj.transform, false);

            var costTextRT = costTextObj.AddComponent<RectTransform>();
            costTextRT.anchorMin = Vector2.zero;
            costTextRT.anchorMax = Vector2.one;
            costTextRT.offsetMin = Vector2.zero;
            costTextRT.offsetMax = Vector2.zero;

            var costTMP = costTextObj.AddComponent<TextMeshProUGUI>();
            costTMP.text = skill.manaCost.ToString();
            costTMP.fontSize = 11;
            costTMP.alignment = TextAlignmentOptions.Center;
            costTMP.color = manaOkColor;
            costTMP.raycastTarget = false;

            // ===== Cooldown Overlay =====
            GameObject cdObj = new GameObject("CDOverlay");
            cdObj.transform.SetParent(btnObj.transform, false);

            var cdRT = cdObj.AddComponent<RectTransform>();
            cdRT.anchorMin = Vector2.zero;
            cdRT.anchorMax = Vector2.one;
            cdRT.offsetMin = Vector2.zero;
            cdRT.offsetMax = Vector2.zero;

            var cdBG = cdObj.AddComponent<Image>();
            cdBG.color = new Color(0, 0, 0, 0.6f);
            cdBG.raycastTarget = false;

            GameObject cdTextObj = new GameObject("CDText");
            cdTextObj.transform.SetParent(cdObj.transform, false);

            var cdTextRT = cdTextObj.AddComponent<RectTransform>();
            cdTextRT.anchorMin = new Vector2(0.15f, 0.25f);
            cdTextRT.anchorMax = new Vector2(0.85f, 0.85f);
            cdTextRT.offsetMin = Vector2.zero;
            cdTextRT.offsetMax = Vector2.zero;

            var cdTMP = cdTextObj.AddComponent<TextMeshProUGUI>();
            cdTMP.text = "";
            cdTMP.fontSize = 30;
            cdTMP.alignment = TextAlignmentOptions.Center;
            cdTMP.fontStyle = FontStyles.Bold;
            cdTMP.color = Color.white;
            cdTMP.raycastTarget = false;

            cdObj.SetActive(false);

            // ===== Store =====
            slots.Add(new SkillSlot
            {
                skill = skill,
                button = btn,
                bgImage = bgImg,
                cdOverlay = cdObj,
                cdText = cdTMP,
                costText = costTMP
            });
        }
    }

    void Update()
    {
        if (BattleManager.Instance == null) return;

        bool isPlayerTurn = BattleManager.Instance.playerTurn;
        BattleEntity player = BattleManager.Instance.player;

        foreach (var slot in slots)
        {
            bool canUse = isPlayerTurn && slot.skill.CanUse(player);
            slot.button.interactable = canUse;

            // bg color
            if (slot.skill.currentCooldown > 0)
                slot.bgImage.color = cooldownColor;
            else if (!canUse)
                slot.bgImage.color = disabledColor;
            else
                slot.bgImage.color = normalColor;

            // cooldown overlay
            if (slot.skill.currentCooldown > 0)
            {
                slot.cdOverlay.SetActive(true);
                slot.cdText.text = slot.skill.currentCooldown.ToString();
            }
            else
            {
                slot.cdOverlay.SetActive(false);
            }

            // mana cost color
            slot.costText.color = player.mana >= slot.skill.manaCost
                ? manaOkColor
                : manaLowColor;
        }
    }

    void OnSkillClick(int index)
    {
        if (BattleManager.Instance == null) return;
        if (!BattleManager.Instance.playerTurn) return;
        if (index < 0 || index >= playerSkills.skills.Count) return;

        var bm = BattleManager.Instance;
        Skill skill = playerSkills.skills[index];

        if (!skill.CanUse(bm.player)) return;

        // chọn target
        BattleEntity caster = bm.player;
        BattleEntity target = skill.TargetsSelf() ? bm.player : bm.enemy;

        bool success = skill.Execute(caster, target);

        if (!success) return;

        // popup
        Transform popSelf = bm.popupPlayer;
        Transform popEnemy = bm.popupEnemy;

        switch (skill.type)
        {
            case Skill.SkillType.Damage:
                bm.SpawnPopup($"-{skill.value}", Color.red, popEnemy);
                break;
            case Skill.SkillType.Burn:
                bm.SpawnPopup($"BURN {skill.duration}t", new Color(1f, 0.4f, 0.1f), popEnemy);
                break;
            case Skill.SkillType.Freeze:
                bm.SpawnPopup($"FREEZE {skill.duration}t", new Color(0.3f, 0.7f, 1f), popEnemy);
                break;
            case Skill.SkillType.Poison:
                bm.SpawnPopup($"POISON {skill.duration}t", new Color(0.4f, 0.9f, 0.2f), popEnemy);
                break;
            case Skill.SkillType.Heal:
                bm.SpawnPopup($"+{skill.value} HP", Color.green, popSelf);
                break;
            case Skill.SkillType.Shield:
                bm.SpawnPopup($"+{skill.value} SHD", new Color(0.5f, 0.7f, 1f), popSelf);
                break;
            case Skill.SkillType.AttackBuff:
                int pct = (int)((skill.buffMultiplier - 1f) * 100);
                bm.SpawnPopup($"ATK +{pct}%", new Color(1f, 0.8f, 0.2f), popSelf);
                break;
            case Skill.SkillType.SpeedBuff:
                bm.SpawnPopup($"+{skill.value} SPD", new Color(0.9f, 0.9f, 0.3f), popSelf);
                break;
            case Skill.SkillType.DamageAndHeal:
                bm.SpawnPopup($"-{skill.value}", Color.magenta, popEnemy);
                bm.SpawnPopup($"+{skill.value / 2} HP", Color.green, popSelf);
                break;
        }

        // check enemy die
        if (bm.enemy.IsDead)
            Debug.Log("PLAYER WIN by skill!");
    }

    class SkillSlot
    {
        public Skill skill;
        public Button button;
        public Image bgImage;
        public GameObject cdOverlay;
        public TextMeshProUGUI cdText;
        public TextMeshProUGUI costText;
    }
}