using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// Kéo tất cả UI references vào Inspector.
/// Script chỉ cập nhật data — KHÔNG tạo UI.
/// Bạn tự layout UI trong Editor.
/// </summary>
public class BattleUI : MonoBehaviour
{
    [Header("=== PLAYER ===")]
    public Image playerHPBar;
    public Image playerManaBar;
    public Image playerSpeedBar;
    public Image playerUltBar;
    public Image playerAvatar;
    public TextMeshProUGUI playerHPText;
    public TextMeshProUGUI playerManaText;
    public TextMeshProUGUI playerShieldText;

    [Header("=== ENEMY ===")]
    public Image enemyHPBar;
    public Image enemyManaBar;
    public Image enemySpeedBar;
    public Image enemyUltBar;
    public Image enemyAvatar;
    public TextMeshProUGUI enemyHPText;
    public TextMeshProUGUI enemyManaText;
    public TextMeshProUGUI enemyShieldText;

    [Header("=== TURN ===")]
    public TextMeshProUGUI turnText;
    public GameObject playerTurnGlow;
    public GameObject enemyTurnGlow;

    [Header("=== ULT BUTTON ===")]
    public Button ultButton;
    public TextMeshProUGUI ultButtonText;

    [Header("=== RESULT ===")]
    public GameObject resultPanel;
    public TextMeshProUGUI resultText;
    public Button resultButton;

    [Header("=== COMBO ===")]
    public TextMeshProUGUI comboText;
    float comboTimer;

    [Header("=== BAR COLORS ===")]
    public Color hpHigh = new Color(0.2f, 0.75f, 0.2f);
    public Color hpMid = new Color(0.85f, 0.75f, 0.1f);
    public Color hpLow = new Color(0.85f, 0.15f, 0.15f);

    bool gameEnded;

    // =====================================================
    //                  SINGLETON
    // =====================================================
    public static BattleUI Instance { get; private set; }

    void Awake()
    {
        Instance = this;
    }

    void Start()
    {
        if (resultPanel != null)
            resultPanel.SetActive(false);

        if (comboText != null)
            comboText.gameObject.SetActive(false);

        if (ultButton != null)
            ultButton.onClick.AddListener(OnUltClick);

        if (resultButton != null)
            resultButton.onClick.AddListener(OnResultClick);
    }

    void Update()
    {
        if (BattleManager.Instance == null) return;

        var bm = BattleManager.Instance;
        bool isPlayerTurn = bm.playerTurn;

        // ========== UPDATE BARS ==========
        UpdateEntity(bm.player,
            playerHPBar, playerManaBar, playerSpeedBar, playerUltBar,
            playerHPText, playerManaText, playerShieldText);

        UpdateEntity(bm.enemy,
            enemyHPBar, enemyManaBar, enemySpeedBar, enemyUltBar,
            enemyHPText, enemyManaText, enemyShieldText);

        // ========== TURN ==========
        if (turnText != null)
        {
            turnText.text = isPlayerTurn ? "YOUR TURN" : "ENEMY TURN";
            turnText.color = isPlayerTurn
                ? new Color(0.3f, 1f, 0.5f)
                : new Color(1f, 0.35f, 0.3f);
        }

        if (playerTurnGlow != null)
            playerTurnGlow.SetActive(isPlayerTurn);
        if (enemyTurnGlow != null)
            enemyTurnGlow.SetActive(!isPlayerTurn);

        // ========== ULT BUTTON ==========
        if (ultButton != null)
        {
            bool canUlt = isPlayerTurn && bm.player.ultimateReady;
            ultButton.interactable = canUlt;

            if (ultButtonText != null)
                ultButtonText.color = canUlt
                    ? new Color(1f, 0.9f, 0.2f)
                    : new Color(0.4f, 0.4f, 0.4f);
        }

        // ========== COMBO TIMER ==========
        if (comboTimer > 0f)
        {
            comboTimer -= Time.deltaTime;
            if (comboTimer <= 0f && comboText != null)
                comboText.gameObject.SetActive(false);
        }

        // ========== RESULT ==========
        if (!gameEnded)
        {
            if (bm.enemy.IsDead)
                ShowResult("VICTORY!", new Color(1f, 0.85f, 0.1f), "CONTINUE");
            else if (bm.player.IsDead)
                ShowResult("DEFEAT!", new Color(0.9f, 0.15f, 0.15f), "RETRY");
        }
    }

    // =====================================================
    //                  UPDATE BARS
    // =====================================================

    void UpdateEntity(BattleEntity e,
        Image hpBar, Image manaBar, Image speedBar, Image ultBar,
        TextMeshProUGUI hpText, TextMeshProUGUI manaText, TextMeshProUGUI shieldText)
    {
        if (e == null) return;

        // HP
        if (hpBar != null)
        {
            float hpRatio = (float)e.currentHP / e.maxHP;
            hpBar.fillAmount = hpRatio;

            if (hpRatio <= 0.25f) hpBar.color = hpLow;
            else if (hpRatio <= 0.5f) hpBar.color = hpMid;
            else hpBar.color = hpHigh;
        }

        if (hpText != null)
            hpText.text = $"{e.currentHP}/{e.maxHP}";

        // Mana
        if (manaBar != null)
            manaBar.fillAmount = (float)e.mana / e.maxMana;

        if (manaText != null)
            manaText.text = $"{e.mana}/{e.maxMana}";

        // Speed
        if (speedBar != null)
            speedBar.fillAmount = (float)e.speed / e.speedMax;

        // Ult
        if (ultBar != null)
            ultBar.fillAmount = (float)e.ultCharge / e.ultChargeMax;

        // Shield
        if (shieldText != null)
        {
            if (e.shield > 0)
            {
                shieldText.gameObject.SetActive(true);
                shieldText.text = e.shield.ToString();
            }
            else
            {
                shieldText.gameObject.SetActive(false);
            }
        }
    }

    // =====================================================
    //                  COMBO
    // =====================================================

    public void ShowCombo(int count)
    {
        if (comboText == null) return;

        comboText.gameObject.SetActive(true);
        comboText.text = $"COMBO x{count}!";

        if (count >= 5) comboText.color = new Color(1f, 0.2f, 0.1f);
        else if (count >= 3) comboText.color = new Color(1f, 0.6f, 0.1f);
        else comboText.color = new Color(1f, 0.9f, 0.2f);

        comboTimer = 1.5f;
    }

    // =====================================================
    //                  RESULT
    // =====================================================

    void ShowResult(string msg, Color color, string btnLabel)
    {
        gameEnded = true;

        if (resultPanel != null)
        {
            resultPanel.SetActive(true);
            if (resultText != null)
            {
                resultText.text = msg;
                resultText.color = color;
            }
            if (resultButton != null)
            {
                var txt = resultButton.GetComponentInChildren<TextMeshProUGUI>();
                if (txt != null) txt.text = btnLabel;
            }
        }
    }

    // =====================================================
    //                  BUTTONS
    // =====================================================

    void OnUltClick()
    {
        if (BattleManager.Instance != null)
            BattleManager.Instance.UseUltimate();
    }

    void OnResultClick()
    {
        UnityEngine.SceneManagement.SceneManager.LoadScene(
            UnityEngine.SceneManagement.SceneManager.GetActiveScene().buildIndex);
    }
}