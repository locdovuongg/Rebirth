using System.Collections.Generic;
using UnityEngine;

public class SkillUser : MonoBehaviour
{
    public List<Skill> skills = new();

    /// <summary>
    /// Gọi khi bắt đầu trận — reset cooldown
    /// </summary>
    public void ResetAllCooldowns()
    {
        foreach (var skill in skills)
            skill.ResetCooldown();
    }

    /// <summary>
    /// Gọi mỗi đầu lượt — giảm cooldown 1
    /// </summary>
    public void TickAllCooldowns()
    {
        foreach (var skill in skills)
            skill.TickCooldown();
    }
}