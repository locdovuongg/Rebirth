using UnityEngine;

/// <summary>
/// Thông tin 1 nhân vật. Tạo bằng: Right Click → Create → Rebirth → Character.
/// </summary>
[CreateAssetMenu(fileName = "NewCharacter", menuName = "Rebirth/Character")]
public class CharacterInfo : ScriptableObject
{
    [Header("Hiển thị")]
    public string characterName = "Hero";
    public Sprite avatar;           // ảnh avatar hiện trên UI
    public Sprite portrait;         // ảnh lớn (dùng ở màn chọn nhân vật)

    [Header("Stats cơ bản")]
    public int maxHP = 100;
    public int maxMana = 20;
    public int speedMax = 5;
    public int ultChargeMax = 10;

    [Header("Damage")]
    public int swordDamage = 10;
    public int ultimateDamage = 40;

    [Header("Skills (kéo vào)")]
    public Skill[] skills;
}
