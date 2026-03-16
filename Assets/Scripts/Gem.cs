using UnityEngine;

public enum GemType
{
    Sword = 0,       // damage thường
    HeavySword = 1,  // damage mạnh
    Horse = 2,       // tốc lực (speed bar)
    Shield = 3,      // phòng thủ (buff defense)
    Tear = 4,        // mana (energy cho ultimate)
    Heart = 5,       // hồi HP
    Diamond = 6      // tiền (gold)
}

public class Gem : MonoBehaviour
{
    public int x;
    public int y;
    public int typeId;
    public GemType gemType;

    public void Init(int xPos, int yPos, int type)
    {
        x = xPos;
        y = yPos;
        typeId = type;
        gemType = (GemType)type;
    }

    public void SetPosition(int newX, int newY)
    {
        x = newX;
        y = newY;
    }
}