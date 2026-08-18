using System.Collections.Generic;
using UnityEngine;

public enum LevelLayoutType
{
    Regular,
    FourRooms,
    StripesHorizontal,
    StripesVertical,
    Maze,
    SplitStripesHorizontal,
    SplitStripesVertical
}

[CreateAssetMenu(fileName = "NewLevelData", menuName = "Bombermania/LevelData")]
public class LevelData : ScriptableObject
{
    [System.Serializable]
    public struct EnemySpawnConfig
    {
        public EnemyData enemyProfile; 
        public int count;              
    }

    [Header("Level Settings")]
    [SerializeField] private List<EnemySpawnConfig> enemiesToSpawn;
    [SerializeField] private LevelLayoutType layoutType = LevelLayoutType.Regular;
    [SerializeField] private List<PowerUpData> powerUpsToSpawn;
    [SerializeField] private int width = 15;
    [SerializeField] private int height = 11;
    [SerializeField] private int cratesToSpawn = 20;
    [SerializeField] private int timeLimitSeconds = 120;

    // Getters for read-only access
    public List<EnemySpawnConfig> EnemiesToSpawn => enemiesToSpawn;
    public LevelLayoutType LayoutType => layoutType;
    public List<PowerUpData> PowerUpsToSpawn => powerUpsToSpawn;
    public int Width => width;
    public int Height => height;
    public int CratesToSpawn => cratesToSpawn;
    public int TimeLimitSeconds => timeLimitSeconds;
}
