using UnityEngine;

public enum GemType
{
    HeavySword = 0,
    Diamond = 1,
    Heart = 2,
    Tear = 3,
    Shield = 4,
    Horse = 5,
    Sword = 6,
    Fire = 7
}

public class Gem : MonoBehaviour
{
    public int x;
    public int y;
    public GemType gemType;

    public void Init(int xPos, int yPos, GemType type)
    {
        x = xPos;
        y = yPos;
        gemType = type;
        gameObject.name = $"{gemType}_{x}_{y}";
    }

    public void SetPosition(int newX, int newY)
    {
        x = newX;
        y = newY;
    }
}