using UnityEngine;

public class CharacterPanel : MonoBehaviour
{
    void Awake()
    {
        Debug.LogWarning("[CharacterPanel] Đã thay bằng BattleUI. Hãy xoá component này.");
        Destroy(this);
    }
}