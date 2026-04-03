using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class CharacterPanel : MonoBehaviour
{
    [Header("Avatar")]
    public Image avatarImage;
    public Image avatarBorder;

    [Header("HP Bar")]
    public Image hpFill;
    public Color hpHighColor = new Color(0.2f, 0.75f, 0.2f);
    public Color hpMidColor = new Color(0.85f, 0.75f, 0.1f);
    public Color hpLowColor = new Color(0.85f, 0.15f, 0.15f);

    [Header("Mana Bar")]
    public Image manaFill;
    public Color manaColor = new Color(0.2f, 0.45f, 0.8f);

    [Header("Speed Bar (extra turn)")]
    public Image speedFill;
    public Color speedColor = new Color(0.4f, 0.8f, 1f);

    [Header("Ult Bar (ultimate)")]
    public Image ultFill;
    public Color ultColor = new Color(1f, 0.5f, 0.1f);
    public Color ultReadyColor = new Color(1f, 0.9f, 0.3f);

    [Header("Text")]
    public TextMeshProUGUI hpText;
    public TextMeshProUGUI shieldText;
    public GameObject shieldIcon;

    [Header("Damage Flash")]
    public float flashDuration = 0.15f;
    public Color flashColor = Color.red;

    BattleEntity entity;
    int lastHP;
    float flashTimer;
    Color originalBorderColor;

    void Start()
    {
        if (manaFill != null)
            manaFill.color = manaColor;
        if (avatarBorder != null)
            originalBorderColor = avatarBorder.color;
    }

    public void SetEntity(BattleEntity e)
    {
        entity = e;
        lastHP = e.currentHP;
    }

    void Update()
    {
        if (entity == null) return;

        // --- HP ---
        float hpRatio = (float)entity.currentHP / entity.maxHP;

        if (hpFill != null)
        {
            hpFill.fillAmount = hpRatio;

            if (hpRatio <= 0.25f)
                hpFill.color = hpLowColor;
            else if (hpRatio <= 0.5f)
                hpFill.color = hpMidColor;
            else
                hpFill.color = hpHighColor;
        }

        if (hpText != null)
            hpText.text = $"{entity.currentHP}/{entity.maxHP}";

        // flash khi mất máu
        if (entity.currentHP < lastHP)
            flashTimer = flashDuration;
        lastHP = entity.currentHP;

        // --- Mana ---
        if (manaFill != null)
            manaFill.fillAmount = (float)entity.mana / entity.maxMana;

        // --- Speed (extra turn) ---
        if (speedFill != null)
        {
            speedFill.fillAmount = (float)entity.speed / entity.speedMax;
            speedFill.color = entity.extraTurnReady ? new Color(1f, 1f, 0.3f) : speedColor;
        }

        // --- Ult charge ---
        if (ultFill != null)
        {
            ultFill.fillAmount = (float)entity.ultCharge / entity.ultChargeMax;
            ultFill.color = entity.ultimateReady ? ultReadyColor : ultColor;
        }

        // --- Shield ---
        if (shieldText != null)
        {
            bool has = entity.shield > 0;
            shieldText.gameObject.SetActive(has);
            if (has) shieldText.text = entity.shield.ToString();
        }
        if (shieldIcon != null)
            shieldIcon.SetActive(entity.shield > 0);

        // --- Flash ---
        if (avatarBorder != null)
        {
            if (flashTimer > 0f)
            {
                flashTimer -= Time.deltaTime;
                float t = flashTimer / flashDuration;
                avatarBorder.color = Color.Lerp(originalBorderColor, flashColor, t);
            }
            else
            {
                avatarBorder.color = originalBorderColor;
            }
        }
    }
}