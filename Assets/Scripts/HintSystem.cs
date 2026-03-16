using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HintSystem : MonoBehaviour
{
    public BoardManager board;
    public float hintDelay = 3f;

    float timer;

    void Update()
    {
        if (board == null) return;

        timer += Time.deltaTime;

        if (timer >= hintDelay)
        {
            ShowHint();
            timer = 0;
        }
    }

    public void ResetTimer()
    {
        timer = 0;
    }

    void ShowHint()
    {
        for (int x = 0; x < board.width; x++)
        {
            for (int y = 0; y < board.height; y++)
            {
                if (x + 1 < board.width)
                {
                    if (Check(x, y, x + 1, y)) return;
                }

                if (y + 1 < board.height)
                {
                    if (Check(x, y, x, y + 1)) return;
                }
            }
        }
    }

    bool Check(int x1, int y1, int x2, int y2)
    {
        Gem a = board.board[x1, y1];
        Gem b = board.board[x2, y2];

        if (a == null || b == null) return false;

        board.board[x1, y1] = b;
        board.board[x2, y2] = a;

        bool match =
            board.HasMatchAt(x1, y1, b.typeId) ||
            board.HasMatchAt(x2, y2, a.typeId);

        board.board[x1, y1] = a;
        board.board[x2, y2] = b;

        if (match)
        {
            Highlight(a);
            Highlight(b);
            return true;
        }

        return false;
    }

    void Highlight(Gem gem)
    {
        StartCoroutine(Blink(gem));
    }

    IEnumerator Blink(Gem gem)
    {
        SpriteRenderer sr = gem.GetComponent<SpriteRenderer>();

        for (int i = 0; i < 3; i++)
        {
            sr.color = Color.yellow;
            yield return new WaitForSeconds(0.2f);

            sr.color = Color.white;
            yield return new WaitForSeconds(0.2f);
        }
    }
}