using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class LevelGenerator : MonoBehaviour
{
    [Header("Level Data")]
    private LevelData currentLevel;

    [Header("Static Environment (Tilemap)")]
    [SerializeField] private Tilemap floorTilemap;
    [SerializeField] private TileBase floorTile;
    [SerializeField] private Tilemap solidWallTilemap;
    [SerializeField] private TileBase solidWallTile;

    [Header("Dynamic Objects (Prefabs)")]
    [SerializeField] private GameObject playerPrefab;
    [SerializeField] private GameObject cratePrefab;
    [SerializeField] private GameObject enemyPrefab;

    [Header("Hidden Items")]
    [SerializeField] private GameObject gatePrefab;
    [SerializeField] private GameObject basePowerUpPrefab;

    [Header("Safe Zone")]
    [SerializeField] private float safeZoneRadius = 6f;

    private void Start()
    {        
        if (GameManager.Instance != null)
        {
            currentLevel = GameManager.Instance.GetCurrentLevelData();
        }

        if (currentLevel == null)
        {
            Debug.LogError("Brak danych poziomu! Sprawdź listę allLevels w GameManagerze.");
            return;
        }

        GenerateLevel();

        if (GameManager.Instance != null)
        {            
            GameManager.Instance.StartLevel(currentLevel.TimeLimitSeconds);
        }
    }

    private void GenerateLevel()
    {
        floorTilemap.ClearAllTiles();
        solidWallTilemap.ClearAllTiles();
        // Generate the floor and outer walls first
        GenerateFloorAndOuterWalls();
        // Depending on the layout type, generate the specific layout and get the player's starting position
        Vector2 playerStartPos;
        switch (currentLevel.LayoutType)
        {
            case LevelLayoutType.FourRooms:
                playerStartPos = GenerateFourRoomsLayout();
                break;
            case LevelLayoutType.StripesHorizontal:
                playerStartPos = GenerateStripesHorizontalLayout();
                break;
            case LevelLayoutType.StripesVertical:
                playerStartPos = GenerateStripesVerticalLayout();
                break;
            case LevelLayoutType.Maze:
                playerStartPos = GenerateMazeLayout();
                break;
            case LevelLayoutType.SplitStripesHorizontal:
                playerStartPos = GenerateSplitStripesHorizontalLayout();
                break;
            case LevelLayoutType.SplitStripesVertical:
                playerStartPos = GenerateSplitStripesVerticalLayout();
                break;
            case LevelLayoutType.Regular:
            default:
                playerStartPos = GenerateRegularLayout();
                break;
        }
        // Automatically determine available spaces for crates and enemies, excluding the player's starting position and its immediate surroundings
        List<Vector2> availableSpaces = GetAvailableSpaces(playerStartPos);
        if (playerPrefab != null)
        {
            Instantiate(playerPrefab, playerStartPos, Quaternion.identity, transform);
        }

        // Crates spawning
        int spawnedCratesCount = 0;
        List<Crate> spawnedCrates = new List<Crate>();

        while (spawnedCratesCount < currentLevel.CratesToSpawn && availableSpaces.Count > 0)
        {
            int randomIndex = Random.Range(0, availableSpaces.Count);
            Vector2 cratePos = availableSpaces[randomIndex];

            GameObject newCrateObj = Instantiate(cratePrefab, cratePos, Quaternion.identity, transform);
            Crate newCrateScript = newCrateObj.GetComponent<Crate>();
            spawnedCrates.Add(newCrateScript);

            availableSpaces.RemoveAt(randomIndex);
            spawnedCratesCount++;
        }

        // Gate and Power-Up assignment
        if (spawnedCrates.Count >= 2)
        {
            
            int gateIndex = Random.Range(0, spawnedCrates.Count);
            spawnedCrates[gateIndex].hiddenItemPrefab = gatePrefab;
            spawnedCrates.RemoveAt(gateIndex);            
            
            if (currentLevel.PowerUpsToSpawn != null && currentLevel.PowerUpsToSpawn.Count > 0)
            {                
                PowerUpData selectedPowerUpData = GameManager.Instance.GetRandomPowerUpFromPool();                
                int crateForPowerUpIndex = Random.Range(0, spawnedCrates.Count);
                
                spawnedCrates[crateForPowerUpIndex].hiddenItemPrefab = basePowerUpPrefab;
                spawnedCrates[crateForPowerUpIndex].powerUpData = selectedPowerUpData;

                spawnedCrates.RemoveAt(crateForPowerUpIndex);
            }
        }

        // Enemies spawning
        List<Vector2> safeEnemySpaces = new List<Vector2>();

        foreach (Vector2 space in availableSpaces)
        {
            if (Vector2.Distance(playerStartPos, space) > safeZoneRadius)
            {
                safeEnemySpaces.Add(space);
            }
        }

        foreach (LevelData.EnemySpawnConfig config in currentLevel.EnemiesToSpawn)
        {
            for (int i = 0; i < config.count; i++)
            {
                if (safeEnemySpaces.Count == 0) break;

                int randomIndex = Random.Range(0, safeEnemySpaces.Count);
                Vector2 enemyPos = safeEnemySpaces[randomIndex];
                
                GameObject newEnemyObj = Instantiate(enemyPrefab, enemyPos, Quaternion.identity, transform);
                
                EnemyAI enemyAI = newEnemyObj.GetComponent<EnemyAI>();
                if (enemyAI != null)
                {
                    enemyAI.Initialize(config.enemyProfile);
                }

                safeEnemySpaces.RemoveAt(randomIndex);
            }
        }
    }

    private Vector2 GenerateRegularLayout()
    {
        for (int x = 1; x < currentLevel.Width - 1; x++)
        {
            for (int y = 1; y < currentLevel.Height - 1; y++)
            {

                if (x % 2 == 0 && y % 2 == 0)
                {
                    solidWallTilemap.SetTile(new Vector3Int(x, y, 0), solidWallTile);
                }
            }
        }
        return new Vector2(1.5f, currentLevel.Height - 1.5f);
    }

    private Vector2 GenerateFourRoomsLayout()
    {
        // Calculate the midpoints of the level to determine the central axes
        int midX = currentLevel.Width / 2;
        int midY = currentLevel.Height / 2;

        for (int x = 1; x < currentLevel.Width - 1; x++)
        {
            for (int y = 1; y < currentLevel.Height - 1; y++)
            {
                Vector3Int cellPosition = new Vector3Int(x, y, 0);

                bool isVerticalCorridor = (x == midX);
                bool isHorizontalCorridor = (y == midY);

                // Check if the current tile is adjacent to the central axes
                bool isWallNextToVertical = (x == midX - 1 || x == midX + 1);
                bool isWallNextToHorizontal = (y == midY - 1 || y == midY + 1);

                // If the tile is part of the central corridors, we skip it to leave open space for movement
                if (isVerticalCorridor || isHorizontalCorridor)
                {
                    continue;
                }
                                
                if (isWallNextToVertical || isWallNextToHorizontal)
                {                    
                    int doorY1 = midY / 2;
                    int doorY2 = midY + (midY / 2);
                    int doorX1 = midX / 2;
                    int doorX2 = midX + (midX / 2);

                    bool isDoor = ((y == doorY1 || y == doorY2) && isWallNextToVertical) ||
                                  ((x == doorX1 || x == doorX2) && isWallNextToHorizontal);
                                        
                    if (!isDoor)
                    {
                        solidWallTilemap.SetTile(cellPosition, solidWallTile);                        
                    }
                    continue;
                }
                               
                if (x % 2 == 0 && y % 2 == 0)
                {
                    solidWallTilemap.SetTile(cellPosition, solidWallTile);
                }
            }
        }
                
        return new Vector2(midX + 0.5f, midY + 0.5f);
    }

    private Vector2 GenerateStripesVerticalLayout()
    {
        for (int x = 1; x < currentLevel.Width - 1; x++)
        {
            for (int y = 1; y < currentLevel.Height - 1; y++)
            {                
                if (x % 2 == 0)
                {                    
                    if (y > 1 && y < currentLevel.Height - 2)
                    {
                        solidWallTilemap.SetTile(new Vector3Int(x, y, 0), solidWallTile);
                    }
                }
            }
        }

        return new Vector2(1.5f, currentLevel.Height - 1.5f);
    }

    private Vector2 GenerateStripesHorizontalLayout()
    {
        for (int x = 1; x < currentLevel.Width - 1; x++)
        {
            for (int y = 1; y < currentLevel.Height - 1; y++)
            {                
                if (y % 2 == 0)
                {                    
                    if (x > 1 && x < currentLevel.Width - 2)
                    {
                        solidWallTilemap.SetTile(new Vector3Int(x, y, 0), solidWallTile);
                    }
                }
            }
        }

        return new Vector2(1.5f, currentLevel.Height - 1.5f);
    }

    private Vector2 GenerateMazeLayout()
    {        
        int midX = currentLevel.Width / 2;
        int midY = currentLevel.Height / 2;

        for (int x = 1; x < currentLevel.Width - 1; x++)
        {
            for (int y = 1; y < currentLevel.Height - 1; y++)
            {                
                int dx = Mathf.Min(x, currentLevel.Width - 1 - x);
                int dy = Mathf.Min(y, currentLevel.Height - 1 - y);
                                
                int ringDepth = Mathf.Min(dx, dy);
                                
                if (ringDepth % 2 == 0)
                {
                    int ringNumber = ringDepth / 2;
                    bool isWall = true;
                                        
                    if (x == midX)
                    {                        
                        isWall = false;
                    }
                    else if (y == midY)
                    {                        
                        if (ringNumber % 2 == 1)
                        {
                            isWall = false;
                        }
                    }

                    if (isWall)
                    {
                        solidWallTilemap.SetTile(new Vector3Int(x, y, 0), solidWallTile);
                    }
                }
            }
        }

        return new Vector2(1.5f, currentLevel.Height - 1.5f);
    }

    private Vector2 GenerateSplitStripesHorizontalLayout()
    {
        int midX = currentLevel.Width / 2;
        int midY = currentLevel.Height / 2;

        for (int x = 1; x < currentLevel.Width - 1; x++)
        {
            for (int y = 1; y < currentLevel.Height - 1; y++)
            {
                bool isWall = false;
                                
                if (y % 2 == 0)
                {
                    if (x > 1 && x < currentLevel.Width - 2)
                    {
                        isWall = true;
                    }
                }
                                
                if (x == midX)
                {
                    isWall = true;
                }
                                
                if (y == midY)
                {
                    isWall = false;
                }

                if (isWall)
                {
                    solidWallTilemap.SetTile(new Vector3Int(x, y, 0), solidWallTile);
                }
            }
        }

        return new Vector2(1.5f, currentLevel.Height - 1.5f);
    }

    private Vector2 GenerateSplitStripesVerticalLayout()
    {
        int midX = currentLevel.Width / 2;
        int midY = currentLevel.Height / 2;

        for (int x = 1; x < currentLevel.Width - 1; x++)
        {
            for (int y = 1; y < currentLevel.Height - 1; y++)
            {
                bool isWall = false;
                                
                if (x % 2 == 0)
                {
                    if (y > 1 && y < currentLevel.Height - 2)
                    {
                        isWall = true;
                    }
                }
                                
                if (y == midY)
                {
                    isWall = true;
                }
                                
                if (x == midX)
                {
                    isWall = false;
                }

                if (isWall)
                {
                    solidWallTilemap.SetTile(new Vector3Int(x, y, 0), solidWallTile);
                }
            }
        }

        return new Vector2(1.5f, currentLevel.Height - 1.5f);
    }

    private void GenerateFloorAndOuterWalls()
    {
        for (int x = 0; x < currentLevel.Width; x++)
        {
            for (int y = 0; y < currentLevel.Height; y++)
            {
                Vector3Int cellPosition = new Vector3Int(x, y, 0);
                            
                floorTilemap.SetTile(cellPosition, floorTile);
                                
                if (x == 0 || x == currentLevel.Width - 1 || y == 0 || y == currentLevel.Height - 1)
                {
                    solidWallTilemap.SetTile(cellPosition, solidWallTile);
                }
            }
        }
    }

    private List<Vector2> GetAvailableSpaces(Vector2 playerStartPos)
    {
        List<Vector2> spaces = new List<Vector2>();

        for (int x = 1; x < currentLevel.Width - 1; x++)
        {
            for (int y = 1; y < currentLevel.Height - 1; y++)
            {
                Vector3Int cellPosition = new Vector3Int(x, y, 0);
                                
                if (!solidWallTilemap.HasTile(cellPosition))
                {
                    Vector2 worldPos = new Vector2(x + 0.5f, y + 0.5f);
                                        
                    float distToPlayer = Vector2.Distance(playerStartPos, worldPos);
                    if (distToPlayer > 2.1f)
                    {
                        spaces.Add(worldPos);
                    }
                }
            }
        }

        return spaces;
    }
}