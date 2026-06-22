using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class BombSpawner : MonoBehaviour
{
    [Header("Bomb Settings")]
    [SerializeField] private GameObject bombPrefab;
    [SerializeField] private LayerMask bombLayer;
    [SerializeField] private LayerMask obstacleLayer;

    [Header("Audio")]
    [SerializeField] private AudioClip plantBombSound;
    [Range(0f, 1f)][SerializeField] private float plantBombVolume = 0.5f;
    [SerializeField] private float minPitch = 0.85f;
    [SerializeField] private float maxPitch = 1.15f;

    private PlayerControls controls;
    private PlayerStats playerStats;

    private int currentBombs = 0;

    private List<Bomb> activeBombs = new List<Bomb>();

    private void Awake()
    {
        controls = new PlayerControls();
        controls.Player.PlaceBomb.performed += context => SpawnBomb();
        controls.Player.Detonate.performed += context => DetonateOldestBomb();

        playerStats = GetComponent<PlayerStats>();
    }

    private void OnEnable() => controls.Enable();
    private void OnDisable() => controls.Disable();

    private void SpawnBomb()
    {
        if (bombPrefab == null || playerStats == null) return;
        if (currentBombs >= playerStats.MaxBombs) return;

        Vector2 playerPosition = transform.position;
        float snappedX = Mathf.Floor(playerPosition.x) + 0.5f;
        float snappedY = Mathf.Floor(playerPosition.y) + 0.5f;
        Vector2 snapPosition = new Vector2(snappedX, snappedY);

        Collider2D overlappingBomb = Physics2D.OverlapCircle(snapPosition, 0.1f, bombLayer);
        if (overlappingBomb != null) return;

        Collider2D overlappingObstacle = Physics2D.OverlapPoint(snapPosition, obstacleLayer);
        if (overlappingObstacle != null) return;

        GameObject spawnedBomb = Instantiate(bombPrefab, snapPosition, Quaternion.identity);
        currentBombs++;
                
        if (AudioManager.Instance != null && plantBombSound != null)
        {
            AudioManager.Instance.PlaySFX(plantBombSound, plantBombVolume, Random.Range(minPitch, maxPitch));
        }

        Bomb bombScript = spawnedBomb.GetComponent<Bomb>();
        if (bombScript != null)
        {
            bombScript.InitializeBomb(this, playerStats.FireRange, playerStats.HasDetonator);
            activeBombs.Add(bombScript);
        }
    }

    private void DetonateOldestBomb()
    {
        if (!playerStats.HasDetonator) return;

        // Remove bombs destroyed by chain reactions
        activeBombs.RemoveAll(b => b == null);

        if (activeBombs.Count > 0)
        {
            Bomb oldestBomb = activeBombs[0];
            activeBombs.RemoveAt(0);

            if (oldestBomb != null)
            {
                oldestBomb.ForceExplode();
            }
        }
    }

    public void OnBombExploded()
    {
        currentBombs--;
        if (currentBombs < 0) currentBombs = 0;
    }
}