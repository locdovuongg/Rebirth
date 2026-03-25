using UnityEngine;

public class TouchInput : MonoBehaviour
{
    public BoardManager board;
    public float minDragDistance = 0.25f;

    Camera cam;
    Gem dragGem;
    Vector2 dragStartWorld;
    bool isDragging;

    void Start()
    {
        cam = Camera.main;
        if (board == null)
            board = FindFirstObjectByType<BoardManager>();
    }

    void Update()
    {
        if (board == null || board.IsBusy) return;
        if (BattleManager.Instance != null && !BattleManager.Instance.playerTurn) return;

#if UNITY_EDITOR || UNITY_STANDALONE
        HandleMouse();
#else
        HandleTouch();
#endif
    }

    // ======================= TOUCH =======================

    void HandleTouch()
    {
        if (Input.touchCount == 0) return;

        Touch touch = Input.GetTouch(0);

        switch (touch.phase)
        {
            case TouchPhase.Began:
                OnPointerDown(touch.position);
                break;

            case TouchPhase.Ended:
            case TouchPhase.Canceled:
                OnPointerUp(touch.position);
                break;
        }
    }

    // ======================= MOUSE (Editor / PC) =======================

    void HandleMouse()
    {
        if (Input.GetMouseButtonDown(0))
            OnPointerDown(Input.mousePosition);

        if (Input.GetMouseButtonUp(0))
            OnPointerUp(Input.mousePosition);
    }

    // ======================= COMMON =======================

    void OnPointerDown(Vector2 screenPos)
    {
        if (cam == null) return;

        Vector2 worldPos = cam.ScreenToWorldPoint(screenPos);
        RaycastHit2D hit = Physics2D.Raycast(worldPos, Vector2.zero);

        if (hit.collider != null)
        {
            Gem gem = hit.collider.GetComponent<Gem>();
            if (gem != null)
            {
                dragGem = gem;
                dragStartWorld = worldPos;
                isDragging = true;
            }
        }
    }

    void OnPointerUp(Vector2 screenPos)
    {
        if (!isDragging || dragGem == null)
        {
            Reset();
            return;
        }

        if (cam == null)
        {
            Reset();
            return;
        }

        Vector2 worldPos = cam.ScreenToWorldPoint(screenPos);
        Vector2 dir = worldPos - dragStartWorld;

        if (dir.magnitude >= minDragDistance)
        {
            int dx = 0, dy = 0;

            if (Mathf.Abs(dir.x) > Mathf.Abs(dir.y))
                dx = dir.x > 0 ? 1 : -1;
            else
                dy = dir.y > 0 ? 1 : -1;

            int nx = dragGem.x + dx;
            int ny = dragGem.y + dy;

            if (board.InBounds(nx, ny))
            {
                Gem target = board.board[nx, ny];
                if (target != null)
                {
                    board.StartSwap(dragGem, target);
                }
            }
        }

        Reset();
    }

    void Reset()
    {
        dragGem = null;
        isDragging = false;
    }
}