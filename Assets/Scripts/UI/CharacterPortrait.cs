using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class CharacterPortrait : MonoBehaviour
{
    [Header("Avatar")]
    public Image avatarImage;
    public Sprite avatarSprite;

    [Header("Bars")]
    public Image hpBarFill;
    public Image manaBarFill;
    public Image ultBarFill;     // thanh speed/ult (nếu có)

    [Header("Text")]
    public TextMeshProUGUI hpText;
    public TextMeshProUGUI shieldText;
    public TextMeshProUGUI nameText;

    [Header("Shield Icon")]
    public GameObject shieldIcon;

    BattleEntity entity;

    void Start()
    {
        if (avatarImage != null && avatarSprite != null)
            avatarImage.sprite = avatarSprite;
    }

    public void SetEntity(BattleEntity e)
    {
        entity = e;
    }

    void Update()
    {
        if (entity == null) return;

        // HP bar
        if (hpBarFill != null)
            hpBarFill.fillAmount = (float)entity.currentHP / entity.maxHP;

        // Mana bar
        if (manaBarFill != null)
            manaBarFill.fillAmount = (float)entity.mana / entity.maxMana;

        // Ult bar
        if (ultBarFill != null)
            ultBarFill.fillAmount = (float)entity.speed / entity.speedMax;

        // HP text
        if (hpText != null)
            hpText.text = $"{entity.currentHP}/{entity.maxHP}";

        // Shield
        if (shieldText != null)
        {
            if (entity.shield > 0)
            {
                shieldText.text = entity.shield.ToString();
                shieldText.gameObject.SetActive(true);
                if (shieldIcon != null) shieldIcon.SetActive(true);
            }
            else
            {
                shieldText.gameObject.SetActive(false);
                if (shieldIcon != null) shieldIcon.SetActive(false);
            }
        }
    }
}