using UnityEngine;

public class BoardManager : MonoBehaviour
{
    public int width = 8;
    public int height = 8;

    public GameObject[] gemPrefabs;
    public GameObject cellPrefab;

    Gem[,] board;

    void Start()
    {
        GenerateCells();
        GenerateBoard();
    }

    void GenerateCells()
    {
        float startX = -width / 2f + 0.5f;
        float startY = -height / 2f + 0.5f;

        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                Vector2 pos = new Vector2(startX + x, startY + y);

                Instantiate(cellPrefab, pos, Quaternion.identity, transform);
            }
        }
    }

    void GenerateBoard()
    {
        board = new Gem[width, height];

        float startX = -width / 2f + 0.5f;
        float startY = -height / 2f + 0.5f;

        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                int rand = Random.Range(0, gemPrefabs.Length);

                Vector2 pos = new Vector2(startX + x, startY + y);

                GameObject gemObj = Instantiate(gemPrefabs[rand], pos, Quaternion.identity, transform);

                Gem gem = gemObj.GetComponent<Gem>();

                gem.SetPosition(x, y);

                board[x, y] = gem;
            }
        }
    }
    void OnDrawGizmos()
{
    Gizmos.color = Color.white;

    float startX = -width / 2f + 0.5f;
    float startY = -height / 2f + 0.5f;

    for (int x = 0; x < width; x++)
    {
        for (int y = 0; y < height; y++)
        {
            Vector2 pos = new Vector2(startX + x, startY + y);
            Gizmos.DrawWireCube(pos, Vector2.one);
        }
    }
}
}