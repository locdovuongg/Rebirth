using UnityEngine;

[System.Serializable]
public class StatusEffect
{
    public enum Type
    {
        Burn,           // damage mỗi lượt
        Freeze,         // bỏ lượt + damage nhẹ
        Poison,         // damage mỗi lượt, giảm dần
        AttackBuff,     // tăng dame
    }

    public Type type;
    public int damagePerTurn;
    public int remainingTurns;
    public float multiplier;    // dùng cho AttackBuff

    public StatusEffect(Type type, int damage, int turns, float multiplier = 1f)
    {
        this.type = type;
        this.damagePerTurn = damage;
        this.remainingTurns = turns;
        this.multiplier = multiplier;
    }

    /// <summary>
    /// Xử lý hiệu ứng đầu lượt. Trả về damage gây ra (0 nếu không có).
    /// </summary>
    public int Tick()
    {
        int dmg = 0;

        switch (type)
        {
            case Type.Burn:
                dmg = damagePerTurn;
                break;

            case Type.Freeze:
                dmg = damagePerTurn / 2; // damage nhẹ
                break;

            case Type.Poison:
                dmg = damagePerTurn;
                damagePerTurn = Mathf.Max(1, damagePerTurn - 1); // giảm dần
                break;

            case Type.AttackBuff:
                dmg = 0; // không gây damage
                break;
        }

        remainingTurns--;
        return dmg;
    }

    public bool IsExpired => remainingTurns <= 0;

    /// <summary>
    /// Có bỏ lượt không?
    /// </summary>
    public bool SkipsTurn => type == Type.Freeze && remainingTurns > 0;

    public Color GetColor()
    {
        return type switch
        {
            Type.Burn => new Color(1f, 0.4f, 0.1f),       // cam đỏ
            Type.Freeze => new Color(0.3f, 0.7f, 1f),     // xanh dương nhạt
            Type.Poison => new Color(0.4f, 0.9f, 0.2f),   // xanh lá
            Type.AttackBuff => new Color(1f, 0.8f, 0.2f),  // vàng
            _ => Color.white
        };
    }

    public string GetIcon()
    {
        return type switch
        {
            Type.Burn => "BRN",
            Type.Freeze => "FRZ",
            Type.Poison => "PSN",
            Type.AttackBuff => "ATK",
            _ => "?"
        };
    }
}