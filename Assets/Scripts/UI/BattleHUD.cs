using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class BattleHUD : MonoBehaviour
{
    [Header("Character Panels")]
    public CharacterPanel playerPanel;
    public CharacterPanel enemyPanel;

    [Header("Turn Glow")]
    public GameObject playerTurnGlow;
    public GameObject enemyTurnGlow;

    [Header("Turn Text")]
    public TextMeshProUGUI turnText;

    [Header("Combo")]
    public TextMeshProUGUI comboText;
    float comboDisplayTimer;

    [Header("Skill Panel")]
    public Transform skillButtonContainer;
    public Button ultButton;
    public TextMeshProUGUI ultButtonText;

    [Header("Popup Anchors")]
    public Transform playerPopupAnchor;
    public Transform enemyPopupAnchor;

    [Header("Result")]
    public GameObject resultPanel;
    public TextMeshProUGUI resultText;
    public Button resultButton;

    bool gameEnded;

    void Start()
    {
        if (BattleManager.Instance != null)
        {
            if (playerPanel != null)
                playerPanel.SetEntity(BattleManager.Instance.player);
            if (enemyPanel != null)
                enemyPanel.SetEntity(BattleManager.Instance.enemy);

            if (BattleManager.Instance.popupPlayer == null && playerPopupAnchor != null)
                BattleManager.Instance.popupPlayer = playerPopupAnchor;
            if (BattleManager.Instance.popupEnemy == null && enemyPopupAnchor != null)
                BattleManager.Instance.popupEnemy = enemyPopupAnchor;
        }

        if (playerTurnGlow == null)
            playerTurnGlow = GameObject.Find("PlayerTurnGlow");
        if (enemyTurnGlow == null)
            enemyTurnGlow = GameObject.Find("EnemyTurnGlow");

        if (ultButton != null)
            ultButton.onClick.AddListener(OnUltClick);

        if (resultButton != null)
            resultButton.onClick.AddListener(OnResultClick);

        if (resultPanel != null)
            resultPanel.SetActive(false);

        HideCombo();
    }

    void Update()
    {
        if (BattleManager.Instance == null) return;
        if (gameEnded) return;

        bool isPlayerTurn = BattleManager.Instance.playerTurn;

        // ========== TURN GLOW ==========
        if (playerTurnGlow != null)
            playerTurnGlow.SetActive(isPlayerTurn);
        if (enemyTurnGlow != null)
            enemyTurnGlow.SetActive(!isPlayerTurn);

        // ========== TURN TEXT ==========
        if (turnText != null)
        {
            turnText.text = isPlayerTurn ? "LƯỢT CỦA BẠN" : "LƯỢT ĐỊCH";
            turnText.color = isPlayerTurn
                ? new Color(0.3f, 1f, 0.4f)
                : new Color(1f, 0.35f, 0.3f);
        }

        // ========== SKILL PANEL ==========
        if (skillButtonContainer != null)
            skillButtonContainer.gameObject.SetActive(isPlayerTurn);

        // ========== ULT BUTTON ==========
        if (ultButton != null)
        {
            bool canUlt = isPlayerTurn && BattleManager.Instance.player.ultimateReady;
            ultButton.interactable = canUlt;

            if (ultButtonText != null)
                ultButtonText.color = canUlt
                    ? new Color(1f, 0.9f, 0.2f)
                    : new Color(0.4f, 0.4f, 0.4f);
        }

        // ========== COMBO TIMER ==========
        if (comboDisplayTimer > 0f)
        {
            comboDisplayTimer -= Time.deltaTime;
            if (comboDisplayTimer <= 0f)
                HideCombo();
        }

        // ========== CHECK KẾT QUẢ ==========
        if (BattleManager.Instance.enemy.IsDead)
            ShowResult("CHIẾN THẮNG!", new Color(1f, 0.85f, 0.1f), "TIẾP TỤC");
        else if (BattleManager.Instance.player.IsDead)
            ShowResult("THẤT BẠI!", new Color(0.9f, 0.15f, 0.15f), "THỬ LẠI");
    }

    // =====================================================
    //                  COMBO
    // =====================================================

    public void ShowCombo(int comboCount)
    {
        if (comboText == null) return;

        comboText.gameObject.SetActive(true);
        comboText.text = $"COMBO x{comboCount}!";

        if (comboCount >= 5)
            comboText.color = new Color(1f, 0.2f, 0.1f);
        else if (comboCount >= 3)
            comboText.color = new Color(1f, 0.6f, 0.1f);
        else
            comboText.color = new Color(1f, 0.9f, 0.2f);

        comboDisplayTimer = 1.5f;
    }

    void HideCombo()
    {
        if (comboText != null)
            comboText.gameObject.SetActive(false);
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
            UnityEngine.SceneManagement.SceneManager.GetActiveScene().buildIndex
        );
    }

    // =====================================================
    //                  RESULT
    // =====================================================

    void ShowResult(string msg, Color color, string buttonLabel)
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
                resultButton.gameObject.SetActive(true);
                var btnText = resultButton.GetComponentInChildren<TextMeshProUGUI>();
                if (btnText != null)
                    btnText.text = buttonLabel;
            }
        }

        if (ultButton != null)
            ultButton.gameObject.SetActive(false);
        if (skillButtonContainer != null)
            skillButtonContainer.gameObject.SetActive(false);
    }
}