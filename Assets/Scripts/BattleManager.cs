using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BattleManager : MonoBehaviour
{
    public static BattleManager Instance;

    public BattleEntity player = new();
    public BattleEntity enemy = new();

    [Header("Damage Values")]
    public int swordDamage = 10;
    public int heavySwordDamage = 15;
    public int fireDamage = 12;
    public int ultimateDamage = 40;

    [Header("Popup")]
    public GameObject damagePopupPrefab;
    public Transform popupPlayer;
    public Transform popupEnemy;

    [Header("State")]
    public bool playerTurn = true;
    bool isBattleOver;

    void Awake()
    {
        Instance = this;
    }

    void Start()
    {
        // đảm bảo HP đầy khi bắt đầu
        player.currentHP = player.maxHP;
        enemy.currentHP = enemy.maxHP;
    }

    // ================= MATCH =================

    
    public void ProcessMatches(HashSet<Gem> matches, int combo)
    {
        if (isBattleOver) return;

        // --- hiện combo ---
        if (combo > 1)
        {
            BattleHUD hud = FindFirstObjectByType<BattleHUD>();
            if (hud != null)
                hud.ShowCombo(combo);
        }

        // ...existing code... (counter + ApplyEffect)
        Dictionary<GemType, int> counter = new();

        foreach (var gem in matches)
        {
            if (gem == null) continue;

            if (!counter.ContainsKey(gem.gemType))
                counter[gem.gemType] = 0;

            counter[gem.gemType]++;
        }

        foreach (var pair in counter)
        {
            ApplyEffect(pair.Key, pair.Value, combo);
        }
    }

    void ApplyEffect(GemType type, int count, int combo)
    {
        float mul = 1f + (combo - 1) * 0.25f;

        BattleEntity self = Current();
        BattleEntity target = Target();

        switch (type)
        {
            case GemType.Sword:
                int swordDmg = (int)(count * swordDamage * mul);
                DealDamage(swordDmg);
                break;

            case GemType.HeavySword:
                int heavyDmg = (int)(count * heavySwordDamage * mul);
                DealDamage(heavyDmg);
                break;

            case GemType.Fire:
                int fireDmg = (int)(count * fireDamage * mul);
                DealDamage(fireDmg);
                break;

            case GemType.Heart:
                int healAmount = (int)(count * 5 * mul);
                self.Heal(healAmount);
                SpawnPopup("+" + healAmount, Color.green, playerTurn ? popupPlayer : popupEnemy);
                break;

            case GemType.Tear:
                int manaAmount = count * 2;
                self.AddMana(manaAmount);
                SpawnPopup("+" + manaAmount + " MP", Color.cyan, playerTurn ? popupPlayer : popupEnemy);
                break;

            case GemType.Shield:
                int shieldAmount = count * 3;
                self.shield += shieldAmount;
                SpawnPopup("+" + shieldAmount + " 🛡", Color.gray, playerTurn ? popupPlayer : popupEnemy);
                break;

            case GemType.Horse:
                self.AddSpeed(count);
                break;

            case GemType.Diamond:
                // Diamond: multi-effect — thêm mana + speed
                self.AddMana(count);
                self.AddSpeed(count);
                break;
        }
    }

    BattleEntity Current() => playerTurn ? player : enemy;
    BattleEntity Target() => playerTurn ? enemy : player;

    void DealDamage(int dmg)
    {
        BattleEntity t = Target();

        int before = t.currentHP;
        t.TakeDamage(dmg);
        int dealt = before - t.currentHP;

        SpawnPopup("-" + dealt, Color.red, playerTurn ? popupEnemy : popupPlayer);

        if (t.IsDead)
        {
            isBattleOver = true;
            string result = playerTurn ? "PLAYER WIN!" : "PLAYER LOSE!";
            Debug.Log(result);
            // TODO: show kết quả UI
        }
    }

    // ================= ULTIMATE =================

    public void UseUltimate()
    {
        if (isBattleOver) return;
        if (!Current().ultimateReady) return;

        Current().ultimateReady = false;
        Current().speed = 0;

        DealDamage(ultimateDamage);
        SpawnPopup("ULTIMATE!", Color.yellow, playerTurn ? popupEnemy : popupPlayer);
    }

    // ================= TURN =================
    public void StartTurn()
    {
        BattleEntity current = playerTurn ? player : enemy;

        // tick status effects
        StatusTickResult result = current.TickAllStatus();

        // hiện popup nếu bị damage từ status
        if (result.totalDamage > 0)
        {
            Transform anchor = playerTurn ? popupPlayer : popupEnemy;

            foreach (var dtype in result.damageTypes)
            {
                string icon = dtype switch
                {
                    StatusEffect.Type.Burn => "🔥",
                    StatusEffect.Type.Freeze => "❄️",
                    StatusEffect.Type.Poison => "☠️",
                    _ => ""
                };

                Color color = dtype switch
                {
                    StatusEffect.Type.Burn => new Color(1f, 0.4f, 0.1f),
                    StatusEffect.Type.Freeze => new Color(0.3f, 0.7f, 1f),
                    StatusEffect.Type.Poison => new Color(0.4f, 0.9f, 0.2f),
                    _ => Color.white
                };

                SpawnPopup($"{icon} -{result.totalDamage}", color, anchor);
            }
        }

        // bị đóng băng → bỏ lượt
        if (result.skipTurn)
        {
            Transform anchor = playerTurn ? popupPlayer : popupEnemy;
            SpawnPopup("❄️ ĐÓNG BĂNG!", new Color(0.3f, 0.7f, 1f), anchor);

            EndTurn(); // tự bỏ lượt
            return;
        }

        // tick skill cooldown
        if (playerTurn)
        {
            SkillUser su = FindFirstObjectByType<SkillUser>();
            if (su != null)
                su.TickAllCooldowns();
        }

        // check chết
        if (current.IsDead)
        {
            isBattleOver = true;
        }
    }
    public void EndTurn()
    {
        if (isBattleOver) return;
        if (!playerTurn) return;

        playerTurn = false;
        StartCoroutine(EnemyTurn());
    }

    IEnumerator EnemyTurn()
    {
        yield return new WaitForSeconds(0.5f);

        if (isBattleOver) yield break;

        // AI ultimate nếu sẵn sàng
        if (enemy.ultimateReady)
        {
            UseUltimate();
            yield return new WaitForSeconds(0.5f);
        }

        EnemyAI ai = FindFirstObjectByType<EnemyAI>();

        if (ai != null)
            yield return ai.DoEnemyMove();

        if (!isBattleOver)
            playerTurn = true;
    }

    // ================= POPUP =================

    public void SpawnPopup(string text, Color color, Transform pos)
    {
        if (damagePopupPrefab == null || pos == null) return;

        var obj = Instantiate(damagePopupPrefab, pos.position, Quaternion.identity);
        var popup = obj.GetComponent<DamagePopup>();
        if (popup != null)
            popup.Setup(text, color);
    }
    
}