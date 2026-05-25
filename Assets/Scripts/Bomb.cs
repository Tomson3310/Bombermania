using UnityEngine;

public class Bomb : MonoBehaviour
{
    [Header("Bomb Settings")]
    [SerializeField] private float fuseTime = 3f;
    [SerializeField] private GameObject explosionPrefab;

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

        if (explosionPrefab != null)
        {
            Instantiate(explosionPrefab, transform.position, Quaternion.identity);

            SpawnExplosionInDirection(Vector2.up);
            SpawnExplosionInDirection(Vector2.down);
            SpawnExplosionInDirection(Vector2.left);
            SpawnExplosionInDirection(Vector2.right);
        }

        if (mySpawner != null)
        {
            mySpawner.OnBombExploded();
        }

        Destroy(gameObject);
    }

    private void SpawnExplosionInDirection(Vector2 direction)
    {
        for (int i = 1; i <= explosionRadius; i++)
        {
            Vector2 spawnPosition = (Vector2)transform.position + (direction * i);

            Collider2D hit = Physics2D.OverlapBox(spawnPosition, new Vector2(0.5f, 0.5f), 0f, obstacleLayer);

            if (hit != null)
            {
                Crate crate = hit.GetComponent<Crate>();
                if (crate != null)
                {
                    crate.DestroyCrate();
                    Instantiate(explosionPrefab, spawnPosition, Quaternion.identity);
                }

                // Chain reaction: detonate adjacent bombs
                Bomb otherBomb = hit.GetComponent<Bomb>();
                if (otherBomb != null)
                {
                    otherBomb.ForceExplode();
                }

                // Stop propagation at obstacles
                break;
            }

            Instantiate(explosionPrefab, spawnPosition, Quaternion.identity);
        }
    }

}