using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HintSystem : MonoBehaviour
{
    public BoardManager board;
    public float hintDelay = 3f;
    public float blinkSpeed = 2f;
    public int blinkCount = 3;

    float timer;
    Coroutine blinkCoroutine;

    void Update()
    {
        if (board == null) return;

        timer += Time.deltaTime;

        if (timer >= hintDelay)
        {
            timer = 0f;
            ShowHint();
        }
    }

    /// <summary>
    /// Player swap thì reset timer + dừng blink
    /// </summary>
    public void ResetTimer()
    {
        timer = 0f;
        StopBlink();
    }

    void ShowHint()
    {
        // tìm 1 cặp swap hợp lệ
        for (int x = 0; x < board.width; x++)
        {
            for (int y = 0; y < board.height; y++)
            {
                if (x + 1 < board.width)
                {
                    Gem a = board.board[x, y];
                    Gem b = board.board[x + 1, y];
                    if (a != null && b != null && CheckSwapMakesMatch(x, y, x + 1, y))
                    {
                        StopBlink();
                        blinkCoroutine = StartCoroutine(Blink(a, b));
                        return;
                    }
                }

                if (y + 1 < board.height)
                {
                    Gem a = board.board[x, y];
                    Gem b = board.board[x, y + 1];
                    if (a != null && b != null && CheckSwapMakesMatch(x, y, x, y + 1))
                    {
                        StopBlink();
                        blinkCoroutine = StartCoroutine(Blink(a, b));
                        return;
                    }
                }
            }
        }
    }

    bool CheckSwapMakesMatch(int x1, int y1, int x2, int y2)
    {
        Gem a = board.board[x1, y1];
        Gem b = board.board[x2, y2];
        if (a == null || b == null) return false;

        board.board[x1, y1] = b;
        board.board[x2, y2] = a;

        bool found = board.HasMatchAt(x1, y1, b.gemType) || board.HasMatchAt(x2, y2, a.gemType);

        board.board[x1, y1] = a;
        board.board[x2, y2] = b;

        return found;
    }

    IEnumerator Blink(Gem a, Gem b)
    {
        SpriteRenderer srA = a != null ? a.GetComponent<SpriteRenderer>() : null;
        SpriteRenderer srB = b != null ? b.GetComponent<SpriteRenderer>() : null;

        Color origA = srA != null ? srA.color : Color.white;
        Color origB = srB != null ? srB.color : Color.white;

        for (int i = 0; i < blinkCount; i++)
        {
            // check null mỗi frame — gem có thể bị Destroy bất cứ lúc nào
            if (a == null || b == null || srA == null || srB == null)
                yield break;

            // sáng
            srA.color = Color.white;
            srB.color = Color.white;
            yield return new WaitForSeconds(1f / blinkSpeed / 2f);

            if (a == null || b == null || srA == null || srB == null)
                yield break;

            // tối
            srA.color = origA;
            srB.color = origB;
            yield return new WaitForSeconds(1f / blinkSpeed / 2f);
        }

        // khôi phục màu gốc
        if (a != null && srA != null) srA.color = origA;
        if (b != null && srB != null) srB.color = origB;
    }

    void StopBlink()
    {
        if (blinkCoroutine != null)
        {
            StopCoroutine(blinkCoroutine);
            blinkCoroutine = null;
        }
    }
}