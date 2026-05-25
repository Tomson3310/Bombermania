using UnityEngine;

[CreateAssetMenu(fileName = "NewEnemyData", menuName = "Bombermania/Enemy Data")]
public class EnemyData : ScriptableObject
{
    [Header("Identity")]
    [SerializeField] private string enemyName;
    [SerializeField] private Sprite enemySprite;

    [Header("Movement Stats")]
    [SerializeField] private float speed = 2f;
    [SerializeField, Range(0f, 1f)] private float spontaneousTurnChance = 0.1f;
    [SerializeField] private bool canPassCrates;
    [SerializeField] private bool canPassBombs;
    [SerializeField] private bool canPassEnemies;

    [Header("Combat")]
    [SerializeField] private LayerMask playerLayer;
    [SerializeField] private float killRadius = 0.3f;
    [SerializeField] private int scoreValue = 100;

    [Header("Visuals")]
    [SerializeField] private int sortingOrder = 0;

    // Getters (for read-only access)
    public string EnemyName => enemyName;
    public Sprite EnemySprite => enemySprite;
    public float Speed => speed;
    public float SpontaneousTurnChance => spontaneousTurnChance;
    public bool CanPassCrates => canPassCrates;
    public bool CanPassBombs => canPassBombs;
    public bool CanPassEnemies => canPassEnemies;
    public LayerMask PlayerLayer => playerLayer;
    public float KillRadius => killRadius;
    public int SortingOrder => sortingOrder;
    public int ScoreValue => scoreValue;
}