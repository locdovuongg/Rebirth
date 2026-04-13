using UnityEngine;

[CreateAssetMenu(menuName = "Game/Chapter")]
public class ChapterData : ScriptableObject
{
    public string chapterName;
    public EnemyData[] enemies;
}