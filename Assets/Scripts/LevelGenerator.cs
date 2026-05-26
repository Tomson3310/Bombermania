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

        List<Vector2> availableSpaces = new List<Vector2>();

        // Player start position (bottom-left corner, away from walls)
        Vector2 playerStartPos = new Vector2(1.5f, currentLevel.Height - 1.5f);
        
        if (playerPrefab != null)
        {
            Instantiate(playerPrefab, playerStartPos, Quaternion.identity, transform);
        }

        // Generating floor and walls
        for (int x = 0; x < currentLevel.Width; x++)
        {
            for (int y = 0; y < currentLevel.Height; y++)
            {
                Vector3Int cellPosition = new Vector3Int(x, y, 0);
                Vector2 worldSpawnPos = new Vector2(x + 0.5f, y + 0.5f);

                floorTilemap.SetTile(cellPosition, floorTile);

                // Outer walls
                if (x == 0 || x == currentLevel.Width - 1 || y == 0 || y == currentLevel.Height - 1)
                {
                    solidWallTilemap.SetTile(cellPosition, solidWallTile);
                }
                // Internal indestructible pillars
                else if (x % 2 == 0 && y % 2 == 0)
                {
                    solidWallTilemap.SetTile(cellPosition, solidWallTile);
                }
                else
                {
                    // Safe zone near player start (to prevent immediate trapping)
                    if ((x == 1 && y == currentLevel.Height - 2) ||
                        (x == 1 && y == currentLevel.Height - 3) ||
                        (x == 2 && y == currentLevel.Height - 2))
                    {
                        continue;
                    }

                    availableSpaces.Add(worldSpawnPos);
                }
            }
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
                
                int randomPowerUpIndex = Random.Range(0, currentLevel.PowerUpsToSpawn.Count);
                PowerUpData selectedPowerUpData = currentLevel.PowerUpsToSpawn[randomPowerUpIndex];

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
            if (Vector2.Distance(playerStartPos, space) > 3f)
            {
                safeEnemySpaces.Add(space);
            }
        }

        foreach (LevelData.EnemySpawnConfig config in currentLevel.EnemiesToSpawn)
        {
            for (int i = 0; i < config.count; i++)
            {
                if (safeEnemySpaces.Count == 0) break; // no more space for enemies

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
}