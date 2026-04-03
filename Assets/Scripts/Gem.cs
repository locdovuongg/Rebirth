using UnityEngine;

public enum GemType
{
    BuffDamage,  // 0 - Gem_BuffDamage  (tăng sát thương tạm thời)
    Diamond,     // 1 - Gem_Diamond     (kim cương – currency, nhận cuối trận)
    Heart,       // 2 - Gem_HP          (hồi máu)
    Tear,        // 3 - Gem_Mana        (hồi mana)
    Shield,      // 4 - Gem_Shield      (cộng shield)
    Horse,       // 5 - Gem_Speed       (cộng speed)
    Sword,       // 6 - Gem_Sword       (damage thường)
    Fire         // 7 - Gem_Ult         (tăng điểm ult)
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