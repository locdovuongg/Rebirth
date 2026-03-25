using UnityEngine;

[CreateAssetMenu(menuName = "Rebirth/Skill")]
public class Skill : ScriptableObject
{
    public string skillName;
    [TextArea] public string description;
    public Sprite icon;

    public int manaCost;
    public int cooldown;
    public bool isUltimate;

    public enum SkillType
    {
        Damage,
        Burn,
        Freeze,
        Poison,
        Heal,
        Shield,
        AttackBuff,
        SpeedBuff,
        DamageAndHeal
    }

    public SkillType type;
    public int value;
    public int duration = 2;
    public float buffMultiplier = 1.5f;

    [HideInInspector] public int currentCooldown;

    public bool CanUse(BattleEntity caster)
    {
        if (caster.mana < manaCost) return false;
        if (currentCooldown > 0) return false;
        if (isUltimate && !caster.ultimateReady) return false;
        return true;
    }

    public bool Execute(BattleEntity caster, BattleEntity target)
    {
        if (!CanUse(caster)) return false;

        caster.mana -= manaCost;

        if (isUltimate)
        {
            caster.ultimateReady = false;
            caster.speed = 0;
        }

        currentCooldown = cooldown;

        switch (type)
        {
            case SkillType.Damage:
                int dmg = Mathf.RoundToInt(value * caster.attackMultiplier);
                target.TakeDamage(dmg);
                break;

            case SkillType.Burn:
                target.ApplyStatus(StatusEffect.Type.Burn, value, duration);
                break;

            case SkillType.Freeze:
                target.ApplyStatus(StatusEffect.Type.Freeze, value, duration);
                break;

            case SkillType.Poison:
                target.ApplyStatus(StatusEffect.Type.Poison, value, duration);
                break;

            case SkillType.Heal:
                caster.Heal(value);
                break;

            case SkillType.Shield:
                caster.shield += value;
                break;

            case SkillType.AttackBuff:
                caster.ApplyStatus(StatusEffect.Type.AttackBuff, 0, duration, buffMultiplier);
                break;

            case SkillType.SpeedBuff:
                caster.AddSpeed(value);
                break;

            case SkillType.DamageAndHeal:
                int lifestealDmg = Mathf.RoundToInt(value * caster.attackMultiplier);
                target.TakeDamage(lifestealDmg);
                caster.Heal(lifestealDmg / 2);
                break;
        }

        return true;
    }

    public void TickCooldown()
    {
        if (currentCooldown > 0)
            currentCooldown--;
    }

    public void ResetCooldown()
    {
        currentCooldown = 0;
    }

    public bool TargetsSelf()
    {
        return type == SkillType.Heal
            || type == SkillType.Shield
            || type == SkillType.AttackBuff
            || type == SkillType.SpeedBuff;
    }
}