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
        yield return new WaitForSeconds(1f);

        List<SwapOption> options = FindAllPossibleSwaps();

        if (options.Count == 0)
        {
            Debug.Log("Enemy found no moves.");
            yield break;
        }

        SwapOption choice = options[Random.Range(0, options.Count)];

        yield return board.DoSwapFromAI(choice.a, choice.b);
    }

    // ================= FIND MOVES =================

    List<SwapOption> FindAllPossibleSwaps()
    {
        List<SwapOption> list = new List<SwapOption>();

        for (int x = 0; x < board.width; x++)
        {
            for (int y = 0; y < board.height; y++)
            {
                if (x + 1 < board.width)
                    TrySwap(x, y, x + 1, y, list);

                if (y + 1 < board.height)
                    TrySwap(x, y, x, y + 1, list);
            }
        }

        return list;
    }

    void TrySwap(int x1, int y1, int x2, int y2, List<SwapOption> list)
    {
        Gem a = board.board[x1, y1];
        Gem b = board.board[x2, y2];

        if (a == null || b == null) return;

        board.board[x1, y1] = b;
        board.board[x2, y2] = a;

        bool valid =
            board.HasMatchAt(x1, y1, b.typeId) ||
            board.HasMatchAt(x2, y2, a.typeId);

        board.board[x1, y1] = a;
        board.board[x2, y2] = b;

        if (valid)
        {
            list.Add(new SwapOption(a, b));
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