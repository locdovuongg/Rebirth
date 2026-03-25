using UnityEngine;

public enum GemType
{
    Sword,
    HeavySword,
    Horse,
    Shield,
    Tear,
    Heart,
    Diamond,
    Fire
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
    }

    public void SetPosition(int newX, int newY)
    {
        x = newX;
        y = newY;
    }
}