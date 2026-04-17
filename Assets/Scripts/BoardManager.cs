using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class BoardManager : MonoBehaviour
{
    [Header("Board")]
    public int width = 8;
    public int height = 8;
    public GameObject[] gemPrefabs; // 8 prefabs: đúng thứ tự GemType enum

    [Header("Timing")]
    public float swapDuration = 0.1f;
    public float fallSpeed = 12f;
    public float cascadeDelay = 0.05f;

    [Header("Input")]
    public float minDragDistance = 0.25f;

    // internal
    public Gem[,] board;
    bool busy;
    Gem dragGem;
    Vector2 dragStart;
    public bool IsBusy => busy;

    int comboCount;

    // ======================= INIT =======================

    void Start()
    {
        board = new Gem[width, height];
        StartCoroutine(InitBoard());
    }

    IEnumerator InitBoard()
    {
        busy = true;

        List<Coroutine> drops = new List<Coroutine>();

        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                GemType type = PickType(x, y, true);
                float spawnY = height + 2 + y;
                Vector2 spawnPos = GridToWorld(x, (int)spawnY);
                Vector2 targetPos = GridToWorld(x, y);

                Gem gem = CreateGem(type, x, y, spawnPos);
                float dist = spawnY - y;
                float dur = dist / fallSpeed;

                drops.Add(StartCoroutine(AnimateMove(gem.transform, spawnPos, targetPos, dur)));
            }
        }

        foreach (var c in drops)
            yield return c;

        yield return Resolve();

        if (!HasAnyValidMove())
            yield return RespawnBoard();

        busy = false;
    }

    // ======================= INPUT (New Input System) =======================

    void Update()
    {
        if (busy) return;

        var pointer = Pointer.current;
        if (pointer == null) return;

        if (BattleManager.Instance != null && !BattleManager.Instance.playerTurn) return;

        Vector2 screenPos = pointer.position.ReadValue();
        bool began = pointer.press.wasPressedThisFrame;
        bool ended = pointer.press.wasReleasedThisFrame;

        if (began)
        {
            Gem gem = RaycastGem(screenPos);
            if (gem != null)
            {
                dragGem = gem;
                dragStart = Camera.main.ScreenToWorldPoint(screenPos);
            }
        }

        if (ended && dragGem != null)
        {
            Vector2 dragEnd = Camera.main.ScreenToWorldPoint(screenPos);
            Vector2 dir = dragEnd - dragStart;

            if (dir.magnitude > minDragDistance)
            {
                int dx = 0, dy = 0;
                if (Mathf.Abs(dir.x) > Mathf.Abs(dir.y))
                    dx = dir.x > 0 ? 1 : -1;
                else
                    dy = dir.y > 0 ? 1 : -1;

                int nx = dragGem.x + dx;
                int ny = dragGem.y + dy;

                if (InBounds(nx, ny))
                    StartCoroutine(TrySwap(dragGem, board[nx, ny]));
            }

            dragGem = null;
        }
    }
public void StartSwap(Gem a, Gem b)
    {
        if (busy) return;
        if (a == null || b == null) return;

        StartCoroutine(TrySwap(a, b));
    }

    Gem RaycastGem(Vector2 screenPos)
    {
        Camera cam = Camera.main;
        if (cam == null) return null;

        Vector2 worldPos = cam.ScreenToWorldPoint(screenPos);
        RaycastHit2D hit = Physics2D.Raycast(worldPos, Vector2.zero);

        if (hit.collider == null) return null;
        return hit.collider.GetComponent<Gem>();
    }

    // ======================= SWAP =======================

    IEnumerator TrySwap(Gem a, Gem b)
    {
        if (a == null || b == null) yield break;

        busy = true;

        // Pause timer khi đang resolve
        if (BattleUI.Instance != null)
            BattleUI.Instance.PauseTurnTimer();

        // reset hint khi player bắt đầu swap
        HintSystem hint = FindFirstObjectByType<HintSystem>();
        if (hint != null) hint.ResetTimer();

        yield return AnimateSwap(a, b);
        SwapData(a, b);

        var matches = FindAllMatches();

        if (matches.Count == 0)
        {
            // swap lại nếu không có match
            yield return AnimateSwap(a, b);
            SwapData(a, b);
            busy = false;
            yield break;
        }

        yield return Resolve();

        if (!HasAnyValidMove())
        {
            Debug.Log("No valid moves! Respawning...");
            yield return RespawnBoard();
        }

        busy = false;

        // thông báo kết thúc lượt cho BattleManager
        if (BattleManager.Instance != null)
            BattleManager.Instance.EndTurn();
    }

    /// <summary>
    /// AI dùng để thực hiện swap
    /// </summary>
    public IEnumerator DoSwapFromAI(Gem a, Gem b)
    {
        busy = true;

        yield return AnimateSwap(a, b);
        SwapData(a, b);

        yield return Resolve();

        if (!HasAnyValidMove())
            yield return RespawnBoard();

        busy = false;
    }

    void SwapData(Gem a, Gem b)
    {
        board[a.x, a.y] = b;
        board[b.x, b.y] = a;

        int ax = a.x, ay = a.y;
        a.SetPosition(b.x, b.y);
        b.SetPosition(ax, ay);
    }

    IEnumerator AnimateSwap(Gem a, Gem b)
    {
        Vector3 posA = a.transform.position;
        Vector3 posB = b.transform.position;

        float t = 0f;
        while (t < swapDuration)
        {
            t += Time.deltaTime;
            float k = Mathf.Clamp01(t / swapDuration);
            a.transform.position = Vector3.Lerp(posA, posB, k);
            b.transform.position = Vector3.Lerp(posB, posA, k);
            yield return null;
        }

        a.transform.position = posB;
        b.transform.position = posA;
    }

    // ======================= RESOLVE =======================

    IEnumerator Resolve()
    {
        comboCount = 0;
        int safety = 100;

        while (safety > 0)
        {
            safety--;

            var matches = FindAllMatches();
            if (matches.Count == 0) break;

            comboCount++;

            // gửi match cho BattleManager kèm combo
            if (BattleManager.Instance != null)
                BattleManager.Instance.ProcessMatches(matches, comboCount);

            // xóa gem
            foreach (var gem in matches)
            {
                if (gem != null)
                {
                    board[gem.x, gem.y] = null;
                    Destroy(gem.gameObject);
                }
            }

            yield return new WaitForSeconds(cascadeDelay);
            yield return Collapse();
            yield return new WaitForSeconds(cascadeDelay);
            yield return SpawnNew();
            yield return new WaitForSeconds(cascadeDelay);
        }

        if (comboCount > 1)
            Debug.Log($"Combo x{comboCount}!");
    }

    // ======================= COLLAPSE =======================

    IEnumerator Collapse()
    {
        List<Coroutine> moves = new List<Coroutine>();

        for (int x = 0; x < width; x++)
        {
            int writeY = 0;

            for (int y = 0; y < height; y++)
            {
                if (board[x, y] == null) continue;

                if (y != writeY)
                {
                    Gem g = board[x, y];
                    board[x, y] = null;
                    board[x, writeY] = g;

                    Vector3 from = g.transform.position;
                    Vector3 to = GridToWorld(x, writeY);

                    float dist = y - writeY;
                    float dur = dist / fallSpeed;

                    g.SetPosition(x, writeY);

                    moves.Add(StartCoroutine(AnimateMove(g.transform, from, to, dur)));
                }

                writeY++;
            }
        }

        foreach (var c in moves)
            yield return c;
    }

    // ======================= SPAWN =======================

    IEnumerator SpawnNew()
    {
        List<Coroutine> moves = new List<Coroutine>();

        for (int x = 0; x < width; x++)
        {
            List<int> emptyYs = new List<int>();
            for (int y = 0; y < height; y++)
            {
                if (board[x, y] == null)
                    emptyYs.Add(y);
            }

            for (int i = 0; i < emptyYs.Count; i++)
            {
                int targetY = emptyYs[i];
                GemType type = PickType(x, targetY, true);

                float spawnOffsetY = height + 1 + i;
                Vector2 spawnPos = GridToWorld(x, (int)spawnOffsetY);
                Vector2 targetPos = GridToWorld(x, targetY);

                Gem gem = CreateGem(type, x, targetY, spawnPos);

                float dist = spawnOffsetY - targetY;
                float dur = dist / fallSpeed;

                moves.Add(StartCoroutine(AnimateMove(gem.transform, spawnPos, targetPos, dur)));
            }
        }

        foreach (var c in moves)
            yield return c;
    }

    // ======================= MATCH =======================

    public HashSet<Gem> FindAllMatches()
    {
        HashSet<Gem> matched = new HashSet<Gem>();

        // ngang
        for (int y = 0; y < height; y++)
        {
            int run = 1;
            for (int x = 1; x < width; x++)
            {
                if (SameType(board[x, y], board[x - 1, y]))
                {
                    run++;
                }
                else
                {
                    if (run >= 3) CollectHorizontal(matched, x - run, y, run);
                    run = 1;
                }
            }
            if (run >= 3) CollectHorizontal(matched, width - run, y, run);
        }

        // dọc
        for (int x = 0; x < width; x++)
        {
            int run = 1;
            for (int y = 1; y < height; y++)
            {
                if (SameType(board[x, y], board[x, y - 1]))
                {
                    run++;
                }
                else
                {
                    if (run >= 3) CollectVertical(matched, x, y - run, run);
                    run = 1;
                }
            }
            if (run >= 3) CollectVertical(matched, x, height - run, run);
        }

        return matched;
    }

    bool SameType(Gem a, Gem b)
    {
        if (a == null || b == null) return false;
        return a.gemType == b.gemType;
    }

    void CollectHorizontal(HashSet<Gem> set, int startX, int y, int len)
    {
        for (int i = 0; i < len; i++)
        {
            Gem g = board[startX + i, y];
            if (g != null) set.Add(g);
        }
    }

    void CollectVertical(HashSet<Gem> set, int x, int startY, int len)
    {
        for (int i = 0; i < len; i++)
        {
            Gem g = board[x, startY + i];
            if (g != null) set.Add(g);
        }
    }

    // ======================= CHECK VALID MOVES =======================

    bool HasAnyValidMove()
    {
        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                if (x + 1 < width && CheckSwapMakesMatch(x, y, x + 1, y))
                    return true;

                if (y + 1 < height && CheckSwapMakesMatch(x, y, x, y + 1))
                    return true;
            }
        }

        return false;
    }

    bool CheckSwapMakesMatch(int x1, int y1, int x2, int y2)
    {
        Gem a = board[x1, y1];
        Gem b = board[x2, y2];

        if (a == null || b == null) return false;

        board[x1, y1] = b;
        board[x2, y2] = a;

        bool found = HasMatchAt(x1, y1, b.gemType) || HasMatchAt(x2, y2, a.gemType);

        board[x1, y1] = a;
        board[x2, y2] = b;

        return found;
    }

    public bool HasMatchAt(int x, int y, GemType type)
    {
        int countH = 1;
        for (int i = x - 1; i >= 0; i--)
        {
            if (board[i, y] != null && board[i, y].gemType == type) countH++;
            else break;
        }
        for (int i = x + 1; i < width; i++)
        {
            if (board[i, y] != null && board[i, y].gemType == type) countH++;
            else break;
        }
        if (countH >= 3) return true;

        int countV = 1;
        for (int i = y - 1; i >= 0; i--)
        {
            if (board[x, i] != null && board[x, i].gemType == type) countV++;
            else break;
        }
        for (int i = y + 1; i < height; i++)
        {
            if (board[x, i] != null && board[x, i].gemType == type) countV++;
            else break;
        }
        if (countV >= 3) return true;

        return false;
    }

    // ======================= RESPAWN =======================

    IEnumerator RespawnBoard()
    {
        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                if (board[x, y] != null)
                {
                    Destroy(board[x, y].gameObject);
                    board[x, y] = null;
                }
            }
        }

        yield return new WaitForSeconds(0.2f);

        List<Coroutine> drops = new List<Coroutine>();

        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                GemType type = PickType(x, y, true);
                float spawnY = height + 2 + y;
                Vector2 spawnPos = GridToWorld(x, (int)spawnY);
                Vector2 targetPos = GridToWorld(x, y);

                Gem gem = CreateGem(type, x, y, spawnPos);
                float dist = spawnY - y;
                float dur = dist / fallSpeed;

                drops.Add(StartCoroutine(AnimateMove(gem.transform, spawnPos, targetPos, dur)));
            }
        }

        foreach (var c in drops)
            yield return c;

        yield return Resolve();

        if (!HasAnyValidMove())
        {
            Debug.Log("Still no valid moves after respawn, trying again...");
            yield return RespawnBoard();
        }
    }

    // ======================= HELPERS =======================

    Gem CreateGem(GemType type, int x, int y, Vector2 pos)
    {
        int prefabIndex = (int)type;

        if (prefabIndex < 0 || prefabIndex >= gemPrefabs.Length)
        {
            Debug.LogError($"Gem prefab index {prefabIndex} out of range! Total prefabs: {gemPrefabs.Length}");
            return null;
        }

        GameObject obj = Instantiate(gemPrefabs[prefabIndex], pos, Quaternion.identity, transform);
        Gem gem = obj.GetComponent<Gem>();
        gem.Init(x, y, type);
        board[x, y] = gem;
        return gem;
    }

    GemType PickType(int x, int y, bool avoidMatch)
    {
        int total = gemPrefabs.Length;

        if (!avoidMatch)
            return (GemType)Random.Range(0, total);

        int safety = 50;
        GemType type;

        do
        {
            type = (GemType)Random.Range(0, total);
            safety--;
        }
        while (safety > 0 && WouldMatch(x, y, type));

        return type;
    }

    bool WouldMatch(int x, int y, GemType type)
    {
        if (x >= 2
            && board[x - 1, y] != null && board[x - 1, y].gemType == type
            && board[x - 2, y] != null && board[x - 2, y].gemType == type)
            return true;

        if (y >= 2
            && board[x, y - 1] != null && board[x, y - 1].gemType == type
            && board[x, y - 2] != null && board[x, y - 2].gemType == type)
            return true;

        return false;
    }

    public bool InBounds(int x, int y)
    {
        return x >= 0 && x < width && y >= 0 && y < height;
    }

     public Vector2 GridToWorld(int x, int y)
    {
        // Local offset: board centred at parent origin
        float startX = -(width - 1) / 2f;
        float startY = -(height - 1) / 2f;
        Vector2 local = new Vector2(startX + x, startY + y);
        // Convert to world (accounts for parent transform offset)
        return (Vector2)transform.TransformPoint(local);
    }

    IEnumerator AnimateMove(Transform t, Vector3 from, Vector3 to, float duration)
    {
        if (t == null) yield break;

        if (duration <= 0f)
        {
            t.position = to;
            yield break;
        }

        float elapsed = 0f;
        while (elapsed < duration)
        {
            if (t == null) yield break;

            elapsed += Time.deltaTime;
            float k = Mathf.Clamp01(elapsed / duration);
            t.position = Vector3.Lerp(from, to, k);
            yield return null;
        }

        if (t != null)
            t.position = to;
    }

    // ======================= GIZMOS =======================
    
    void OnDrawGizmos()
    {
        Gizmos.color = Color.white;
        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                Gizmos.DrawWireCube(GridToWorld(x, y), Vector2.one);
            }
        }
    }
}