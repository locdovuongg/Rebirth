using System.Collections.Generic;
using UnityEngine;

public class BattleManager : MonoBehaviour
{
    public static BattleManager Instance;

    [Header("Player Stats")]
    public int playerHP = 100;
    public int playerMaxHP = 100;
    public int playerMana = 0;
    public int playerShield = 0;

    [Header("Enemy Stats")]
    public int enemyHP = 120;
    public int enemyMaxHP = 120;

    [Header("Speed Bar")]
    public int speedBar = 0;
    public int speedMax = 5;

    [Header("Damage")]
    public int swordDamage = 10;
    public int heavySwordDamage = 15;

    bool playerTurn = true;

    void Awake()
    {
        Instance = this;
    }

    // ================= MATCH PROCESS =================

    public void ProcessMatches(HashSet<Gem> matches)
    {
        Dictionary<int, int> counter = new Dictionary<int, int>();

        foreach (var gem in matches)
        {
            if (!counter.ContainsKey(gem.typeId))
                counter[gem.typeId] = 0;

            counter[gem.typeId]++;
        }

        foreach (var pair in counter)
        {
            int type = pair.Key;
            int count = pair.Value;

            ApplyGemEffect(type, count);
        }
    }

    // ================= EFFECT =================

    void ApplyGemEffect(int type, int count)
    {
        switch (type)
        {
            // Sword
            case 0:
                DamageEnemy(count * swordDamage);
                Debug.Log("Sword damage: " + (count * swordDamage));
                break;

            // Heavy Sword
            case 1:
                DamageEnemy(count * heavySwordDamage);
                Debug.Log("Heavy sword damage: " + (count * heavySwordDamage));
                break;

            // Horse
            case 2:
                int speedGain = GetHorseSpeed(count);
                AddSpeed(speedGain);
                Debug.Log("Speed +" + speedGain);
                break;

            // Shield
            case 3:
                playerShield += count * 3;
                Debug.Log("Shield +" + (count * 3));
                break;

            // Tear (mana)
            case 4:
                playerMana += count * 2;
                Debug.Log("Mana +" + (count * 2));
                break;

            // Heart
            case 5:
                HealPlayer(count * 5);
                Debug.Log("Heal +" + (count * 5));
                break;

            // Diamond
            case 6:
                Debug.Log("Diamond reward +" + count);
                break;
        }
    }

    int GetHorseSpeed(int count)
    {
        if (count == 3) return 1;
        if (count == 4) return 3;
        if (count >= 5) return 5;

        return 0;
    }

    // ================= COMBAT =================

    void DamageEnemy(int dmg)
    {
        enemyHP -= dmg;

        if (enemyHP <= 0)
        {
            enemyHP = 0;
            Debug.Log("Enemy defeated!");
        }
    }

    void HealPlayer(int value)
    {
        playerHP += value;
        playerHP = Mathf.Min(playerHP, playerMaxHP);
    }

    public void DamagePlayer(int dmg)
    {
        int finalDamage = Mathf.Max(0, dmg - playerShield);

        playerShield = Mathf.Max(0, playerShield - dmg);

        playerHP -= finalDamage;

        if (playerHP <= 0)
        {
            playerHP = 0;
            Debug.Log("Player defeated!");
        }
    }

    // ================= SPEED =================

    void AddSpeed(int value)
    {
        speedBar += value;
        speedBar = Mathf.Clamp(speedBar, 0, speedMax);

        if (speedBar >= speedMax)
        {
            Debug.Log("Ultimate ready!");
        }
    }

    // ================= TURN =================

    public void EndTurn()
    {
        if (!playerTurn) return;

        playerTurn = false;

        StartCoroutine(EnemyTurn());
    }

    System.Collections.IEnumerator EnemyTurn()
{
    yield return new WaitForSeconds(1f);

    EnemyAI ai = FindFirstObjectByType<EnemyAI>();

    if (ai != null)
        yield return ai.DoEnemyMove();

    playerTurn = true;
}}