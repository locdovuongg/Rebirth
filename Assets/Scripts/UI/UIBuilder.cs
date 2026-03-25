using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// Gắn vào 1 GameObject trống. Khi Play sẽ tự tạo toàn bộ Canvas + UI.
/// </summary>
public class UIBuilder : MonoBehaviour
{
    [Header("Skill")]
    public SkillUser playerSkillUser;
    public Sprite skillButtonSprite;

    [Header("Sprites (kéo vào Inspector)")]
    public Sprite boardFrameSprite;
    public Sprite panelBGSprite;
    public Sprite avatarBorderSprite;
    public Sprite barBGSprite;
    public Sprite barFillSprite;
    public Sprite glowSprite;
    public Sprite topDecoSprite;
    public Sprite playerAvatarSprite;
    public Sprite enemyAvatarSprite;
    public Sprite buttonSprite;

    [Header("Colors")]
    public Color bgColor = new Color(0.08f, 0.06f, 0.1f);
    public Color frameColor = new Color(0.35f, 0.25f, 0.18f);
    public Color hpColor = new Color(0.2f, 0.72f, 0.2f);
    public Color manaColor = new Color(0.2f, 0.4f, 0.82f);
    public Color barBGColor = new Color(0.12f, 0.1f, 0.1f);
    public Color borderGold = new Color(0.85f, 0.7f, 0.2f);
    public Color turnGlowColor = new Color(1f, 0.9f, 0.3f, 0.7f);

    // internal
    Canvas canvas;
    RectTransform safeArea;
    BattleHUD battleHUD;

    // lưu tạm để assign sau
    CharacterPanel playerCP;
    CharacterPanel enemyCP;
    Button ultBtn;
    TextMeshProUGUI ultTMP;
    Transform skillContainer;
    TextMeshProUGUI turnTMP;
    TextMeshProUGUI comboTMP;
    Transform playerPopupAnchor;
    Transform enemyPopupAnchor;

    void Awake()
    {
        BuildAll();
    }

    void BuildAll()
    {
        // ========== CANVAS ==========
        GameObject canvasObj = new GameObject("BattleCanvas");
        canvas = canvasObj.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 10;

        var scaler = canvasObj.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1080, 1920);
        scaler.matchWidthOrHeight = 0.5f;

        canvasObj.AddComponent<GraphicRaycaster>();

        // ========== SAFE AREA ==========
        safeArea = CreatePanel(canvasObj.transform, "SafeArea",
            Vector2.zero, Vector2.one, new Vector2(0.5f, 0.5f));
        safeArea.gameObject.AddComponent<SafeArea>();

        // ========== BG ==========
        CreateImage(safeArea, "Background",
            Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero,
            null, bgColor);

        // ========== BUILD ALL SECTIONS ==========
        BuildTopDecor();
        BuildBoardFrame();
        BuildTurnText();
        BuildComboText();
        BuildBottomPanel();
        BuildPopupAnchors();
        BuildResultPanel();

        // ========== ASSIGN HUD ==========
        AssignHUD();
    }

    // =====================================================
    //                  TOP DECORATION
    // =====================================================

    void BuildTopDecor()
    {
        var top = CreatePanel(safeArea, "TopDecoration",
            new Vector2(0, 1), new Vector2(1, 1),
            new Vector2(0.5f, 1));
        top.sizeDelta = new Vector2(0, 280);

        var img = top.gameObject.AddComponent<Image>();
        if (topDecoSprite != null)
        {
            img.sprite = topDecoSprite;
            img.preserveAspect = true;
            img.color = Color.white;
        }
        else
        {
            img.color = new Color(0.05f, 0.03f, 0.08f);
        }
    }

    // =====================================================
    //                  BOARD FRAME
    // =====================================================

    void BuildBoardFrame()
    {
        var frame = CreatePanel(safeArea, "BoardFrame",
            new Vector2(0, 0), new Vector2(1, 1),
            new Vector2(0.5f, 0.5f));
        frame.offsetMin = new Vector2(20, 500);
        frame.offsetMax = new Vector2(-20, -260);

        var frameImg = frame.gameObject.AddComponent<Image>();
        if (boardFrameSprite != null)
        {
            frameImg.sprite = boardFrameSprite;
            frameImg.type = Image.Type.Sliced;
        }
        frameImg.color = frameColor;

        frame.gameObject.AddComponent<BoardFrameEffect>().frameImage = frameImg;
    }

    // =====================================================
    //                  TURN TEXT
    // =====================================================

    void BuildTurnText()
    {
        var turnRT = CreatePanel(safeArea, "TurnText",
            new Vector2(0.5f, 0), new Vector2(0.5f, 0),
            new Vector2(0.5f, 0));
        turnRT.sizeDelta = new Vector2(500, 60);
        turnRT.anchoredPosition = new Vector2(0, 490);

        turnTMP = turnRT.gameObject.AddComponent<TextMeshProUGUI>();
        turnTMP.text = "LƯỢT CỦA BẠN";
        turnTMP.fontSize = 32;
        turnTMP.alignment = TextAlignmentOptions.Center;
        turnTMP.fontStyle = FontStyles.Bold;
        turnTMP.color = new Color(0.3f, 1f, 0.4f);

        // outline
        var outline = turnRT.gameObject.AddComponent<Outline>();
        outline.effectColor = Color.black;
        outline.effectDistance = new Vector2(2, -2);
    }

    // =====================================================
    //                  COMBO TEXT
    // =====================================================

    void BuildComboText()
    {
        var comboRT = CreatePanel(safeArea, "ComboText",
            new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f),
            new Vector2(0.5f, 0.5f));
        comboRT.sizeDelta = new Vector2(500, 80);
        comboRT.anchoredPosition = new Vector2(0, 100);

        comboTMP = comboRT.gameObject.AddComponent<TextMeshProUGUI>();
        comboTMP.text = "";
        comboTMP.fontSize = 48;
        comboTMP.alignment = TextAlignmentOptions.Center;
        comboTMP.fontStyle = FontStyles.Bold;
        comboTMP.color = Color.yellow;

        var outline = comboRT.gameObject.AddComponent<Outline>();
        outline.effectColor = Color.black;
        outline.effectDistance = new Vector2(2, -2);

        comboRT.gameObject.SetActive(false);
    }

    // =====================================================
    //                  BOTTOM PANEL
    // =====================================================

    void BuildBottomPanel()
    {
        // --- Container ---
        var bottom = CreatePanel(safeArea, "BottomPanel",
            new Vector2(0, 0), new Vector2(1, 0),
            new Vector2(0.5f, 0));
        bottom.sizeDelta = new Vector2(0, 480);

        var bottomBG = bottom.gameObject.AddComponent<Image>();
        if (panelBGSprite != null)
        {
            bottomBG.sprite = panelBGSprite;
            bottomBG.type = Image.Type.Sliced;
        }
        bottomBG.color = new Color(0.1f, 0.08f, 0.12f);

        // --- Player Side (LEFT, phần trên) ---
        var playerSide = CreatePanel(bottom, "PlayerSide",
            new Vector2(0, 0.42f), new Vector2(0.48f, 1),
            new Vector2(0, 1));
        playerSide.offsetMin = new Vector2(15, 0);
        playerSide.offsetMax = new Vector2(0, -10);

        playerCP = BuildCharacterSide(playerSide, playerAvatarSprite, false);

        // --- Enemy Side (RIGHT, phần trên) ---
        var enemySide = CreatePanel(bottom, "EnemySide",
            new Vector2(0.52f, 0.42f), new Vector2(1, 1),
            new Vector2(1, 1));
        enemySide.offsetMin = new Vector2(0, 0);
        enemySide.offsetMax = new Vector2(-15, -10);

        enemyCP = BuildCharacterSide(enemySide, enemyAvatarSprite, true);

        // --- Ult Button (giữa 2 portrait) ---
        BuildUltButton(bottom);

        // --- Skill Panel (phần dưới, thay skip button) ---
        BuildSkillPanel(bottom);
    }

    // =====================================================
    //                  CHARACTER SIDE
    // =====================================================

    CharacterPanel BuildCharacterSide(RectTransform parent, Sprite avatar, bool isRight)
    {
        var cp = parent.gameObject.AddComponent<CharacterPanel>();

        float avatarSize = 150;
        float barWidth = 200;
        float barHeight = 30;
        float barGap = 10;

        // ========== TURN GLOW (behind avatar) ==========
        var glow = CreatePanel(parent,
            isRight ? "EnemyTurnGlow" : "PlayerTurnGlow",
            isRight ? new Vector2(1, 0.5f) : new Vector2(0, 0.5f),
            isRight ? new Vector2(1, 0.5f) : new Vector2(0, 0.5f),
            new Vector2(0.5f, 0.5f));
        glow.sizeDelta = new Vector2(avatarSize + 24, avatarSize + 24);
        glow.anchoredPosition = new Vector2(
            isRight ? -avatarSize / 2 - 10 : avatarSize / 2 + 10, 0);

        var glowImg = glow.gameObject.AddComponent<Image>();
        if (glowSprite != null)
            glowImg.sprite = glowSprite;
        glowImg.color = turnGlowColor;

        var turnGlowComp = glow.gameObject.AddComponent<TurnGlow>();
        turnGlowComp.glowImage = glowImg;
        glow.gameObject.SetActive(false);

        // ========== AVATAR FRAME ==========
        var avatarFrame = CreatePanel(parent, "AvatarFrame",
            isRight ? new Vector2(1, 0.5f) : new Vector2(0, 0.5f),
            isRight ? new Vector2(1, 0.5f) : new Vector2(0, 0.5f),
            new Vector2(0.5f, 0.5f));
        avatarFrame.sizeDelta = new Vector2(avatarSize, avatarSize);
        avatarFrame.anchoredPosition = new Vector2(
            isRight ? -avatarSize / 2 - 10 : avatarSize / 2 + 10, 0);

        var borderImg = avatarFrame.gameObject.AddComponent<Image>();
        if (avatarBorderSprite != null)
        {
            borderImg.sprite = avatarBorderSprite;
            borderImg.type = Image.Type.Sliced;
        }
        borderImg.color = borderGold;
        cp.avatarBorder = borderImg;

        // Avatar image (inside frame)
        var avatarImg = CreatePanel(avatarFrame, "Avatar",
            Vector2.zero, Vector2.one,
            new Vector2(0.5f, 0.5f));
        avatarImg.offsetMin = new Vector2(8, 8);
        avatarImg.offsetMax = new Vector2(-8, -8);

        var avImg = avatarImg.gameObject.AddComponent<Image>();
        if (avatar != null)
            avImg.sprite = avatar;
        avImg.color = avatar != null ? Color.white : new Color(0.3f, 0.3f, 0.4f);
        cp.avatarImage = avImg;

        // ========== BARS ==========
        float barX = isRight ? -avatarSize - 25 : avatarSize + 25;
        float barAnchorX = isRight ? 1f : 0f;

        // HP Bar
        cp.hpFill = BuildBar(parent, "HPBar",
            barX, barGap / 2 + barHeight / 2,
            barWidth, barHeight,
            barAnchorX, hpColor, isRight);

        // Mana Bar
        cp.manaFill = BuildBar(parent, "ManaBar",
            barX, -(barGap / 2 + barHeight / 2),
            barWidth, barHeight,
            barAnchorX, manaColor, isRight);

        return cp;
    }

    // =====================================================
    //                  BAR BUILDER
    // =====================================================

    Image BuildBar(RectTransform parent, string name,
        float posX, float posY,
        float width, float height,
        float anchorX, Color fillColor, bool growLeft)
    {
        Vector2 anchor = new Vector2(anchorX, 0.5f);

        // BG
        var barBG = CreatePanel(parent, name + "_BG", anchor, anchor, anchor);
        barBG.sizeDelta = new Vector2(width, height);
        barBG.anchoredPosition = new Vector2(posX, posY);

        var bgImg = barBG.gameObject.AddComponent<Image>();
        if (barBGSprite != null)
        {
            bgImg.sprite = barBGSprite;
            bgImg.type = Image.Type.Sliced;
        }
        bgImg.color = barBGColor;

        // Fill
        var fill = CreatePanel(barBG, name + "_Fill",
            Vector2.zero, Vector2.one,
            new Vector2(0, 0.5f));
        fill.offsetMin = new Vector2(3, 3);
        fill.offsetMax = new Vector2(-3, -3);

        var fillImg = fill.gameObject.AddComponent<Image>();
        if (barFillSprite != null)
            fillImg.sprite = barFillSprite;
        fillImg.color = fillColor;
        fillImg.type = Image.Type.Filled;
        fillImg.fillMethod = Image.FillMethod.Horizontal;
        fillImg.fillOrigin = growLeft ? 1 : 0;
        fillImg.fillAmount = 1f;

        return fillImg;
    }

    // =====================================================
    //                  ULT BUTTON
    // =====================================================

    void BuildUltButton(RectTransform bottom)
    {
        var ultRT = CreatePanel(bottom, "UltButton",
            new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f),
            new Vector2(0.5f, 0.5f));
        ultRT.sizeDelta = new Vector2(140, 55);
        ultRT.anchoredPosition = new Vector2(0, 50);

        var ultImg = ultRT.gameObject.AddComponent<Image>();
        if (buttonSprite != null)
        {
            ultImg.sprite = buttonSprite;
            ultImg.type = Image.Type.Sliced;
        }
        ultImg.color = new Color(0.8f, 0.65f, 0.15f);

        ultBtn = ultRT.gameObject.AddComponent<Button>();
        ultBtn.targetGraphic = ultImg;

        var colors = ultBtn.colors;
        colors.normalColor = new Color(0.8f, 0.65f, 0.15f);
        colors.highlightedColor = new Color(0.95f, 0.8f, 0.25f);
        colors.pressedColor = new Color(0.6f, 0.5f, 0.1f);
        colors.disabledColor = new Color(0.3f, 0.25f, 0.15f);
        ultBtn.colors = colors;

        // text
        var textObj = new GameObject("UltText");
        textObj.transform.SetParent(ultRT, false);

        var textRT = textObj.AddComponent<RectTransform>();
        textRT.anchorMin = Vector2.zero;
        textRT.anchorMax = Vector2.one;
        textRT.offsetMin = Vector2.zero;
        textRT.offsetMax = Vector2.zero;

        ultTMP = textObj.AddComponent<TextMeshProUGUI>();
        ultTMP.text = "⚡ ULT";
        ultTMP.fontSize = 24;
        ultTMP.alignment = TextAlignmentOptions.Center;
        ultTMP.fontStyle = FontStyles.Bold;
        ultTMP.color = Color.white;
    }

    // =====================================================
    //                  SKILL PANEL
    // =====================================================

    void BuildSkillPanel(RectTransform bottomPanel)
    {
        // --- Container chiếm phần dưới bottom panel ---
        var skillArea = CreatePanel(bottomPanel, "SkillPanel",
            new Vector2(0, 0), new Vector2(1, 0.38f),
            new Vector2(0.5f, 0));
        skillArea.offsetMin = new Vector2(10, 8);
        skillArea.offsetMax = new Vector2(-10, 0);

        // nền riêng cho skill panel
        var skillBG = skillArea.gameObject.AddComponent<Image>();
        skillBG.color = new Color(0.08f, 0.06f, 0.1f, 0.5f);

        // HorizontalLayoutGroup
        var layout = skillArea.gameObject.AddComponent<HorizontalLayoutGroup>();
        layout.spacing = 12;
        layout.childAlignment = TextAnchor.MiddleCenter;
        layout.childControlWidth = false;
        layout.childControlHeight = false;
        layout.childForceExpandWidth = false;
        layout.childForceExpandHeight = false;
        layout.padding = new RectOffset(15, 15, 5, 5);

        // SkillUI component
        var skillUI = skillArea.gameObject.AddComponent<SkillUI>();
        skillUI.playerSkills = playerSkillUser;
        skillUI.buttonContainer = skillArea;
        skillUI.buttonBGSprite = skillButtonSprite;
        skillUI.buttonSize = 110;

        skillContainer = skillArea;
    }

    // =====================================================
    //                  POPUP ANCHORS
    // =====================================================

    void BuildPopupAnchors()
    {
        var pAnchor = new GameObject("PlayerPopupAnchor");
        pAnchor.transform.SetParent(transform);
        pAnchor.transform.position = new Vector3(-2f, -3f, 0);
        playerPopupAnchor = pAnchor.transform;

        var eAnchor = new GameObject("EnemyPopupAnchor");
        eAnchor.transform.SetParent(transform);
        eAnchor.transform.position = new Vector3(2f, -3f, 0);
        enemyPopupAnchor = eAnchor.transform;
    }

    // =====================================================
    //                  RESULT PANEL
    // =====================================================

    void BuildResultPanel()
    {
        var panel = CreatePanel(safeArea, "ResultPanel",
            Vector2.zero, Vector2.one,
            new Vector2(0.5f, 0.5f));
        panel.offsetMin = Vector2.zero;
        panel.offsetMax = Vector2.zero;

        var panelBG = panel.gameObject.AddComponent<Image>();
        panelBG.color = new Color(0, 0, 0, 0.75f);

        // Result Text
        var textObj = new GameObject("ResultText");
        textObj.transform.SetParent(panel, false);

        var textRT = textObj.AddComponent<RectTransform>();
        textRT.anchorMin = new Vector2(0.1f, 0.45f);
        textRT.anchorMax = new Vector2(0.9f, 0.65f);
        textRT.offsetMin = Vector2.zero;
        textRT.offsetMax = Vector2.zero;

        var resultTMP = textObj.AddComponent<TextMeshProUGUI>();
        resultTMP.text = "";
        resultTMP.fontSize = 72;
        resultTMP.alignment = TextAlignmentOptions.Center;
        resultTMP.color = Color.yellow;
        resultTMP.fontStyle = FontStyles.Bold;
        resultTMP.enableWordWrapping = true;

        var outline = textObj.AddComponent<Outline>();
        outline.effectColor = Color.black;
        outline.effectDistance = new Vector2(3, -3);

        // Result Button
        var btnRT = CreatePanel(panel, "ResultButton",
            new Vector2(0.5f, 0.3f), new Vector2(0.5f, 0.3f),
            new Vector2(0.5f, 0.5f));
        btnRT.sizeDelta = new Vector2(300, 70);

        var btnImg = btnRT.gameObject.AddComponent<Image>();
        if (buttonSprite != null)
        {
            btnImg.sprite = buttonSprite;
            btnImg.type = Image.Type.Sliced;
        }
        btnImg.color = new Color(0.6f, 0.5f, 0.3f);

        var resultBtn = btnRT.gameObject.AddComponent<Button>();
        resultBtn.targetGraphic = btnImg;

        var btnTextObj = new GameObject("ButtonText");
        btnTextObj.transform.SetParent(btnRT, false);

        var btnTextRT = btnTextObj.AddComponent<RectTransform>();
        btnTextRT.anchorMin = Vector2.zero;
        btnTextRT.anchorMax = Vector2.one;
        btnTextRT.offsetMin = Vector2.zero;
        btnTextRT.offsetMax = Vector2.zero;

        var btnTMP = btnTextObj.AddComponent<TextMeshProUGUI>();
        btnTMP.text = "TIẾP TỤC";
        btnTMP.fontSize = 30;
        btnTMP.alignment = TextAlignmentOptions.Center;
        btnTMP.fontStyle = FontStyles.Bold;
        btnTMP.color = Color.white;

        panel.gameObject.SetActive(false);

        // --- lưu tạm để assign sau ---
        _resultPanel = panel.gameObject;
        _resultText = resultTMP;
        _resultButton = resultBtn;
    }

    // tạm giữ để assign
    GameObject _resultPanel;
    TextMeshProUGUI _resultText;
    Button _resultButton;

    // =====================================================
    //                  ASSIGN HUD
    // =====================================================

    void AssignHUD()
    {
        battleHUD = safeArea.gameObject.AddComponent<BattleHUD>();

        // character panels
        battleHUD.playerPanel = playerCP;
        battleHUD.enemyPanel = enemyCP;

        // turn glow (tìm theo tên)
        battleHUD.playerTurnGlow = GameObject.Find("PlayerTurnGlow");
        battleHUD.enemyTurnGlow = GameObject.Find("EnemyTurnGlow");

        // turn text
        battleHUD.turnText = turnTMP;

        // combo
        battleHUD.comboText = comboTMP;

        // skill
        battleHUD.skillButtonContainer = skillContainer;

        // ult
        battleHUD.ultButton = ultBtn;
        battleHUD.ultButtonText = ultTMP;

        // popup anchors
        battleHUD.playerPopupAnchor = playerPopupAnchor;
        battleHUD.enemyPopupAnchor = enemyPopupAnchor;

        // result
        battleHUD.resultPanel = _resultPanel;
        battleHUD.resultText = _resultText;
        battleHUD.resultButton = _resultButton;
    }

    // =====================================================
    //                  HELPERS
    // =====================================================

    RectTransform CreatePanel(Transform parent, string name,
        Vector2 anchorMin, Vector2 anchorMax, Vector2 pivot)
    {
        var obj = new GameObject(name);
        obj.transform.SetParent(parent, false);

        var rt = obj.AddComponent<RectTransform>();
        rt.anchorMin = anchorMin;
        rt.anchorMax = anchorMax;
        rt.pivot = pivot;
        rt.offsetMin = Vector2.zero;
        rt.offsetMax = Vector2.zero;

        return rt;
    }

    RectTransform CreatePanel(Transform parent, string name,
        Vector2 anchorMin, Vector2 anchorMax,
        Vector2 pivot, Vector2 sizeDelta)
    {
        var rt = CreatePanel(parent, name, anchorMin, anchorMax, pivot);
        rt.sizeDelta = sizeDelta;
        return rt;
    }

    Image CreateImage(RectTransform parent, string name,
        Vector2 anchorMin, Vector2 anchorMax,
        Vector2 offsetMin, Vector2 offsetMax,
        Sprite sprite, Color color)
    {
        var rt = CreatePanel(parent, name, anchorMin, anchorMax, new Vector2(0.5f, 0.5f));
        rt.offsetMin = offsetMin;
        rt.offsetMax = offsetMax;

        var img = rt.gameObject.AddComponent<Image>();
        if (sprite != null)
            img.sprite = sprite;
        img.color = color;

        return img;
    }
}