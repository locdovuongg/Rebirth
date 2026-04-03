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
        GenerateMissingSprites();
        BuildAll();
    }

    /// <summary>
    /// Tự tạo sprite nếu chưa kéo vào Inspector
    /// </summary>
    void GenerateMissingSprites()
    {
        if (boardFrameSprite == null)   boardFrameSprite = SpriteGenerator.BoardFrame();
        if (panelBGSprite == null)      panelBGSprite = SpriteGenerator.PanelBG();
        if (avatarBorderSprite == null)  avatarBorderSprite = SpriteGenerator.AvatarBorder();
        if (barBGSprite == null)        barBGSprite = SpriteGenerator.BarBG();
        if (barFillSprite == null)      barFillSprite = SpriteGenerator.BarFill();
        if (glowSprite == null)         glowSprite = SpriteGenerator.Glow();
        if (topDecoSprite == null)      topDecoSprite = SpriteGenerator.TopDecoration();
        if (playerAvatarSprite == null)  playerAvatarSprite = SpriteGenerator.PlayerAvatar();
        if (enemyAvatarSprite == null)  enemyAvatarSprite = SpriteGenerator.EnemyAvatar();
        if (buttonSprite == null)       buttonSprite = SpriteGenerator.ButtonSprite();
        if (skillButtonSprite == null)  skillButtonSprite = SpriteGenerator.SkillButton();
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

        // ========== BG — BỎ, không tạo background che game ==========
        // UI chỉ hiện ở top, bottom, không phủ lên board

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
        // Chiếm 6.3% trên màn hình
        var top = CreatePanel(safeArea, "TopDecoration",
            new Vector2(0, 0.937f), new Vector2(1, 1),
            new Vector2(0.5f, 1));
        top.offsetMin = Vector2.zero;
        top.offsetMax = Vector2.zero;

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
        img.raycastTarget = false; // không chặn click
    }

    // =====================================================
    //                  BOARD FRAME
    // =====================================================

    void BuildBoardFrame()
    {
        // Board frame nằm giữa top decor (120px) và turn text (trên bottom panel 550px + 55px gap)
        // Dùng anchor tương đối để responsive
        // bottomPanel = 550/1920 ≈ 0.286, turnText gap = 55/1920 ≈ 0.029
        // topDecor = 120/1920 ≈ 0.063
        float bottomAnchor = 0.32f;  // trên bottom panel + turn text
        float topAnchor = 0.935f;    // dưới top decor

        var frame = CreatePanel(safeArea, "BoardFrame",
            new Vector2(0, bottomAnchor), new Vector2(1, topAnchor),
            new Vector2(0.5f, 0.5f));
        frame.offsetMin = new Vector2(10, 5);    // padding trái, dưới
        frame.offsetMax = new Vector2(-10, -5);   // padding phải, trên

        var frameImg = frame.gameObject.AddComponent<Image>();
        if (boardFrameSprite != null)
        {
            frameImg.sprite = boardFrameSprite;
            frameImg.type = Image.Type.Sliced;
        }
        frameImg.color = frameColor;
        frameImg.raycastTarget = false; // không chặn click vào board

        frame.gameObject.AddComponent<BoardFrameEffect>().frameImage = frameImg;
    }

    // =====================================================
    //                  TURN TEXT
    // =====================================================

    void BuildTurnText()
    {
        // Nằm ngay trên bottom panel
        // bottomPanel top anchor = 550/1920 ≈ 0.286
        float anchor = 0.295f;

        var turnRT = CreatePanel(safeArea, "TurnText",
            new Vector2(0, anchor), new Vector2(1, anchor),
            new Vector2(0.5f, 0));
        turnRT.sizeDelta = new Vector2(0, 50);
        turnRT.anchoredPosition = Vector2.zero;

        turnTMP = turnRT.gameObject.AddComponent<TextMeshProUGUI>();
        turnTMP.text = "LƯỢT CỦA BẠN";
        turnTMP.fontSize = 32;
        turnTMP.alignment = TextAlignmentOptions.Center;
        turnTMP.fontStyle = FontStyles.Bold;
        turnTMP.color = new Color(0.3f, 1f, 0.4f);
        turnTMP.raycastTarget = false;

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
        comboRT.anchoredPosition = new Vector2(0, 200);

        comboTMP = comboRT.gameObject.AddComponent<TextMeshProUGUI>();
        comboTMP.text = "";
        comboTMP.fontSize = 48;
        comboTMP.alignment = TextAlignmentOptions.Center;
        comboTMP.fontStyle = FontStyles.Bold;
        comboTMP.color = Color.yellow;
        comboTMP.raycastTarget = false;

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
        // --- Container chiếm 29% dưới màn hình ---
        var bottom = CreatePanel(safeArea, "BottomPanel",
            new Vector2(0, 0), new Vector2(1, 0.286f),
            new Vector2(0.5f, 0));
        bottom.offsetMin = Vector2.zero;
        bottom.offsetMax = Vector2.zero;

        var bottomBG = bottom.gameObject.AddComponent<Image>();
        if (panelBGSprite != null)
        {
            bottomBG.sprite = panelBGSprite;
            bottomBG.type = Image.Type.Sliced;
        }
        bottomBG.color = new Color(0.1f, 0.08f, 0.12f);

        // --- Player Side (LEFT, phần trên) ---
        var playerSide = CreatePanel(bottom, "PlayerSide",
            new Vector2(0, 0.38f), new Vector2(0.48f, 1),
            new Vector2(0, 1));
        playerSide.offsetMin = new Vector2(10, 0);
        playerSide.offsetMax = new Vector2(0, -8);

        playerCP = BuildCharacterSide(playerSide, playerAvatarSprite, false);

        // --- Enemy Side (RIGHT, phần trên) ---
        var enemySide = CreatePanel(bottom, "EnemySide",
            new Vector2(0.52f, 0.38f), new Vector2(1, 1),
            new Vector2(1, 1));
        enemySide.offsetMin = new Vector2(0, 0);
        enemySide.offsetMax = new Vector2(-10, -8);

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

        float avatarSize = 110;
        float barWidth = 160;
        float barHeight = 20;
        float barGap = 5;

        // ========== TURN GLOW (behind avatar) ==========
        var glow = CreatePanel(parent,
            isRight ? "EnemyTurnGlow" : "PlayerTurnGlow",
            isRight ? new Vector2(1, 0.5f) : new Vector2(0, 0.5f),
            isRight ? new Vector2(1, 0.5f) : new Vector2(0, 0.5f),
            new Vector2(0.5f, 0.5f));
        glow.sizeDelta = new Vector2(avatarSize + 20, avatarSize + 20);
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
        float barX = isRight ? -avatarSize - 15 : avatarSize + 15;
        float barAnchorX = isRight ? 1f : 0f;

        // 4 bars stacked: HP, Mana, Speed, Ult
        float totalBars = 4;
        float totalHeight = totalBars * barHeight + (totalBars - 1) * barGap;
        float topY = totalHeight / 2f - barHeight / 2f;

        // HP Bar (top)
        cp.hpFill = BuildBar(parent, "HPBar",
            barX, topY,
            barWidth, barHeight,
            barAnchorX, hpColor, isRight);

        // Mana Bar
        cp.manaFill = BuildBar(parent, "ManaBar",
            barX, topY - (barHeight + barGap),
            barWidth, barHeight,
            barAnchorX, manaColor, isRight);

        // Speed Bar (extra turn)
        cp.speedFill = BuildBar(parent, "SpeedBar",
            barX, topY - 2 * (barHeight + barGap),
            barWidth, barHeight,
            barAnchorX, new Color(0.4f, 0.8f, 1f), isRight);

        // Ult Bar (ultimate)
        cp.ultFill = BuildBar(parent, "UltBar",
            barX, topY - 3 * (barHeight + barGap),
            barWidth, barHeight,
            barAnchorX, new Color(1f, 0.5f, 0.1f), isRight);

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
        // Nằm giữa, ngay trên skill panel
        var ultRT = CreatePanel(bottom, "UltButton",
            new Vector2(0.5f, 0.38f), new Vector2(0.5f, 0.38f),
            new Vector2(0.5f, 1));
        ultRT.sizeDelta = new Vector2(130, 48);
        ultRT.anchoredPosition = new Vector2(0, -5);

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
            new Vector2(0, 0), new Vector2(1, 0.35f),
            new Vector2(0.5f, 0));
        skillArea.offsetMin = new Vector2(8, 5);
        skillArea.offsetMax = new Vector2(-8, 0);

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
        skillUI.buttonSize = 90;

        skillContainer = skillArea;
    }

    // =====================================================
    //                  POPUP ANCHORS
    // =====================================================

    void BuildPopupAnchors()
    {
        // Popup nằm trong board area
        // Board center ở world (0, offset) — CameraFit dịch BoardManager lên
        // Player popup: bên trái-dưới board
        // Enemy popup: bên phải-trên board
        var pAnchor = new GameObject("PlayerPopupAnchor");
        pAnchor.transform.SetParent(transform);
        pAnchor.transform.position = new Vector3(-1f, -1f, 0);
        playerPopupAnchor = pAnchor.transform;

        var eAnchor = new GameObject("EnemyPopupAnchor");
        eAnchor.transform.SetParent(transform);
        eAnchor.transform.position = new Vector3(1f, 1f, 0);
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
        resultTMP.textWrappingMode = TextWrappingModes.Normal;

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