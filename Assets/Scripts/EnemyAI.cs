using System.Collections.Generic;
using UnityEngine;

public class EnemyAI : MonoBehaviour
{
    [Header("Movement")]
    [SerializeField] private float speed = 2f;
    [SerializeField, Range(0f, 1f)] private float spontaneousTurnChance = 0.1f;

    private Vector2 currentDirection;
    private Rigidbody2D rb;
    private Vector2Int lastDecisionTile = new Vector2Int(-9999, -9999);

    [Header("Sensors")]
    [SerializeField] private LayerMask obstacleLayer;
    [SerializeField] private float sensorLength = 0.6f;

    [Header("Combat")]
    [SerializeField] private LayerMask playerLayer;
    [SerializeField] private float killRadius = 0.3f;

    private bool isDead = false;

    private void Start()
    {
        if (GameManager.Instance != null) GameManager.Instance.RegisterEnemy();

        rb = GetComponent<Rigidbody2D>();
        currentDirection = GetRandomValidDirection(transform.position);
    }

    private void FixedUpdate()
    {
        if (isDead) return;

        rb.linearVelocity = currentDirection * speed;

        Collider2D playerCollider = Physics2D.OverlapCircle(transform.position, killRadius, playerLayer);
        if (playerCollider != null)
        {
            PlayerMovement player = playerCollider.GetComponent<PlayerMovement>();
            if (player != null) player.Die();
        }

        CheckIntersection();
    }

    private void CheckIntersection()
    {
        int tileX = Mathf.FloorToInt(transform.position.x);
        int tileY = Mathf.FloorToInt(transform.position.y);
        Vector2Int currentTile = new Vector2Int(tileX, tileY);
        Vector2 tileCenter = new Vector2(tileX + 0.5f, tileY + 0.5f);

        if (Vector2.Distance(transform.position, tileCenter) < 0.05f)
        {
            // Prevent re-evaluation on same tile if blocked (waiting for path to open)
            if (currentTile == lastDecisionTile && currentDirection != Vector2.zero) return;

            lastDecisionTile = currentTile;
            rb.position = tileCenter;

            List<Vector2> availableDirections = GetAvailableDirections(tileCenter);

            if (availableDirections.Count == 0)
            {
                currentDirection = Vector2.zero;
                return;
            }

            bool canGoForward = availableDirections.Contains(currentDirection);

            if (!canGoForward)
            {
                List<Vector2> options = new List<Vector2>(availableDirections);

                if (options.Count > 1 && currentDirection != Vector2.zero)
                {
                    options.Remove(-currentDirection);
                }

                currentDirection = options[Random.Range(0, options.Count)];
            }
            else
            {
                // Chance to spontaneously turn into side paths
                if (Random.value <= spontaneousTurnChance)
                {
                    List<Vector2> sidePaths = new List<Vector2>();

                    foreach (Vector2 dir in availableDirections)
                    {
                        if (dir != currentDirection && dir != -currentDirection)
                        {
                            sidePaths.Add(dir);
                        }
                    }

                    if (sidePaths.Count > 0)
                    {
                        currentDirection = sidePaths[Random.Range(0, sidePaths.Count)];
                    }
                    else if (Random.value <= 0.1f)
                    {
                        currentDirection = -currentDirection;
                    }
                }
            }
        }
    }

    // Reverse direction on head-on collision with obstacles
    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (currentDirection == Vector2.zero) return;

        foreach (ContactPoint2D contact in collision.contacts)
        {
            // Vector2.Dot determines collision angle: value < -0.5 indicates head-on impact
            if (Vector2.Dot(contact.normal, currentDirection) < -0.5f)
            {
                currentDirection = -currentDirection;
                lastDecisionTile = new Vector2Int(-9999, -9999);
                return;
            }
        }
    }

    private List<Vector2> GetAvailableDirections(Vector2 checkPosition)
    {
        List<Vector2> validDirs = new List<Vector2>();
        Vector2[] directions = { Vector2.up, Vector2.down, Vector2.left, Vector2.right };

        foreach (Vector2 dir in directions)
        {
            if (!IsDirectionBlocked(checkPosition, dir, sensorLength))
            {
                validDirs.Add(dir);
            }
        }
        return validDirs;
    }

    private bool IsDirectionBlocked(Vector2 startPos, Vector2 dir, float distance)
    {
        RaycastHit2D[] hits = Physics2D.RaycastAll(startPos, dir, distance, obstacleLayer);

        foreach (RaycastHit2D hit in hits)
        {
            if (hit.collider.gameObject != gameObject && !hit.collider.isTrigger)
            {
                return true;
            }
        }
        return false;
    }

    private Vector2 GetRandomValidDirection(Vector2 position)
    {
        List<Vector2> available = GetAvailableDirections(position);
        if (available.Count > 0) return available[Random.Range(0, available.Count)];
        return Vector2.zero;
    }

    public void Die()
    {
        if (isDead) return;
        isDead = true;

        if (GameManager.Instance != null) GameManager.Instance.EnemyDefeated(transform.position);

        Destroy(gameObject);
    }
}