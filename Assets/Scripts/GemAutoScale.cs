using UnityEngine;

/// <summary>
/// Gắn vào mỗi Gem prefab. Tự động scale sprite về đúng kích thước mong muốn.
/// Chạy trong Awake() nên gem sẽ đúng size ngay khi spawn.
/// </summary>
[RequireComponent(typeof(SpriteRenderer))]
public class GemAutoScale : MonoBehaviour
{
    /// <summary>
    /// Kích thước mong muốn (world units). 0.85 = vừa 1 ô board với padding.
    /// </summary>
    public static float targetSize = 0.85f;

    void Awake()
    {
        FitToSize();
    }

    public void FitToSize()
    {
        SpriteRenderer sr = GetComponent<SpriteRenderer>();
        if (sr == null || sr.sprite == null) return;

        // lấy size thật của sprite (pixels / pixelsPerUnit)
        Vector2 spriteSize = sr.sprite.bounds.size;

        // tìm dimension lớn nhất
        float maxDim = Mathf.Max(spriteSize.x, spriteSize.y);

        if (maxDim <= 0f) return;

        // tính scale cần thiết
        float scale = targetSize / maxDim;

        transform.localScale = new Vector3(scale, scale, 1f);
    }
}
