using UnityEngine;

/// <summary>
/// Dữ liệu tồn tại giữa các scene (DontDestroyOnLoad).
/// Lưu thông tin nhân vật đã chọn.
/// </summary>
public class GameData : MonoBehaviour
{
    public static GameData Instance { get; private set; }

    [Header("Player đã chọn")]
    public CharacterInfo selectedCharacter;

    [Header("Enemy (set theo level/random)")]
    public CharacterInfo selectedEnemy;

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);
    }
}
