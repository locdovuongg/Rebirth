using UnityEngine;

[RequireComponent(typeof(Camera))]
public class CameraFit : MonoBehaviour
{
    [Header("Board")]
    public int boardWidth = 8;
    public int boardHeight = 8;
    public float padding = 0.3f;

    Camera cam;
    int lastW, lastH;

    void Awake()
    {
        cam = GetComponent<Camera>();
        cam.orthographic = true;
    }

    void Start()
    {
        Fit();
    }

    void Update()
    {
        if (Screen.width != lastW || Screen.height != lastH)
        {
            Fit();
        }
    }

    public void Fit()
    {
        lastW = Screen.width;
        lastH = Screen.height;

        float totalW = boardWidth + padding * 2f;
        float totalH = boardHeight + padding * 2f;

        float screenAspect = (float)Screen.width / Screen.height;

        float sizeH = totalH / 2f;
        float sizeW = totalW / (2f * screenAspect);

        cam.orthographicSize = Mathf.Max(sizeH, sizeW);
        cam.orthographicSize = Mathf.Max(sizeH, sizeW);

        // camera nhìn vào giữa board        transform.position = new Vector3(0f, 0f, -10f);
        transform.position = new Vector3(0f, 0f, -10f);
    }}