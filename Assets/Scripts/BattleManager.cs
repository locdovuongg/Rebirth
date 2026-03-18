using System.Collections.Generic;
using UnityEngine;
using System.Collections;

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
    public int enemyShield = 0;

    [Header("Speed Bar")]
    public int speedBar = 0;
    public int speedMax = 5;

    [Header("Ultimate Bar")]
    public int ultBar = 0;
    public int ultMax = 10;

    [Header("Damage")]
    public int swordDamage = 10;
    public int heavySwordDamage = 15;
    public int ultimateDamage = 40;

    [Header("Damage Popups")]
    public GameObject damagePopupPrefab;
    public Transform popupSpawnPlayer;
    public Transform popupSpawnEnemy;

    public bool playerTurn = true;
    public bool ultimateReady;
    bool extraTurn;
    bool isBattleOver;

    void Awake()
    {
        Instance = this;
    }

    // ================= MATCH PROCESS =================

    public void ProcessMatches(HashSet<Gem> matches, int combo = 1)
    {
        if (isBattleOver) return;

        Dictionary<GemType, int> counter = new Dictionary<GemType, int>();

        foreach (var gem in matches)
        {
            if (gem == null) continue;

            if (!counter.ContainsKey(gem.gemType))
                counter[gem.gemType] = 0;

            counter[gem.gemType]++;
        }

        foreach (var pair in counter)
        {
            ApplyGemEffect(pair.Key, pair.Value);
        }

        Debug.Log($"[{WhoIsAttacking()}] matched {matches.Count} gems");
    }

    // ================= EFFECT =================

    void ApplyGemEffect(GemType type, int count)
    {
        switch (type)
        {
            case GemType.HeavySword:
                DealDamage(count * heavySwordDamage);
                Debug.Log($"🗡️ HeavySword damage: {count * heavySwordDamage} (by {WhoIsAttacking()})");
                break;

            case GemType.Diamond:
                Debug.Log($"💎 Diamond reward +{count}");
                break;

            case GemType.Heart:
                HealSelf(count * 5);
                Debug.Log($"❤️ Heal +{count * 5} (by {WhoIsAttacking()})");
                break;

            case GemType.Tear:
                AddMana(count * 2);
                Debug.Log($"💧 Mana +{count * 2} (by {WhoIsAttacking()})");
                break;

            case GemType.Shield:
                AddShield(count * 3);
                Debug.Log($"🛡️ Shield +{count * 3} (by {WhoIsAttacking()})");
                break;

            case GemType.Horse:
                AddSpeed(count);
                Debug.Log($"🐴 Speed +{count} (bar: {speedBar}/{speedMax}) (by {WhoIsAttacking()})");
                break;

            case GemType.Sword:
                DealDamage(count * swordDamage);
                Debug.Log($"⚔️ Sword damage: {count * swordDamage} (by {WhoIsAttacking()})");
                break;

            case GemType.Fire:
                if (playerTurn)
                {
                    AddUltimate(count);
                    Debug.Log($"🔥 Player ultimate charge +{count} (bar: {ultBar}/{ultMax})");
                }
                else
                {
                    EnemyAI ai = FindFirstObjectByType<EnemyAI>();
                    if (ai != null)
                    {
                        ai.AddUltCharge(count);
                        Debug.Log($"🔥 Enemy ultimate charge +{count} (bar: {ai.enemyUltBar}/{ai.enemyUltMax})");
                    }
                }
                break;
        }
    }

    string WhoIsAttacking() => playerTurn ? "Player" : "Enemy";

    // ================= COMBAT =================

    void DealDamage(int dmg)
    {
        if (playerTurn)
        {
            int finalDmg = Mathf.Max(0, dmg - enemyShield);
            enemyShield = Mathf.Max(0, enemyShield - dmg);
            enemyHP -= finalDmg;
            enemyHP = Mathf.Max(0, enemyHP);

            SpawnPopup("-" + finalDmg, Color.red, popupSpawnEnemy);

            if (enemyHP <= 0)
            {
                isBattleOver = true;
                Debug.Log("🏆 Enemy defeated!");
            }
        }
        else
        {
            int finalDmg = Mathf.Max(0, dmg - playerShield);
            playerShield = Mathf.Max(0, playerShield - dmg);
            playerHP -= finalDmg;
            playerHP = Mathf.Max(0, playerHP);

            SpawnPopup("-" + finalDmg, Color.red, popupSpawnPlayer);

            if (playerHP <= 0)
            {
                isBattleOver = true;
                Debug.Log("💀 Player defeated!");
            }
        }
    }

    void HealSelf(int value)
    {
        if (playerTurn)
        {
            playerHP = Mathf.Min(playerHP + value, playerMaxHP);
            SpawnPopup("+" + value, Color.green, popupSpawnPlayer);
        }
        else
        {
            enemyHP = Mathf.Min(enemyHP + value, enemyMaxHP);
            SpawnPopup("+" + value, Color.green, popupSpawnEnemy);
        }
    }

    void AddShield(int value)
    {
        if (playerTurn)
            playerShield += value;
        else
            enemyShield += value;
    }

    void AddMana(int value)
    {
        if (playerTurn)
            playerMana += value;
    }

    // ================= SPEED (Horse) =================

    void AddSpeed(int value)
    {
        if (!playerTurn) return;

        speedBar += value;

        while (speedBar >= speedMax)
        {
            speedBar -= speedMax;
            extraTurn = true;
            Debug.Log("⚡ EXTRA TURN earned!");
        }
    }

    // ================= ULTIMATE BAR (Fire) =================

    void AddUltimate(int value)
    {
        ultBar += value;

        if (ultBar >= ultMax)
        {
            ultBar = ultMax;
            ultimateReady = true;
            Debug.Log("🔥 ULTIMATE READY!");
        }

        SpawnPopup("🔥+" + value, Color.yellow, popupSpawnPlayer);
    }

    // ================= SKILLS =================

    public void UseUltimate()
    {
        if (!ultimateReady) return;

        ultimateReady = false;
        ultBar = 0;

        enemyHP -= ultimateDamage;
        enemyHP = Mathf.Max(0, enemyHP);

        SpawnPopup("-" + ultimateDamage, Color.magenta, popupSpawnEnemy);

        Debug.Log($"💥 ULTIMATE: {ultimateDamage} damage!");

        if (enemyHP <= 0)
        {
            isBattleOver = true;
            Debug.Log("🏆 Enemy defeated!");
        }
    }

    // ================= DAMAGE PLAYER (cho EnemyAI gọi) =================

    public void DamagePlayer(int dmg)
    {
        int finalDamage = Mathf.Max(0, dmg - playerShield);
        playerShield = Mathf.Max(0, playerShield - dmg);
        playerHP -= finalDamage;
        playerHP = Mathf.Max(0, playerHP);

        SpawnPopup("-" + finalDamage, Color.red, popupSpawnPlayer);

        if (playerHP <= 0)
        {
            isBattleOver = true;
            Debug.Log("💀 Player defeated!");
        }
    }

    // ================= TURN =================

    public void EndTurn()
    {
        if (isBattleOver) return;
        if (!playerTurn) return;

        if (extraTurn)
        {
            extraTurn = false;
            Debug.Log("⚡ Extra turn! Player goes again!");
            return;
        }

        Debug.Log("=== SWITCHING TO ENEMY TURN ===");
        playerTurn = false;
        StartCoroutine(EnemyTurnRoutine());
    }

    IEnumerator EnemyTurnRoutine()
    {
        // đợi board xong hoàn toàn
        BoardManager bm = FindFirstObjectByType<BoardManager>();
        if (bm != null)
        {
            while (bm.IsBusy)
                yield return null;
        }

        yield return new WaitForSeconds(0.5f);

        Debug.Log("=== ENEMY TURN START ===");

        EnemyAI ai = FindFirstObjectByType<EnemyAI>();

        if (ai != null)
        {
            yield return ai.DoEnemyMove();

            // đợi board xong sau khi AI swap
            if (bm != null)
            {
                while (bm.IsBusy)
                    yield return null;
            }
        }

        yield return new WaitForSeconds(0.3f);

        Debug.Log("=== SWITCHING TO PLAYER TURN ===");
        playerTurn = true;
    }

    // ================= POPUP =================

    public void SpawnPopup(string text, Color color, Transform pos)
    {
        if (damagePopupPrefab == null || pos == null) return;

        GameObject obj = Instantiate(damagePopupPrefab, pos.position, Quaternion.identity);
        DamagePopup popup = obj.GetComponent<DamagePopup>();
        if (popup != null)
            popup.Setup(text, color);
    }
}