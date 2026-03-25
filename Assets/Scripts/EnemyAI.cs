using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyAI : MonoBehaviour
{
    public BoardManager board;

    void Start()
    {
        if (board == null)
            board = FindFirstObjectByType<BoardManager>();
    }

    public IEnumerator DoEnemyMove()
    {
        yield return new WaitForSeconds(0.5f);

        List<SwapOption> moves = FindMoves();

        if (moves.Count == 0)
            yield break;

        SwapOption best = ChooseBest(moves);

        yield return board.DoSwapFromAI(best.a, best.b);
    }

    List<SwapOption> FindMoves()
    {
        List<SwapOption> list = new();

        for (int x = 0; x < board.width; x++)
        {
            for (int y = 0; y < board.height; y++)
            {
                Try(x, y, x + 1, y, list);
                Try(x, y, x, y + 1, list);
            }
        }

        return list;
    }

    void Try(int x1, int y1, int x2, int y2, List<SwapOption> list)
    {
        if (!board.InBounds(x2, y2)) return;

        Gem a = board.board[x1, y1];
        Gem b = board.board[x2, y2];

        if (a == null || b == null) return;

        // swap tạm
        board.board[x1, y1] = b;
        board.board[x2, y2] = a;

        bool valid =
            board.HasMatchAt(x1, y1, b.gemType) ||
            board.HasMatchAt(x2, y2, a.gemType);

        // swap lại
        board.board[x1, y1] = a;
        board.board[x2, y2] = b;

        if (valid)
            list.Add(new SwapOption(a, b));
    }

    SwapOption ChooseBest(List<SwapOption> moves)
    {
        SwapOption best = null;
        int bestScore = -999;

        foreach (var m in moves)
        {
            int score = Evaluate(m);

            if (score > bestScore)
            {
                bestScore = score;
                best = m;
            }
        }

        return best;
    }

    int Evaluate(SwapOption m)
    {
        int score = 0;

        int ax = m.a.x, ay = m.a.y;
        int bx = m.b.x, by = m.b.y;

        board.board[ax, ay] = m.b;
        board.board[bx, by] = m.a;

        var matches = board.FindAllMatches();
        score += matches.Count * 5;

        // ưu tiên gem tấn công
        foreach (var gem in matches)
        {
            if (gem == null) continue;
            switch (gem.gemType)
            {
                case GemType.Sword:
                case GemType.HeavySword:
                    score += 3; // ưu tiên damage
                    break;
                case GemType.Heart:
                    // nếu máu thấp thì ưu tiên heal
                    if (BattleManager.Instance != null &&
                        BattleManager.Instance.enemy.currentHP < BattleManager.Instance.enemy.maxHP * 0.5f)
                        score += 4;
                    break;
                case GemType.Shield:
                    score += 2;
                    break;
            }
        }

        board.board[ax, ay] = m.a;
        board.board[bx, by] = m.b;

        return score;
    }

    class SwapOption
    {
        public Gem a, b;
        public SwapOption(Gem a, Gem b)
        {
            this.a = a;
            this.b = b;
        }
    }
}