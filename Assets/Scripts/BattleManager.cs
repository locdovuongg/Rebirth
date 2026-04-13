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
    public int ultimateDamage = 40;

    [Header("Ult Charge")]
    [Tooltip("Mỗi gem Fire cộng bao nhiêu điểm speed/ult")]
    public int fireUltCharge = 1;

    [Header("Diamond (currency)")]
    [Tooltip("Số kim cương player thu được trong trận")]
    public int playerDiamondCollected;
    public int enemyDiamondCollected;

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
        // Load player từ GameData (từ màn chọn nhân vật)
        ApplyCharacterData();

        // Load enemy từ GameManager (từ map select)
        ApplyEnemyFromMap();

        // đảm bảo HP đầy khi bắt đầu
        player.currentHP = player.maxHP;
        player.mana = 0;
        player.shield = 0;
        player.speed = 0;
        player.ultCharge = 0;
        player.extraTurnReady = false;
        player.ultimateReady = false;

        StartTurn();
    }

    /// <summary>
    /// Load enemy data từ GameManager (map select → battle)
    /// </summary>
    void ApplyEnemyFromMap()
    {
        if (GameManager.Instance == null) return;
        if (GameManager.Instance.currentChapter == null) return;

        var gm = GameManager.Instance;
        if (gm.currentLevelIndex < 0 || gm.currentLevelIndex >= gm.currentChapter.enemies.Length)
        {
            Debug.LogError($"[BattleManager] Invalid levelIndex: {gm.currentLevelIndex}");
            return;
        }

        EnemyData data = gm.currentChapter.enemies[gm.currentLevelIndex];
        if (data == null) return;

        enemy = new BattleEntity
        {
            maxHP = data.maxHP,
            currentHP = data.maxHP,
            maxMana = data.maxMana,
            mana = 0,
            speedMax = data.speedMax,
            ultChargeMax = data.ultChargeMax,
            shield = 0,
            speed = 0,
            ultCharge = 0,
            extraTurnReady = false,
            ultimateReady = false
        };

        // Set enemy avatar trên UI
        if (BattleUI.Instance != null && BattleUI.Instance.enemyAvatar != null && data.avatar != null)
            BattleUI.Instance.enemyAvatar.sprite = data.avatar;

        // Ghi đè damage values cho boss nếu cần
        // (enemy dùng swordDamage/ultimateDamage riêng nếu muốn, hiện tại dùng chung)

        Debug.Log($"[BattleManager] Enemy loaded: {data.enemyName} HP={data.maxHP} Boss={data.isBoss}");
    }

    void ApplyCharacterData()
    {
        if (GameData.Instance == null) return;

        // --- PLAYER ---
        CharacterInfo pc = GameData.Instance.selectedCharacter;
        if (pc != null)
        {
            player.maxHP = pc.maxHP;
            player.maxMana = pc.maxMana;
            player.speedMax = pc.speedMax;
            player.ultChargeMax = pc.ultChargeMax;
            swordDamage = pc.swordDamage;
            ultimateDamage = pc.ultimateDamage;

            // Set avatar trên UI
            if (BattleUI.Instance != null && BattleUI.Instance.playerAvatar != null && pc.avatar != null)
                BattleUI.Instance.playerAvatar.sprite = pc.avatar;

            // Set skills
            SkillUser su = FindFirstObjectByType<SkillUser>();
            if (su != null && pc.skills != null && pc.skills.Length > 0)
                su.skills = new List<Skill>(pc.skills);
        }

        // --- ENEMY ---
        CharacterInfo ec = GameData.Instance.selectedEnemy;
        if (ec != null)
        {
            enemy.maxHP = ec.maxHP;
            enemy.maxMana = ec.maxMana;
            enemy.speedMax = ec.speedMax;
            enemy.ultChargeMax = ec.ultChargeMax;

            if (BattleUI.Instance != null && BattleUI.Instance.enemyAvatar != null && ec.avatar != null)
                BattleUI.Instance.enemyAvatar.sprite = ec.avatar;
        }
    }

    // ================= MATCH =================

    
    public void ProcessMatches(HashSet<Gem> matches, int combo)
    {
        if (isBattleOver) return;

        // --- hiện combo ---
        if (combo > 1 && BattleUI.Instance != null)
            BattleUI.Instance.ShowCombo(combo);

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

        Debug.Log($"[Battle] ApplyEffect: {type} x{count} combo={combo} mul={mul:F2} turn={(playerTurn ? "Player" : "Enemy")}");

        switch (type)
        {
            case GemType.Sword:
                int swordDmg = (int)(count * swordDamage * mul * self.attackMultiplier);
                DealDamage(swordDmg);
                break;

            case GemType.Fire:
                // Fire = tăng điểm ult charge
                int ultPts = count * fireUltCharge;
                self.AddUltCharge(ultPts);
                SpawnPopup("+" + ultPts + " ULT", new Color(1f, 0.5f, 0.1f),
                    playerTurn ? popupPlayer : popupEnemy);
                break;

            case GemType.BuffDamage:
                // Tăng sát thương tạm thời (3 lượt, x1.5)
                int buffTurns = 3;
                float buffMul = 1.5f;
                self.ApplyStatus(StatusEffect.Type.AttackBuff, 0, buffTurns, buffMul);
                SpawnPopup("ATK UP!", new Color(1f, 0.8f, 0.2f),
                    playerTurn ? popupPlayer : popupEnemy);
                break;

            case GemType.Heart:
                int healAmount = (int)(count * 5 * mul);
                self.Heal(healAmount);
                SpawnPopup("+" + healAmount, Color.green,
                    playerTurn ? popupPlayer : popupEnemy);
                break;

            case GemType.Tear:
                int manaAmount = count * 2;
                self.AddMana(manaAmount);
                SpawnPopup("+" + manaAmount + " MP", Color.cyan,
                    playerTurn ? popupPlayer : popupEnemy);
                break;

            case GemType.Shield:
                int shieldAmount = count * 3;
                self.shield += shieldAmount;
                SpawnPopup("+" + shieldAmount + " SHD", Color.gray,
                    playerTurn ? popupPlayer : popupEnemy);
                break;

            case GemType.Horse:
                self.AddSpeed(count);
                SpawnPopup("+" + count + " SPD", new Color(0.4f, 0.8f, 1f),
                    playerTurn ? popupPlayer : popupEnemy);
                break;

            case GemType.Diamond:
                // Diamond: tích luỹ kim cương, nhận khi kết thúc trận
                int gems = count;
                if (playerTurn)
                    playerDiamondCollected += gems;
                else
                    enemyDiamondCollected += gems;
                SpawnPopup("+" + gems + " GEM", new Color(0.4f, 0.85f, 1f),
                    playerTurn ? popupPlayer : popupEnemy);
                break;
        }
    }

    BattleEntity Current() => playerTurn ? player : enemy;
    BattleEntity Target() => playerTurn ? enemy : player;

    void DealDamage(int dmg)
    {
        BattleEntity t = Target();

        int hpBefore = t.currentHP;
        t.TakeDamage(dmg);
        int hpLost = hpBefore - t.currentHP;
        int shieldAbsorbed = t.lastShieldAbsorbed;

        Transform anchor = playerTurn ? popupEnemy : popupPlayer;

        // Hiện popup shield absorbed nếu có
        if (shieldAbsorbed > 0)
            SpawnPopup($"SHD -{shieldAbsorbed}", Color.gray, anchor);

        // Hiện popup HP lost
        if (hpLost > 0)
            SpawnPopup($"-{hpLost}", Color.red, anchor);
        else if (shieldAbsorbed > 0)
            SpawnPopup("BLOCKED!", Color.gray, anchor);

        if (t.IsDead)
        {
            isBattleOver = true;
            bool playerWon = playerTurn;
            string result = playerWon ? "PLAYER WIN!" : "PLAYER LOSE!";
            Debug.Log(result);
            Debug.Log($"[Diamond] Player: {playerDiamondCollected} | Enemy: {enemyDiamondCollected}");

            // Unlock level tiếp theo nếu player thắng
            if (playerWon && GameManager.Instance != null)
                GameManager.Instance.OnBattleWon();

            // TODO: lưu kim cương vào save data / trao thưởng
            // Ví dụ: PlayerData.diamonds += playerDiamondCollected;
        }
    }

    // ================= ULTIMATE =================

    public void UseUltimate()
    {
        if (isBattleOver) return;
        if (!Current().ultimateReady) return;

        Current().ultimateReady = false;
        Current().ultCharge = 0;

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
                    StatusEffect.Type.Burn => "BURN",
                    StatusEffect.Type.Freeze => "FREEZE",
                    StatusEffect.Type.Poison => "POISON",
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
            SpawnPopup("DONG BANG!", new Color(0.3f, 0.7f, 1f), anchor);

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

        BattleEntity current = Current();

        // Check extra turn: speed đủ 5 → được thêm lượt
        if (current.extraTurnReady)
        {
            current.extraTurnReady = false;
            Debug.Log($"[Battle] EXTRA TURN! ({(playerTurn ? "Player" : "Enemy")})");
            SpawnPopup("EXTRA TURN!", new Color(1f, 0.9f, 0.3f),
                playerTurn ? popupPlayer : popupEnemy);

            // Bắt đầu lượt mới cho cùng người chơi (không đổi turn)
            StartTurn();
            return;
        }

        if (playerTurn)
        {
            playerTurn = false;
            StartCoroutine(EnemyTurn());
        }
        else
        {
            playerTurn = true;
            StartTurn();
        }
    }

    IEnumerator EnemyTurn()
    {
        yield return new WaitForSeconds(0.5f);

        if (isBattleOver) yield break;

        // Bắt đầu lượt enemy (tick status, check freeze...)
        StartTurn();

        if (isBattleOver) yield break;

        // AI ultimate nếu sẵn sàng
        if (enemy.ultimateReady)
        {
            UseUltimate();
            yield return new WaitForSeconds(0.5f);
        }

        if (isBattleOver) yield break;

        EnemyAI ai = FindFirstObjectByType<EnemyAI>();

        if (ai != null)
            yield return ai.DoEnemyMove();

        // Check extra turn cho enemy
        if (enemy.extraTurnReady && !isBattleOver)
        {
            enemy.extraTurnReady = false;
            Debug.Log("[Battle] EXTRA TURN! (Enemy)");
            SpawnPopup("EXTRA TURN!", new Color(1f, 0.9f, 0.3f), popupEnemy);
            yield return new WaitForSeconds(0.5f);

            // enemy đánh thêm lượt
            if (enemy.ultimateReady)
            {
                UseUltimate();
                yield return new WaitForSeconds(0.5f);
            }
            if (ai != null && !isBattleOver)
                yield return ai.DoEnemyMove();
        }

        if (!isBattleOver)
        {
            playerTurn = true;
            StartTurn();
        }
    }

    // ================= POPUP =================

    public void SpawnPopup(string text, Color color, Transform pos)
    {
        // Popup spawn tại world pos, nằm trong board
        Vector3 spawnPos;
        if (pos != null)
            spawnPos = pos.position;
        else
            spawnPos = Vector3.zero;

        DamagePopup.Create(text, color, spawnPos);
    }
    
}