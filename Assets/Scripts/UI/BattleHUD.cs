using UnityEngine;

public class BattleHUD : MonoBehaviour
{
    void Awake()
    {
        Debug.LogWarning("[BattleHUD] Đã thay bằng BattleUI. Hãy xoá component này.");
        Destroy(this);
    }
}