using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyAI : MonoBehaviour
{
    public BoardManager board;

    [Header("Enemy Ultimate")]
    public int enemyUltBar = 0;
    public int enemyUltMax = 10;
    public int enemyUltDamage = 30;
    public bool enemyUltReady = false;

    void Start()
    {
        if (board == null)
            board = FindFirstObjectByType<BoardManager>();
    }

    // ================= MAIN =================

    public IEnumerator DoEnemyMove()
    {
        // đợi board sẵn sàng
        while (board.IsBusy)
            yield return null;

        yield return new WaitForSeconds(0.8f);

        // ultimate trước
        if (enemyUltReady)
        {
            UseEnemyUltimate();
            yield return new WaitForSeconds(0.8f);
        }

        // tìm tất cả nước đi hợp lệ
        List<SwapOption> moves = FindAllPossibleSwaps();

        Debug.Log($"🤖 Enemy found {moves.Count} possible swaps");

        if (moves.Count == 0)
        {
            Debug.Log("🤖 Enemy: no valid moves, skipping turn");
            yield return new WaitForSeconds(0.5f);
            yield break;
        }

        SwapOption bestMove = ChooseBestMove(moves);

        Debug.Log($"🤖 Enemy swapping ({bestMove.a.gemType} at {bestMove.a.x},{bestMove.a.y}) <-> ({bestMove.b.gemType} at {bestMove.b.x},{bestMove.b.y})");

        // thực hiện swap
        yield return board.DoSwapFromAI(bestMove.a, bestMove.b);

        // đợi board xử lý xong hoàn toàn (cascade, collapse, spawn)
        while (board.IsBusy)
            yield return null;

        yield return new WaitForSeconds(0.3f);

        Debug.Log("🤖 Enemy turn complete");
    }

    // ================= ENEMY ULTIMATE =================

    public void AddUltCharge(int value)
    {
        enemyUltBar += value;

        if (enemyUltBar >= enemyUltMax)
        {
            enemyUltBar = enemyUltMax;
            enemyUltReady = true;
            Debug.Log("🔥 ENEMY ULTIMATE READY!");
        }
    }

    void UseEnemyUltimate()
    {
        if (!enemyUltReady) return;

        enemyUltReady = false;
        enemyUltBar = 0;

        if (BattleManager.Instance != null)
        {
            BattleManager.Instance.DamagePlayer(enemyUltDamage);
            BattleManager.Instance.SpawnPopup(
                "💥-" + enemyUltDamage,
                Color.magenta,
                BattleManager.Instance.popupSpawnPlayer
            );
        }

        Debug.Log($"💥 ENEMY ULTIMATE: {enemyUltDamage} damage to Player!");
    }

    // ================= FIND MOVES =================

    List<SwapOption> FindAllPossibleSwaps()
    {
        List<SwapOption> list = new List<SwapOption>();

        for (int x = 0; x < board.width; x++)
        {
            for (int y = 0; y < board.height; y++)
            {
                // swap phải
                if (x + 1 < board.width)
                    TryAddSwap(x, y, x + 1, y, list);

                // swap trên
                if (y + 1 < board.height)
                    TryAddSwap(x, y, x, y + 1, list);
            }
        }

        return list;
    }

    void TryAddSwap(int x1, int y1, int x2, int y2, List<SwapOption> list)
    {
        Gem a = board.board[x1, y1];
        Gem b = board.board[x2, y2];

        if (a == null || b == null) return;

        // fake swap
        board.board[x1, y1] = b;
        board.board[x2, y2] = a;

        // tạm đổi position data
        int ax = a.x, ay = a.y;
        int bx = b.x, by = b.y;
        a.x = x2; a.y = y2;
        b.x = x1; b.y = y1;

        bool valid =
            board.HasMatchAt(x1, y1, b.gemType) ||
            board.HasMatchAt(x2, y2, a.gemType);

        // restore
        board.board[x1, y1] = a;
        board.board[x2, y2] = b;
        a.x = ax; a.y = ay;
        b.x = bx; b.y = by;

        if (valid)
        {
            list.Add(new SwapOption(a, b));
        }
    }

    // ================= AI LOGIC =================

    SwapOption ChooseBestMove(List<SwapOption> moves)
    {
        SwapOption best = null;
        int bestScore = -999;

        foreach (var move in moves)
        {
            int score = EvaluateMove(move);

            if (score > bestScore)
            {
                bestScore = score;
                best = move;
            }
        }

        // fallback: nếu không tìm được best → random
        if (best == null && moves.Count > 0)
            best = moves[Random.Range(0, moves.Count)];

        return best;
    }

    int EvaluateMove(SwapOption move)
    {
        Gem a = move.a;
        Gem b = move.b;

        // fake swap
        board.board[a.x, a.y] = b;
        board.board[b.x, b.y] = a;

        int ax = a.x, ay = a.y;
        int bx = b.x, by = b.y;
        a.x = bx; a.y = by;
        b.x = ax; b.y = ay;

        HashSet<Gem> matches = board.FindAllMatches();

        int score = 0;
        foreach (var gem in matches)
        {
            if (gem == null) continue;
            score += ScoreGem(gem.gemType);
        }

        // restore
        board.board[ax, ay] = a;
        board.board[bx, by] = b;
        a.x = ax; a.y = ay;
        b.x = bx; b.y = by;

        return score;
    }

    int ScoreGem(GemType type)
    {
        // ưu tiên AI: damage > ultimate > heal > shield > speed > mana > diamond
        switch (type)
        {
            case GemType.Sword:      return 10;
            case GemType.HeavySword: return 12;
            case GemType.Fire:       return 9;
            case GemType.Heart:      return 8;
            case GemType.Shield:     return 6;
            case GemType.Horse:      return 3;
            case GemType.Tear:       return 2;
            case GemType.Diamond:    return 1;
            default:                 return 0;
        }
    }

    // ================= STRUCT =================

    class SwapOption
    {
        public Gem a;
        public Gem b;

        public SwapOption(Gem a, Gem b)
        {
            this.a = a;
            this.b = b;
        }
    }
}