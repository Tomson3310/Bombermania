using UnityEngine;

public class Bomb : MonoBehaviour
{
    [Header("Bomb Settings")]
    [SerializeField] private float fuseTime = 3f;

    [Header("Explosion Prefabs")]
    [SerializeField] private GameObject explosionCenterPrefab;
    [SerializeField] private GameObject explosionExtensionPrefab;
    [SerializeField] private GameObject explosionEndPrefab;

    [Header("Audio")]
    [SerializeField] private System.Collections.Generic.List<AudioClip> explosionSounds;
    [Range(0f, 1f)][SerializeField] private float explosionVolume = 1f;
    [SerializeField] private float minPitch = 0.85f;
    [SerializeField] private float maxPitch = 1.15f;

    [Header("Collision Settings")]
    [SerializeField] private LayerMask obstacleLayer;

    private int explosionRadius = 1;

    private Collider2D bombCollider;
    private Collider2D playerCollider;
    private BombSpawner mySpawner;

    private bool isExploding = false;
    private bool isDetonatorControlled = false;

    public void InitializeBomb(BombSpawner spawner, int radius, bool detonatorActive)
    {
        mySpawner = spawner;
        explosionRadius = radius;
        isDetonatorControlled = detonatorActive;

        // Use timed fuse if detonator is inactive
        if (!isDetonatorControlled)
        {
            Invoke(nameof(Explode), fuseTime);
        }
    }

    private void Start()
    {
        bombCollider = GetComponent<Collider2D>();
        GameObject player = GameObject.FindGameObjectWithTag("Player");

        if (player != null)
        {
            playerCollider = player.GetComponent<Collider2D>();

            if (bombCollider != null && playerCollider != null)
            {
                Physics2D.IgnoreCollision(bombCollider, playerCollider, true);
            }
        }
    }

    private void Update()
    {
        if (playerCollider != null && bombCollider != null)
        {
            if (!bombCollider.bounds.Intersects(playerCollider.bounds))
            {
                Physics2D.IgnoreCollision(bombCollider, playerCollider, false);
                playerCollider = null;
            }
        }
    }

    // Triggered when another bomb's explosion reaches this one
    public void ForceExplode()
    {
        if (!isExploding)
        {
            CancelInvoke(nameof(Explode));
            Explode();
        }
    }

    private void Explode()
    {
        if (isExploding) return;

        isExploding = true;

        // Spawn center explosion effect
        if (explosionCenterPrefab != null)
        {
            Instantiate(explosionCenterPrefab, transform.position, Quaternion.identity);
        }

        // 4 directions explosion propagation
        SpawnExplosionInDirection(Vector2.up);
        SpawnExplosionInDirection(Vector2.down);
        SpawnExplosionInDirection(Vector2.left);
        SpawnExplosionInDirection(Vector2.right);

        if (mySpawner != null)
        {
            mySpawner.OnBombExploded();
        }
        
        if (AudioManager.Instance != null && explosionSounds != null && explosionSounds.Count > 0)
        {            
            int randomIndex = Random.Range(0, explosionSounds.Count);
            AudioClip selectedExplosionSound = explosionSounds[randomIndex];
                        
            float randomPitch = Random.Range(minPitch, maxPitch);
                        
            if (selectedExplosionSound != null)
            {
                AudioManager.Instance.PlaySFX(selectedExplosionSound, explosionVolume, randomPitch);
            }
        }
        Destroy(gameObject);
    }

    private void SpawnExplosionInDirection(Vector2 direction)
    {
        // Rotation for explosion effects based on direction
        Quaternion rotation = Quaternion.identity;
        if (direction == Vector2.up) rotation = Quaternion.Euler(0, 0, 90);
        else if (direction == Vector2.left) rotation = Quaternion.Euler(0, 0, 180);
        else if (direction == Vector2.down) rotation = Quaternion.Euler(0, 0, 270);

        for (int i = 1; i <= explosionRadius; i++)
        {
            Vector2 spawnPosition = (Vector2)transform.position + (direction * i);
            Collider2D hit = Physics2D.OverlapBox(spawnPosition, new Vector2(0.5f, 0.5f), 0f, obstacleLayer);

            // Check if this is the last tile in the explosion range
            bool isLastTile = (i == explosionRadius);

            if (hit != null)
            {
                Crate crate = hit.GetComponent<Crate>();
                if (crate != null)
                {                    
                    crate.DestroyCrate();
                }

                // Chain reaction with other bombs
                Bomb otherBomb = hit.GetComponent<Bomb>();
                if (otherBomb != null)
                {
                    otherBomb.ForceExplode();
                }

                // We encountered an obstacle, so we stop the spread of fire in this direction
                break;
            }

            // If it's an empty tile, spawn the appropriate explosion effect
            if (isLastTile)
            {
                if (explosionEndPrefab != null) Instantiate(explosionEndPrefab, spawnPosition, rotation);
            }
            else
            {
                if (explosionExtensionPrefab != null) Instantiate(explosionExtensionPrefab, spawnPosition, rotation);
            }
        }
    }
}