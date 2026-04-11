using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;

/// <summary>
/// Màn hình chọn nhân vật.
/// Gắn vào Canvas ở scene "CharacterSelect".
/// </summary>
public class CharacterSelectUI : MonoBehaviour
{
    [Header("Danh sách nhân vật")]
    public CharacterInfo[] characters;

    [Header("UI hiển thị nhân vật đang chọn")]
    public Image portraitImage;
    public TextMeshProUGUI nameText;
    public TextMeshProUGUI statsText;

    [Header("Container chứa các nút chọn")]
    public Transform buttonContainer;

    [Header("Prefab nút chọn nhân vật")]
    public GameObject characterButtonPrefab;

    [Header("Battle Scene name")]
    public string battleSceneName = "BattleScene";

    [Header("Enemy (mặc định)")]
    public CharacterInfo defaultEnemy;

    int selectedIndex = 0;

    void Start()
    {
        // Tạo nút cho mỗi nhân vật
        for (int i = 0; i < characters.Length; i++)
        {
            CreateButton(i);
        }

        // Chọn nhân vật đầu tiên
        if (characters.Length > 0)
            SelectCharacter(0);
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

            btnObj.AddComponent<Button>();

            var rt = btnObj.GetComponent<RectTransform>();
            rt.sizeDelta = new Vector2(150, 180);
        }

        // Set avatar
        var avatarImg = btnObj.transform.Find("Avatar");
        if (avatarImg != null)
        {
            var img = avatarImg.GetComponent<Image>();
            if (img != null && c.avatar != null)
                img.sprite = c.avatar;
        }
        else if (c.avatar != null)
        {
            // Tạo avatar image con
            var avatarObj = new GameObject("Avatar");
            avatarObj.transform.SetParent(btnObj.transform, false);
            var aImg = avatarObj.AddComponent<Image>();
            aImg.sprite = c.avatar;
            aImg.preserveAspect = true;
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
            if (tmp != null) tmp.text = c.characterName;
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
            var nRT = nObj.GetComponent<RectTransform>();
            nRT.anchorMin = new Vector2(0, 0);
            nRT.anchorMax = new Vector2(1, 0.25f);
            nRT.offsetMin = Vector2.zero;
            nRT.offsetMax = Vector2.zero;
        }

        // Click event
        int idx = index;
        var btn = btnObj.GetComponent<Button>();
        btn.onClick.AddListener(() => SelectCharacter(idx));
    }

    public void SelectCharacter(int index)
    {
        selectedIndex = index;
        CharacterInfo c = characters[index];

        // Cập nhật preview
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
    }

    /// <summary>
    /// Gắn vào nút "START BATTLE"
    /// </summary>
    public void ConfirmAndStart()
    {
        // Đảm bảo GameData tồn tại
        if (GameData.Instance == null)
        {
            var go = new GameObject("GameData");
            go.AddComponent<GameData>();
        }

        GameData.Instance.selectedCharacter = characters[selectedIndex];
        GameData.Instance.selectedEnemy = defaultEnemy;

        SceneManager.LoadScene(battleSceneName);
    }
}
