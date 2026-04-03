using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class BattleEntity
{
    public int maxHP = 100;
    public int currentHP = 100;

    public int maxMana = 20;
    public int mana;

    public int shield;

    // Speed: đủ 5 → được thêm 1 lượt
    public int speed;
    public int speedMax = 5;
    public bool extraTurnReady;

    // Ult: đủ 10 → được dùng Ultimate
    public int ultCharge;
    public int ultChargeMax = 10;
    public bool ultimateReady;

    // ========== STATUS EFFECTS ==========
    public List<StatusEffect> statusEffects = new();

    /// <summary>
    /// Hệ số tấn công hiện tại (bị ảnh hưởng bởi AttackBuff)
    /// </summary>
    public float attackMultiplier
    {
        get
        {
            float mult = 1f;
            foreach (var s in statusEffects)
            {
                if (s.type == StatusEffect.Type.AttackBuff)
                    mult *= s.multiplier;
            }
            return mult;
        }
    }

    /// <summary>
    /// Đang bị đóng băng?
    /// </summary>
    public bool isFrozen
    {
        get
        {
            foreach (var s in statusEffects)
            {
                if (s.type == StatusEffect.Type.Freeze && s.remainingTurns > 0)
                    return true;
            }
            return false;
        }
    }

    /// <summary>
    /// Đang bị cháy?
    /// </summary>
    public bool isBurning
    {
        get
        {
            foreach (var s in statusEffects)
            {
                if (s.type == StatusEffect.Type.Burn && s.remainingTurns > 0)
                    return true;
            }
            return false;
        }
    }

    /// <summary>
    /// Đang bị độc?
    /// </summary>
    public bool isPoisoned
    {
        get
        {
            foreach (var s in statusEffects)
            {
                if (s.type == StatusEffect.Type.Poison && s.remainingTurns > 0)
                    return true;
            }
            return false;
        }
    }

    /// <summary>
    /// Có buff attack?
    /// </summary>
    public bool hasAttackBuff
    {
        get
        {
            foreach (var s in statusEffects)
            {
                if (s.type == StatusEffect.Type.AttackBuff && s.remainingTurns > 0)
                    return true;
            }
            return false;
        }
    }

    // ========== CORE ==========

    // Lưu lại phần shield absorbed lần cuối để popup hiển thị
    [System.NonSerialized] public int lastShieldAbsorbed;

    public void TakeDamage(int dmg)
    {
        int shieldBefore = shield;

        int absorbed = Mathf.Min(shield, dmg);
        shield -= absorbed;
        lastShieldAbsorbed = absorbed;

        int remaining = dmg - absorbed;
        currentHP -= remaining;

        if (currentHP < 0) currentHP = 0;

        Debug.Log($"[TakeDamage] dmg={dmg} shield={shieldBefore}→{shield} (absorbed {absorbed}) HP={currentHP + remaining}→{currentHP}");
    }

    public void Heal(int value)
    {
        currentHP = Mathf.Min(currentHP + value, maxHP);
    }

    public void AddMana(int value)
    {
        mana = Mathf.Min(mana + value, maxMana);
    }

    public void AddSpeed(int value)
    {
        speed += value;

        if (speed >= speedMax)
        {
            speed -= speedMax;  // trừ đi, phần dư giữ lại
            extraTurnReady = true;
        }
    }

    public void AddUltCharge(int value)
    {
        ultCharge = Mathf.Min(ultCharge + value, ultChargeMax);

        if (ultCharge >= ultChargeMax)
        {
            ultimateReady = true;
        }
    }

    public bool IsDead => currentHP <= 0;

    // ========== STATUS MANAGEMENT ==========

    /// <summary>
    /// Áp dụng status effect mới
    /// </summary>
    public void ApplyStatus(StatusEffect.Type type, int damage, int turns, float multiplier = 1f)
    {
        // cùng loại → refresh (không stack)
        for (int i = 0; i < statusEffects.Count; i++)
        {
            if (statusEffects[i].type == type)
            {
                statusEffects[i] = new StatusEffect(type, damage, turns, multiplier);
                return;
            }
        }

        statusEffects.Add(new StatusEffect(type, damage, turns, multiplier));
    }

    /// <summary>
    /// Xử lý tất cả status đầu lượt.
    /// Trả về tổng damage nhận và có bị skip turn không.
    /// </summary>
    public StatusTickResult TickAllStatus()
    {
        var result = new StatusTickResult();

        for (int i = statusEffects.Count - 1; i >= 0; i--)
        {
            var s = statusEffects[i];

            // check freeze trước khi tick
            if (s.SkipsTurn)
                result.skipTurn = true;

            // tick damage
            int dmg = s.Tick();
            if (dmg > 0)
            {
                TakeDamage(dmg);
                result.totalDamage += dmg;

                // ghi nhận loại damage
                result.damageTypes.Add(s.type);
            }

            // xoá nếu hết hạn
            if (s.IsExpired)
                statusEffects.RemoveAt(i);
        }

        return result;
    }

    /// <summary>
    /// Xoá tất cả status (khi bắt đầu trận mới)
    /// </summary>
    public void ClearAllStatus()
    {
        statusEffects.Clear();
    }

    /// <summary>
    /// Xoá 1 loại status cụ thể
    /// </summary>
    public void RemoveStatus(StatusEffect.Type type)
    {
        statusEffects.RemoveAll(s => s.type == type);
    }
}

/// <summary>
/// Kết quả xử lý status mỗi lượt
/// </summary>
public class StatusTickResult
{
    public int totalDamage;
    public bool skipTurn;
    public List<StatusEffect.Type> damageTypes = new();
}