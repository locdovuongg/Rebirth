using UnityEngine;

/// <summary>
/// Camera cố định tại (0,0,-10).  
/// Tính orthoSize vừa board, rồi dịch BoardManager lên  
/// để board nằm giữa vùng trống (giữa BottomPanel và TopDecor).
/// </summary>
[RequireComponent(typeof(Camera))]
public class CameraFit : MonoBehaviour
{
    [Header("Board")]
    public int boardWidth = 8;
    public int boardHeight = 8;
    public float padding = 0.5f;

    [Header("UI tỉ lệ (screen %)")]
    [Tooltip("BottomPanel chiếm % dưới màn hình")]
    public float bottomUIRatio = 0.286f;
    [Tooltip("TopDecor chiếm % trên màn hình")]
    public float topUIRatio = 0.063f;

    [Header("Ref (tự tìm nếu trống)")]
    public Transform boardRoot;

    Camera cam;
    int lastW, lastH;

    void Awake()
    {
        cam = GetComponent<Camera>();
        cam.orthographic = true;
    }

    void Start()
    {
        if (boardRoot == null)
        {
            var bm = FindFirstObjectByType<BoardManager>();
            if (bm != null) boardRoot = bm.transform;
        }
        Fit();
    }

    void Update()
    {
        if (Screen.width != lastW || Screen.height != lastH)
            Fit();
    }

    public void Fit()
    {
        lastW = Screen.width;
        lastH = Screen.height;

        float screenAspect = (float)Screen.width / Screen.height;

        // --- Vùng visible giữa 2 UI panel (tỉ lệ % màn hình) ---
        float visibleRatio = 1f - bottomUIRatio - topUIRatio; // ≈ 0.651

        float totalW = boardWidth + padding * 2f;  // 9
        float totalH = boardHeight + padding * 2f; // 9

        // Ortho size phải đủ để board FIT trong vùng visible
        // Vùng visible chiều cao world = visibleRatio * orthoSize * 2
        // Cần: visibleRatio * orthoSize * 2 >= totalH → orthoSize >= totalH / (2 * visibleRatio)
        float sizeForHeight = totalH / (2f * visibleRatio);
        float sizeForWidth = totalW / (2f * screenAspect);

        cam.orthographicSize = Mathf.Max(sizeForHeight, sizeForWidth);

        // Camera cố định tại center
        transform.position = new Vector3(0f, 0f, -10f);

        // --- Dịch board lên để nằm giữa vùng visible ---
        // Tâm vùng visible (tính từ dưới) = bottomUIRatio + visibleRatio / 2
        // Screen center = 0.5
        // Offset (screen %) = (bottomUIRatio + visibleRatio/2) - 0.5
        //                   = (bottomUIRatio - topUIRatio) / 2
        float screenOffset = (bottomUIRatio - topUIRatio) / 2f; // ≈ 0.1115
        float worldOffset = screenOffset * cam.orthographicSize * 2f;

        if (boardRoot != null)
        {
            boardRoot.position = new Vector3(0f, worldOffset, 0f);
        }
    }
}