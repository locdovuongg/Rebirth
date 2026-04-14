using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;
using System.Collections.Generic;

/// <summary>
/// Màn hình chọn nhân vật.
/// Container hiển thị các nhân vật CHƯA chọn (click để swap).
/// Portrait hiển thị nhân vật ĐANG chọn.
/// </summary>
public class CharacterSelectUI : MonoBehaviour
{
    [Header("Danh sách nhân vật")]
    public CharacterInfo[] characters;

    [Header("UI hiển thị nhân vật đang chọn")]
    public Image portraitImage;
    public TextMeshProUGUI nameText;
    public TextMeshProUGUI statsText;

    [Header("Container chứa các nút chọn (nhân vật chưa chọn)")]
    public Transform buttonContainer;

    [Header("Prefab nút chọn nhân vật")]
    public GameObject characterButtonPrefab;

    [Header("Scene tiếp theo")]
    public string nextSceneName = "MapSelect";

    [Header("Enemy (mặc định)")]
    public CharacterInfo defaultEnemy;

    int selectedIndex = 0;
    List<GameObject> buttonObjects = new();

    void Start()
    {
        // Tắt raycast trên các UI preview
        if (portraitImage != null) portraitImage.raycastTarget = false;
        if (nameText != null) nameText.raycastTarget = false;
        if (statsText != null) statsText.raycastTarget = false;

        // Chọn nhân vật đầu tiên
        if (characters.Length > 0)
            SelectCharacter(0);
    }

    /// <summary>
    /// Xóa button cũ, tạo lại button cho các nhân vật CHƯA được chọn
    /// </summary>
    void RebuildButtons()
    {
        // Xóa button cũ
        foreach (var obj in buttonObjects)
        {
            if (obj != null) Destroy(obj);
        }
        buttonObjects.Clear();

        // Tạo button cho mỗi nhân vật CHƯA chọn
        for (int i = 0; i < characters.Length; i++)
        {
            if (i == selectedIndex) continue; // bỏ qua nhân vật đang chọn
            CreateButton(i);
        }
    }

    void CreateButton(int index)
    {
        CharacterInfo c = characters[index];

        GameObject btnObj;

        if (characterButtonPrefab != null)
        {
            btnObj = Instantiate(characterButtonPrefab, buttonContainer);
        }
        else
        {
            // Tự tạo button đơn giản nếu chưa có prefab
            btnObj = new GameObject("Btn_" + c.characterName);
            btnObj.transform.SetParent(buttonContainer, false);

            var img = btnObj.AddComponent<Image>();
            img.color = new Color(0.15f, 0.12f, 0.2f);

            var rt = btnObj.GetComponent<RectTransform>();
            rt.sizeDelta = new Vector2(150, 180);
        }

        // Đảm bảo luôn có Button component
        var btn = btnObj.GetComponent<Button>();
        if (btn == null)
            btn = btnObj.AddComponent<Button>();

        // Set avatar
        var avatarImg = btnObj.transform.Find("Avatar");
        if (avatarImg != null)
        {
            var img = avatarImg.GetComponent<Image>();
            if (img != null && c.avatar != null)
                img.sprite = c.avatar;
            // Tắt raycast trên avatar con
            if (img != null) img.raycastTarget = false;
        }
        else if (c.avatar != null)
        {
            var avatarObj = new GameObject("Avatar");
            avatarObj.transform.SetParent(btnObj.transform, false);
            var aImg = avatarObj.AddComponent<Image>();
            aImg.sprite = c.avatar;
            aImg.preserveAspect = true;
            aImg.raycastTarget = false;
            var aRT = avatarObj.GetComponent<RectTransform>();
            aRT.anchorMin = new Vector2(0.1f, 0.25f);
            aRT.anchorMax = new Vector2(0.9f, 0.95f);
            aRT.offsetMin = Vector2.zero;
            aRT.offsetMax = Vector2.zero;
        }

        // Set tên
        var nameObj = btnObj.transform.Find("Name");
        if (nameObj != null)
        {
            var tmp = nameObj.GetComponent<TextMeshProUGUI>();
            if (tmp != null)
            {
                tmp.text = c.characterName;
                tmp.raycastTarget = false;
            }
        }
        else
        {
            var nObj = new GameObject("Name");
            nObj.transform.SetParent(btnObj.transform, false);
            var tmp = nObj.AddComponent<TextMeshProUGUI>();
            tmp.text = c.characterName;
            tmp.fontSize = 18;
            tmp.alignment = TextAlignmentOptions.Center;
            tmp.color = Color.white;
            tmp.raycastTarget = false;
            var nRT = nObj.GetComponent<RectTransform>();
            nRT.anchorMin = new Vector2(0, 0);
            nRT.anchorMax = new Vector2(1, 0.25f);
            nRT.offsetMin = Vector2.zero;
            nRT.offsetMax = Vector2.zero;
        }

        // Tắt raycast trên tất cả child (chỉ giữ button root nhận click)
        foreach (var graphic in btnObj.GetComponentsInChildren<UnityEngine.UI.Graphic>())
        {
            if (graphic.gameObject != btnObj)
                graphic.raycastTarget = false;
        }

        // Click event
        int idx = index;
        btn.onClick.AddListener(() =>
        {
            Debug.Log($"[CharacterSelect] Clicked: {characters[idx].characterName}");
            SelectCharacter(idx);
        });

        buttonObjects.Add(btnObj);
    }

    public void SelectCharacter(int index)
    {
        selectedIndex = index;
        CharacterInfo c = characters[index];

        // Cập nhật preview (nhân vật đang chọn)
        if (portraitImage != null && c.portrait != null)
            portraitImage.sprite = c.portrait;
        else if (portraitImage != null && c.avatar != null)
            portraitImage.sprite = c.avatar;

        if (nameText != null)
            nameText.text = c.characterName;

        if (statsText != null)
        {
            statsText.text = $"HP: {c.maxHP}\n" +
                             $"Mana: {c.maxMana}\n" +
                             $"ATK: {c.swordDamage}\n" +
                             $"ULT DMG: {c.ultimateDamage}\n" +
                             $"Skills: {(c.skills != null ? c.skills.Length : 0)}";
        }

        // Rebuild button → chỉ hiện nhân vật CHƯA chọn
        RebuildButtons();
    }

    /// <summary>
    /// Gắn vào nút "START BATTLE"
    /// </summary>
    public void ConfirmAndStart()
    {
        if (characters == null || characters.Length == 0)
        {
            Debug.LogError("CharacterSelectUI: characters array is empty! Kéo CharacterInfo assets vào Inspector.");
            return;
        }

        if (selectedIndex < 0 || selectedIndex >= characters.Length)
        {
            Debug.LogError($"CharacterSelectUI: selectedIndex ({selectedIndex}) out of range. Resetting to 0.");
            selectedIndex = 0;
        }

        // Đảm bảo GameData tồn tại
        if (GameData.Instance == null)
        {
            var go = new GameObject("GameData");
            go.AddComponent<GameData>();
        }

        GameData.Instance.selectedCharacter = characters[selectedIndex];
        GameData.Instance.selectedEnemy = defaultEnemy;

        SceneManager.LoadScene(nextSceneName);
    }
}
