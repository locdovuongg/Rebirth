using UnityEngine;

public class OrientationManager : MonoBehaviour
{
    public CameraFit cameraFit;
    public bool lockPortrait = true;

    int lastWidth, lastHeight;

    void Start()
    {
        if (lockPortrait)
        {
            Screen.orientation = ScreenOrientation.Portrait;
            Screen.autorotateToPortrait = true;
            Screen.autorotateToPortraitUpsideDown = true;
            Screen.autorotateToLandscapeLeft = false;
            Screen.autorotateToLandscapeRight = false;
        }

        lastWidth = Screen.width;
        lastHeight = Screen.height;

        if (cameraFit == null)
            cameraFit = FindFirstObjectByType<CameraFit>();
    }

    void Update()
    {
        if (Screen.width != lastWidth || Screen.height != lastHeight)
        {
            lastWidth = Screen.width;
            lastHeight = Screen.height;

            if (cameraFit != null)
                cameraFit.Fit();
        }
    }
}