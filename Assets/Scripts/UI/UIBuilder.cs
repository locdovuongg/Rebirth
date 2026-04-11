// FILE NÀY KHÔNG CÒN DÙNG — Đã thay bằng BattleUI.cs
// Bạn có thể xoá file này trong Unity Editor
// (Right click > Delete trong Project window)
//
// UIBuilder đã được thay bằng BattleUI.cs
// BattleUI chỉ cập nhật data, bạn tự layout UI trong Editor.

using UnityEngine;

public class UIBuilder : MonoBehaviour
{
    void Awake()
    {
        Debug.LogWarning("[UIBuilder] File này không còn dùng. Hãy xoá và dùng BattleUI thay thế.");
        Destroy(gameObject);
    }
}