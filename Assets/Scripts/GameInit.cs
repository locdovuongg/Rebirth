using UnityEngine;

public class GameInit : MonoBehaviour
{
    void Awake()
    {
        Application.targetFrameRate = 60;
        Screen.sleepTimeout = SleepTimeout.NeverSleep;

        // tránh multi-touch gây bug
        Input.multiTouchEnabled = false;
    }
}