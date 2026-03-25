using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class BattleUI : MonoBehaviour
{
    public BattleManager battle;

    [Header("Player")]
    public Image playerHP, playerMana, playerUlt;
    public TextMeshProUGUI playerShield;
    public TextMeshProUGUI playerHPText;
    public Button ultButton;

    [Header("Enemy")]
    public Image enemyHP, enemyMana, enemyUlt;
    public TextMeshProUGUI enemyShield;
    public TextMeshProUGUI enemyHPText;

    [Header("Turn")]
    public TextMeshProUGUI turnText;

    void Start()
    {
        if (battle == null)
            battle = BattleManager.Instance;

        if (ultButton != null)
            ultButton.onClick.AddListener(OnUltClick);
    }

    void Update()
    {
        if (battle == null) return;

        // Player bars
        if (playerHP != null)
            playerHP.fillAmount = (float)battle.player.currentHP / battle.player.maxHP;
        if (playerMana != null)
            playerMana.fillAmount = (float)battle.player.mana / battle.player.maxMana;
        if (playerUlt != null)
            playerUlt.fillAmount = (float)battle.player.speed / battle.player.speedMax;
        if (playerShield != null)
            playerShield.text = battle.player.shield > 0 ? battle.player.shield.ToString() : "";
        if (playerHPText != null)
            playerHPText.text = $"{battle.player.currentHP}/{battle.player.maxHP}";

        // Enemy bars
        if (enemyHP != null)
            enemyHP.fillAmount = (float)battle.enemy.currentHP / battle.enemy.maxHP;
        if (enemyMana != null)
            enemyMana.fillAmount = (float)battle.enemy.mana / battle.enemy.maxMana;
        if (enemyUlt != null)
            enemyUlt.fillAmount = (float)battle.enemy.speed / battle.enemy.speedMax;
        if (enemyShield != null)
            enemyShield.text = battle.enemy.shield > 0 ? battle.enemy.shield.ToString() : "";
        if (enemyHPText != null)
            enemyHPText.text = $"{battle.enemy.currentHP}/{battle.enemy.maxHP}";

        // Ultimate button
        if (ultButton != null)
            ultButton.interactable = battle.player.ultimateReady && battle.playerTurn;

        // Turn indicator
        if (turnText != null)
            turnText.text = battle.playerTurn ? "YOUR TURN" : "ENEMY TURN";
    }

    void OnUltClick()
    {
        if (battle != null)
            battle.UseUltimate();
    }
}