using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class LevelGenerator : MonoBehaviour
{
    [Header("Grid Settings")]
    [SerializeField] private int width = 15;
    [SerializeField] private int height = 11;

    [Header("Static Environment (Tilemap)")]
    [SerializeField] private Tilemap floorTilemap;
    [SerializeField] private TileBase floorTile;
    [SerializeField] private Tilemap solidWallTilemap;
    [SerializeField] private TileBase solidWallTile;

    [Header("Dynamic Objects (Prefabs)")]
    [SerializeField] private GameObject cratePrefab;

    [Header("Hidden Items")]
    [SerializeField] private GameObject gatePrefab;
    [SerializeField] private GameObject[] powerUpPrefabs;

    [Header("Enemies")]
    [SerializeField] private GameObject enemyPrefab;
    [SerializeField] private int enemiesToSpawn = 3;

    [Header("Level Setup")]
    [SerializeField] private int cratesToSpawn = 20;

    public int CratesToSpawn => cratesToSpawn;

    private void Start()
    {
        GenerateLevel();
    }

    private void GenerateLevel()
    {
        floorTilemap.ClearAllTiles();
        solidWallTilemap.ClearAllTiles();

        List<Vector2> availableSpaces = new List<Vector2>();

        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                Vector3Int cellPosition = new Vector3Int(x, y, 0);
                Vector2 worldSpawnPos = new Vector2(x + 0.5f, y + 0.5f);

                floorTilemap.SetTile(cellPosition, floorTile);

                if (x == 0 || x == width - 1 || y == 0 || y == height - 1)
                {
                    solidWallTilemap.SetTile(cellPosition, solidWallTile);
                }
                else if (x % 2 == 0 && y % 2 == 0)
                {
                    solidWallTilemap.SetTile(cellPosition, solidWallTile);
                }
                else
                {
                    // Keep player spawn area clear
                    if ((x == 1 && y == height - 2) ||
                        (x == 1 && y == height - 3) ||
                        (x == 2 && y == height - 2))
                    {
                        continue;
                    }

                    availableSpaces.Add(worldSpawnPos);
                }
            }
        }

        int spawned = 0;
        List<Crate> spawnedCrates = new List<Crate>();

        while (spawned < cratesToSpawn && availableSpaces.Count > 0)
        {
            int randomIndex = Random.Range(0, availableSpaces.Count);
            Vector2 cratePos = availableSpaces[randomIndex];

            GameObject newCrateObj = Instantiate(cratePrefab, cratePos, Quaternion.identity, transform);
            Crate newCrateScript = newCrateObj.GetComponent<Crate>();
            spawnedCrates.Add(newCrateScript);

            availableSpaces.RemoveAt(randomIndex);
            spawned++;
        }

        if (spawnedCrates.Count >= 2)
        {
            int gateIndex = Random.Range(0, spawnedCrates.Count);
            spawnedCrates[gateIndex].hiddenItemPrefab = gatePrefab;
            spawnedCrates.RemoveAt(gateIndex);

            GameObject selectedPowerUp = null;
            if (powerUpPrefabs != null && powerUpPrefabs.Length > 0)
            {
                int randomPowerUpIndex = Random.Range(0, powerUpPrefabs.Length);
                selectedPowerUp = powerUpPrefabs[randomPowerUpIndex];
            }

            int crateForPowerUpIndex = Random.Range(0, spawnedCrates.Count);
            spawnedCrates[crateForPowerUpIndex].hiddenItemPrefab = selectedPowerUp;
            spawnedCrates.RemoveAt(crateForPowerUpIndex);
        }

        // Spawn enemies away from player start position
        List<Vector2> safeEnemySpaces = new List<Vector2>();
        Vector2 playerStartPos = new Vector2(1.5f, height - 1.5f);

        foreach (Vector2 space in availableSpaces)
        {
            if (Vector2.Distance(playerStartPos, space) > 3f)
            {
                safeEnemySpaces.Add(space);
            }
        }

        int spawnedEnemies = 0;
        while (spawnedEnemies < enemiesToSpawn && safeEnemySpaces.Count > 0)
        {
            int randomIndex = Random.Range(0, safeEnemySpaces.Count);
            Vector2 enemyPos = safeEnemySpaces[randomIndex];

            Instantiate(enemyPrefab, enemyPos, Quaternion.identity, transform);

            safeEnemySpaces.RemoveAt(randomIndex);
            spawnedEnemies++;
        }
    }
}