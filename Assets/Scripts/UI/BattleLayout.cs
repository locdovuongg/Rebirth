using UnityEngine;
public class BattleLayout : MonoBehaviour
{
    [Header("Panels")]
    public RectTransform boardPanel;        // chứa board (phía trên, chiếm ~70% màn hình)
    public RectTransform bottomPanel;       // chứa 2 nhân vật + bar (phía dưới, ~30%)
}