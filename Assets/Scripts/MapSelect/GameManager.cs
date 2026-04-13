using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;

    public ChapterData currentChapter;
    public int currentLevelIndex;

    const string UNLOCK_KEY = "level_unlock";

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    /// <summary>
    /// Level cao nhất đã mở (0-based). Level 0 luôn mở.
    /// </summary>
    public int UnlockedLevel
    {
        get => PlayerPrefs.GetInt(UNLOCK_KEY, 0);
        set
        {
            PlayerPrefs.SetInt(UNLOCK_KEY, value);
            PlayerPrefs.Save();
        }
    }

    /// <summary>
    /// Gọi từ MapNode khi click
    /// </summary>
    public void StartLevel(ChapterData chapter, int levelIndex)
    {
        currentChapter = chapter;
        currentLevelIndex = levelIndex;
        SceneManager.LoadScene("BattleScene");
    }

    /// <summary>
    /// Gọi từ BattleManager khi player thắng
    /// </summary>
    public void OnBattleWon()
    {
        // Mở level tiếp theo nếu đây là level cao nhất đang chơi
        if (currentLevelIndex >= UnlockedLevel)
        {
            int nextLevel = currentLevelIndex + 1;

            // Chỉ unlock nếu chapter còn level tiếp
            if (currentChapter != null && nextLevel < currentChapter.enemies.Length)
            {
                UnlockedLevel = nextLevel;
                Debug.Log($"[GameManager] Unlocked level {nextLevel}!");
            }
        }
    }

    /// <summary>
    /// Quay về map select
    /// </summary>
    public void ReturnToMap()
    {
        SceneManager.LoadScene("MapSelect");
    }
}