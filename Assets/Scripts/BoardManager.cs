using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BoardManager : MonoBehaviour
{
    [Header("Board")]
    public int width = 8;
    public int height = 8;
    public GameObject[] gemPrefabs;

    [Header("Timing")]
    public float swapDuration = 0.1f;
    public float fallSpeed = 12f;       // ô/giây — càng cao càng nhanh
    public float cascadeDelay = 0.05f;   // delay nhẹ giữa các pha

    [Header("Input")]
    public float minDragDistance = 0.25f;

    // internal
    public Gem[,] board;
    bool busy;
    Gem dragGem;
    Vector2 dragStart;

    // ======================= INIT =======================

    void Start()
    {
        board = new Gem[width, height];
        StartCoroutine(InitBoard());
    }

    IEnumerator InitBoard()
    {
        busy = true;

        // spawn tất cả ở trên cao
        List<Coroutine> drops = new List<Coroutine>();

        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                int idx = PickType(x, y, true);
                float spawnY = height + 2 + y; // offset cao dần
                Vector2 spawnPos = GridToWorld(x, (int)spawnY);
                Vector2 targetPos = GridToWorld(x, y);

                Gem gem = CreateGem(idx, x, y, spawnPos);
                float dist = spawnY - y;
                float dur = dist / fallSpeed;

                drops.Add(StartCoroutine(AnimateMove(gem.transform, spawnPos, targetPos, dur)));
            }
        }

        // chờ tất cả rơi xong cùng lúc
        foreach (var c in drops)
            yield return c;

        // dọn match có sẵn (nếu có)
        yield return Resolve();

        busy = false;
    }

    // ======================= INPUT =======================

    void Update()
    {
        if (busy) return;

        Vector2 screenPos = Vector2.zero;
        bool began = false;
        bool ended = false;

        if (Input.touchCount > 0)
        {
            Touch t = Input.GetTouch(0);
            screenPos = t.position;
            began = (t.phase == TouchPhase.Began);
            ended = (t.phase == TouchPhase.Ended);
        }
        else
        {
            screenPos = Input.mousePosition;
            began = Input.GetMouseButtonDown(0);
            ended = Input.GetMouseButtonUp(0);
        }

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

        // animate swap
        yield return AnimateSwap(a, b);

        // swap trong data
        SwapData(a, b);

        // check match
        var matches = FindAllMatches();

        if (matches.Count == 0)
        {
            // revert
            yield return AnimateSwap(a, b);
            SwapData(a, b);
            busy = false;
            yield break;
        }

        // có match → resolve cascade
        yield return Resolve();

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
        int safety = 100;

        while (safety > 0)
        {
            safety--;

            var matches = FindAllMatches();
            if (matches.Count == 0) break;

            // xóa
            foreach (var gem in matches)
            {
                if (gem != null)
                {
                    board[gem.x, gem.y] = null;
                    Destroy(gem.gameObject);
                }
            }

            yield return new WaitForSeconds(cascadeDelay);

            // rơi xuống (đồng thời)
            yield return Collapse();

            yield return new WaitForSeconds(cascadeDelay);

            // spawn mới (đồng thời)
            yield return SpawnNew();

            yield return new WaitForSeconds(cascadeDelay);
        }
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
            // đếm ô trống trong cột
            List<int> emptyYs = new List<int>();
            for (int y = 0; y < height; y++)
            {
                if (board[x, y] == null)
                    emptyYs.Add(y);
            }

            for (int i = 0; i < emptyYs.Count; i++)
            {
                int targetY = emptyYs[i];
                int idx = PickType(x, targetY, true);

                float spawnOffsetY = height + 1 + i;
                Vector2 spawnPos = GridToWorld(x, (int)spawnOffsetY);
                Vector2 targetPos = GridToWorld(x, targetY);

                Gem gem = CreateGem(idx, x, targetY, spawnPos);

                float dist = spawnOffsetY - targetY;
                float dur = dist / fallSpeed;

                moves.Add(StartCoroutine(AnimateMove(gem.transform, spawnPos, targetPos, dur)));
            }
        }

        foreach (var c in moves)
            yield return c;
    }

    // ======================= MATCH =======================

    HashSet<Gem> FindAllMatches()
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
        return a.typeId == b.typeId;
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

    // ======================= HELPERS =======================

    Gem CreateGem(int typeIdx, int x, int y, Vector2 pos)
    {
        GameObject obj = Instantiate(gemPrefabs[typeIdx], pos, Quaternion.identity, transform);
        Gem gem = obj.GetComponent<Gem>();
        gem.Init(x, y, typeIdx);
        board[x, y] = gem;
        return gem;
    }

    int PickType(int x, int y, bool avoidMatch)
    {
        if (!avoidMatch)
            return Random.Range(0, gemPrefabs.Length);

        int safety = 50;
        int idx;

        do
        {
            idx = Random.Range(0, gemPrefabs.Length);
            safety--;
        }
        while (safety > 0 && WouldMatch(x, y, idx));

        return idx;
    }

    bool WouldMatch(int x, int y, int typeId)
    {
        // 2 bên trái
        if (x >= 2
            && board[x - 1, y] != null && board[x - 1, y].typeId == typeId
            && board[x - 2, y] != null && board[x - 2, y].typeId == typeId)
            return true;

        // 2 bên dưới
        if (y >= 2
            && board[x, y - 1] != null && board[x, y - 1].typeId == typeId
            && board[x, y - 2] != null && board[x, y - 2].typeId == typeId)
            return true;

        return false;
    }

    bool InBounds(int x, int y)
    {
        return x >= 0 && x < width && y >= 0 && y < height;
    }

    Vector2 GridToWorld(int x, int y)
    {
        float startX = -width / 2f + 0.5f;
        float startY = -height / 2f + 0.5f;
        return new Vector2(startX + x, startY + y);
    }

    IEnumerator AnimateMove(Transform t, Vector3 from, Vector3 to, float duration)
    {
        if (duration <= 0f)
        {
            t.position = to;
            yield break;
        }

        float elapsed = 0f;
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float k = Mathf.Clamp01(elapsed / duration);
            t.position = Vector3.Lerp(from, to, k);
            yield return null;
        }
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