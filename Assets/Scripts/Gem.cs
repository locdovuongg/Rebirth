using UnityEngine;

public class Gem : MonoBehaviour
{
    public int x;
    public int y;
    public int typeId;

    public void Init(int xPos, int yPos, int type)
    {
        x = xPos;
        y = yPos;
        typeId = type;
    }

    public void SetPosition(int newX, int newY)
    {
        x = newX;
        y = newY;
    }

    // This method was made public to be accessible from outside the class
    public void TryMove(int deltaX, int deltaY)
    {
        // Logic for moving the gem
        x += deltaX;
        y += deltaY;
    }
}
