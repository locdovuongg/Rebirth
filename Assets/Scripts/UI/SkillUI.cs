using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class SkillUI : MonoBehaviour
{
    [Header("References")]
    public SkillUser playerSkills;
    public Transform buttonContainer;

    [Header("Button Style")]
    public Sprite buttonBGSprite;
    public Color normalColor = new Color(0.25f, 0.2f, 0.35f);
    public Color disabledColor = new Color(0.15f, 0.12f, 0.18f);
    public Color cooldownColor = new Color(0.5f, 0.15f, 0.15f);
    public float buttonSize = 120f;

    List<SkillSlot> slots = new();

    void Start()
    {
        if (playerSkills == null)
            playerSkills = FindFirstObjectByType<SkillUser>();

        if (buttonContainer == null)
            buttonContainer = transform;

        BuildButtons();
    }

    void BuildButtons()
    {
        if (playerSkills == null) return;

        for (int i = 0; i < playerSkills.skills.Count; i++)
        {
            Skill skill = playerSkills.skills[i];
            int index = i;

            // --- Button Root ---
            GameObject btnObj = new GameObject($"Skill_{skill.skillName}");
            btnObj.transform.SetParent(buttonContainer, false);

            var rt = btnObj.AddComponent<RectTransform>();
            rt.sizeDelta = new Vector2(buttonSize, buttonSize + 30);

            // --- BG ---
            var bgImg = btnObj.AddComponent<Image>();
            if (buttonBGSprite != null)
            {
                bgImg.sprite = buttonBGSprite;
                bgImg.type = Image.Type.Sliced;
            }
            bgImg.color = normalColor;

            var btn = btnObj.AddComponent<Button>();
            btn.targetGraphic = bgImg;
            btn.onClick.AddListener(() => OnSkillClick(index));

            // --- Icon ---
            GameObject iconObj = new GameObject("Icon");
            iconObj.transform.SetParent(btnObj.transform, false);

            var iconRT = iconObj.AddComponent<RectTransform>();
            iconRT.anchorMin = new Vector2(0.1f, 0.25f);
            iconRT.anchorMax = new Vector2(0.9f, 0.95f);
            iconRT.offsetMin = Vector2.zero;
            iconRT.offsetMax = Vector2.zero;

            var iconImg = iconObj.AddComponent<Image>();
            if (skill.icon != null)
                iconImg.sprite = skill.icon;
            else
                iconImg.color = GetSkillTypeColor(skill.type);

            // --- Name ---
            GameObject nameObj = new GameObject("Name");
            nameObj.transform.SetParent(btnObj.transform, false);

            var nameRT = nameObj.AddComponent<RectTransform>();
            nameRT.anchorMin = new Vector2(0, 0);
            nameRT.anchorMax = new Vector2(1, 0.22f);
            nameRT.offsetMin = Vector2.zero;
            nameRT.offsetMax = Vector2.zero;

            var nameTMP = nameObj.AddComponent<TextMeshProUGUI>();
            nameTMP.text = skill.skillName;
            nameTMP.fontSize = 14;
            nameTMP.alignment = TextAlignmentOptions.Center;
            nameTMP.color = Color.white;
            nameTMP.enableWordWrapping = false;
            nameTMP.overflowMode = TextOverflowModes.Ellipsis;

            // --- Mana Cost (top-right) ---
            GameObject costObj = new GameObject("ManaCost");
            costObj.transform.SetParent(btnObj.transform, false);

            var costRT = costObj.AddComponent<RectTransform>();
            costRT.anchorMin = new Vector2(0.6f, 0.78f);
            costRT.anchorMax = new Vector2(1f, 1f);
            costRT.offsetMin = Vector2.zero;
            costRT.offsetMax = Vector2.zero;

            var costBG = costObj.AddComponent<Image>();
            costBG.color = new Color(0.1f, 0.3f, 0.7f, 0.85f);

            var costTextObj = new GameObject("CostText");
            costTextObj.transform.SetParent(costObj.transform, false);

            var costTextRT = costTextObj.AddComponent<RectTransform>();
            costTextRT.anchorMin = Vector2.zero;
            costTextRT.anchorMax = Vector2.one;
            costTextRT.offsetMin = Vector2.zero;
            costTextRT.offsetMax = Vector2.zero;

            var costTMP = costTextObj.AddComponent<TextMeshProUGUI>();
            costTMP.text = skill.manaCost.ToString();
            costTMP.fontSize = 13;
            costTMP.alignment = TextAlignmentOptions.Center;
            costTMP.color = new Color(0.6f, 0.85f, 1f);

            // --- Duration badge (top-left, cho buff/debuff) ---
            GameObject durObj = new GameObject("Duration");
            durObj.transform.SetParent(btnObj.transform, false);

            var durRT = durObj.AddComponent<RectTransform>();
            durRT.anchorMin = new Vector2(0, 0.78f);
            durRT.anchorMax = new Vector2(0.35f, 1f);
            durRT.offsetMin = Vector2.zero;
            durRT.offsetMax = Vector2.zero;

            var durBG = durObj.AddComponent<Image>();
            durBG.color = new Color(0.6f, 0.4f, 0.1f, 0.85f);

            var durTextObj = new GameObject("DurText");
            durTextObj.transform.SetParent(durObj.transform, false);

            var durTextRT = durTextObj.AddComponent<RectTransform>();
            durTextRT.anchorMin = Vector2.zero;
            durTextRT.anchorMax = Vector2.one;
            durTextRT.offsetMin = Vector2.zero;
            durTextRT.offsetMax = Vector2.zero;

            var durTMP = durTextObj.AddComponent<TextMeshProUGUI>();
            durTMP.text = $"{skill.duration}t";
            durTMP.fontSize = 11;
            durTMP.alignment = TextAlignmentOptions.Center;
            durTMP.color = Color.white;

            bool hasDuration = skill.type == Skill.SkillType.Burn
                || skill.type == Skill.SkillType.Freeze
                || skill.type == Skill.SkillType.Poison
                || skill.type == Skill.SkillType.AttackBuff;
            durObj.SetActive(hasDuration);

            // --- Cooldown Overlay ---
            GameObject cdObj = new GameObject("CooldownOverlay");
            cdObj.transform.SetParent(btnObj.transform, false);

            var cdRT = cdObj.AddComponent<RectTransform>();
            cdRT.anchorMin = Vector2.zero;
            cdRT.anchorMax = Vector2.one;
            cdRT.offsetMin = Vector2.zero;
            cdRT.offsetMax = Vector2.zero;

            var cdBG = cdObj.AddComponent<Image>();
            cdBG.color = new Color(0, 0, 0, 0.65f);

            var cdTextObj = new GameObject("CDText");
            cdTextObj.transform.SetParent(cdObj.transform, false);

            var cdTextRT = cdTextObj.AddComponent<RectTransform>();
            cdTextRT.anchorMin = new Vector2(0.2f, 0.3f);
            cdTextRT.anchorMax = new Vector2(0.8f, 0.8f);
            cdTextRT.offsetMin = Vector2.zero;
            cdTextRT.offsetMax = Vector2.zero;

            var cdTMP = cdTextObj.AddComponent<TextMeshProUGUI>();
            cdTMP.text = "2";
            cdTMP.fontSize = 36;
            cdTMP.alignment = TextAlignmentOptions.Center;
            cdTMP.color = Color.white;
            cdTMP.fontStyle = FontStyles.Bold;

            cdObj.SetActive(false);

            // --- Store ---
            slots.Add(new SkillSlot
            {
                skill = skill,
                button = btn,
                bgImage = bgImg,
                iconImage = iconImg,
                cooldownOverlay = cdObj,
                cooldownText = cdTMP,
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

            // icon dim
            slot.iconImage.color = canUse ? Color.white : new Color(0.4f, 0.4f, 0.4f);

            // cooldown overlay
            if (slot.skill.currentCooldown > 0)
            {
                slot.cooldownOverlay.SetActive(true);
                slot.cooldownText.text = slot.skill.currentCooldown.ToString();
            }
            else
            {
                slot.cooldownOverlay.SetActive(false);
            }

            // mana cost color
            if (slot.costText != null)
            {
                slot.costText.color = player.mana >= slot.skill.manaCost
                    ? new Color(0.6f, 0.85f, 1f)
                    : new Color(1f, 0.3f, 0.3f);
            }
        }
    }

    void OnSkillClick(int index)
    {
        if (BattleManager.Instance == null) return;
        if (!BattleManager.Instance.playerTurn) return;

        var bm = BattleManager.Instance;
        Skill skill = playerSkills.skills[index];

        if (!skill.CanUse(bm.player)) return;

        // chọn target
        BattleEntity target = skill.TargetsSelf() ? bm.player : bm.enemy;

        bool success = skill.Execute(bm.player, target);

        if (success)
        {
            ShowSkillPopup(skill, bm);
        }
    }

    void ShowSkillPopup(Skill skill, BattleManager bm)
    {
        switch (skill.type)
        {
            case Skill.SkillType.Damage:
                bm.SpawnPopup($"-{skill.value}", Color.red, bm.popupEnemy);
                break;

            case Skill.SkillType.Burn:
                bm.SpawnPopup($"🔥 Đốt cháy {skill.duration}t", new Color(1f, 0.4f, 0.1f), bm.popupEnemy);
                break;

            case Skill.SkillType.Freeze:
                bm.SpawnPopup($"❄️ Đóng băng {skill.duration}t", new Color(0.3f, 0.7f, 1f), bm.popupEnemy);
                break;

            case Skill.SkillType.Poison:
                bm.SpawnPopup($"☠️ Nhiễm độc {skill.duration}t", new Color(0.4f, 0.9f, 0.2f), bm.popupEnemy);
                break;

            case Skill.SkillType.Heal:
                bm.SpawnPopup($"+{skill.value} HP", Color.green, bm.popupPlayer);
                break;

            case Skill.SkillType.Shield:
                bm.SpawnPopup($"+{skill.value} 🛡", new Color(0.5f, 0.7f, 1f), bm.popupPlayer);
                break;

            case Skill.SkillType.AttackBuff:
                bm.SpawnPopup($"⚔️ +{(int)((skill.buffMultiplier - 1f) * 100)}% DMG", new Color(1f, 0.8f, 0.2f), bm.popupPlayer);
                break;

            case Skill.SkillType.SpeedBuff:
                bm.SpawnPopup($"⚡ +{skill.value} Speed", new Color(0.9f, 0.9f, 0.3f), bm.popupPlayer);
                break;

            case Skill.SkillType.DamageAndHeal:
                bm.SpawnPopup($"-{skill.value}", Color.magenta, bm.popupEnemy);
                bm.SpawnPopup($"+{skill.value / 2} HP", Color.green, bm.popupPlayer);
                break;
        }
    }

    Color GetSkillTypeColor(Skill.SkillType type)
    {
        return type switch
        {
            Skill.SkillType.Damage => new Color(0.85f, 0.2f, 0.15f),
            Skill.SkillType.Burn => new Color(1f, 0.5f, 0.1f),
            Skill.SkillType.Freeze => new Color(0.3f, 0.65f, 1f),
            Skill.SkillType.Poison => new Color(0.35f, 0.8f, 0.2f),
            Skill.SkillType.Heal => new Color(0.2f, 0.8f, 0.3f),
            Skill.SkillType.Shield => new Color(0.4f, 0.6f, 0.9f),
            Skill.SkillType.AttackBuff => new Color(0.9f, 0.7f, 0.15f),
            Skill.SkillType.SpeedBuff => new Color(0.85f, 0.85f, 0.2f),
            Skill.SkillType.DamageAndHeal => new Color(0.8f, 0.2f, 0.6f),
            _ => Color.gray
        };
    }

    class SkillSlot
    {
        public Skill skill;
        public Button button;
        public Image bgImage;
        public Image iconImage;
        public GameObject cooldownOverlay;
        public TextMeshProUGUI cooldownText;
        public TextMeshProUGUI costText;
    }
}