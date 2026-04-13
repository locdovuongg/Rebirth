using UnityEngine;

[CreateAssetMenu(menuName = "Game/Enemy")]
public class EnemyData : ScriptableObject
{
    [Header("Hiển thị")]
    public string enemyName;
    public Sprite avatar;

    [Header("Stats")]
    public int maxHP = 100;
    public int maxMana = 50;
    public int speedMax = 5;
    public int ultChargeMax = 10;

    [Header("Damage")]
    public int swordDamage = 10;
    public int ultimateDamage = 40;

    [Header("Boss?")]
    public bool isBoss;
}