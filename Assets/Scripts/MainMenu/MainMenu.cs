using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenu : MonoBehaviour
{
    [Header("Scene Settings")]
    public string gameSceneName = "CharacterSelectScene";     // Scene khi bấm Play
    public string optionSceneName = "Options"; // Scene Option (nếu có)
    public SceneFader fader;
    // PLAY GAME


    public void PlayGame()
{
    fader.FadeToScene(gameSceneName);
}
    // CONTINUE GAME
    public void ContinueGame()
    {
        Debug.Log("Continue Game");

        if (PlayerPrefs.HasKey("SaveScene"))
        {
            string savedScene = PlayerPrefs.GetString("SaveScene");
            SceneManager.LoadScene(savedScene);
        }
        else
        {
            Debug.Log("No save data found!");
        }
    }

    // OPTIONS
    public void OpenOptions()
    {
        Debug.Log("Open Options");

        // Nếu bạn có scene riêng cho Options
        SceneManager.LoadScene(optionSceneName);

        // Hoặc nếu dùng UI panel:
        // optionsPanel.SetActive(true);
    }

    // EXIT GAME
    public void QuitGame()
    {
        Debug.Log("Quit Game");

        Application.Quit();

        // Nếu chạy trong Editor
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#endif
    }
}