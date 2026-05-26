using System.Collections.Generic;
using UnityEngine;

public class EnemyAI : MonoBehaviour
{
    [Header("Identity Profile")]
    [SerializeField] private EnemyData data;

    public void Initialize(EnemyData profileData)
    {
        data = profileData;

        SpriteRenderer spriteRenderer = GetComponentInChildren<SpriteRenderer>();
        spriteRenderer.sprite = data.EnemySprite;
        spriteRenderer.sortingOrder = data.SortingOrder;
    }

    private Vector2 currentDirection;
    private Vector2 targetPosition;
    private Rigidbody2D rb;
    private Collider2D myCollider;
    private float bounceCooldown = 0f;

    [Header("Sensors")]
    [SerializeField] private LayerMask obstacleLayer;
    [SerializeField] private float sensorLength = 0.6f;

    private bool isDead = false;

    private void Start()
    {
        if (GameManager.Instance != null) GameManager.Instance.RegisterEnemy();

        rb = GetComponent<Rigidbody2D>();
        myCollider = GetComponent<Collider2D>();
                
        targetPosition = new Vector2(Mathf.Floor(transform.position.x) + 0.5f, Mathf.Floor(transform.position.y) + 0.5f);
        transform.position = targetPosition;
    }

    private void FixedUpdate()
    {
        if (isDead) return;
        
        Collider2D playerCollider = Physics2D.OverlapCircle(transform.position, data.KillRadius, data.PlayerLayer);
        if (playerCollider != null)
        {
            PlayerMovement player = playerCollider.GetComponent<PlayerMovement>();
            if (player != null) player.Die();
        }

        // Bounce cooldown to prevent multiple rapid collisions from causing erratic behavior
        if (bounceCooldown > 0) bounceCooldown -= Time.fixedDeltaTime;

        // Disable physics-based movement to ensure grid-aligned, deterministic behavior
        rb.linearVelocity = Vector2.zero;
        rb.angularVelocity = 0f;

        // Axis alignment
        Vector2 alignedPosition = rb.position;
        if (currentDirection.x != 0)
        {
            alignedPosition.y = targetPosition.y;
        }
        else if (currentDirection.y != 0)
        {
            alignedPosition.x = targetPosition.x;
        }

        // Move towards the target position
        Vector2 newPos = Vector2.MoveTowards(alignedPosition, targetPosition, data.Speed * Time.fixedDeltaTime);
        rb.MovePosition(newPos);

        // Reached target position, choose next target
        if (Vector2.Distance(rb.position, targetPosition) < 0.05f)
        {
            rb.position = targetPosition;
            ChooseNextTarget();
        }
    }


    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (currentDirection == Vector2.zero) return;

        if (bounceCooldown > 0f) return;

        // Check if we can ignore this collision based on our pass-through abilities
        if (collision.gameObject.CompareTag("Bomb") && data.CanPassBombs)
        {
            Physics2D.IgnoreCollision(myCollider, collision.collider, true);
            return;
        }
        if (collision.gameObject.CompareTag("Enemy") && data.CanPassEnemies)
        {
            Physics2D.IgnoreCollision(myCollider, collision.collider, true);
            return;
        }
        if (collision.gameObject.CompareTag("Crate") && data.CanPassCrates)
        {
            Physics2D.IgnoreCollision(myCollider, collision.collider, true);
            return;
        }


        // Check if the collision is head-on (from the direction we're moving towards)
        bool isHeadOnCollision = false;
        foreach (ContactPoint2D contact in collision.contacts)
        {
            // -0.5f to allow for some angle of collision, not just perfectly head-on
            if (Vector2.Dot(contact.normal, currentDirection) < -0.5f)
            {
                isHeadOnCollision = true;
                break;
            }
        }
                
        if (isHeadOnCollision)
        {
            currentDirection = -currentDirection;
            targetPosition += currentDirection;

            bounceCooldown = 0.2f;
        }
    }

    private void ChooseNextTarget()
    {
        // Check available directions from the current position
        List<Vector2> availableDirections = GetAvailableDirections(targetPosition);

        if (availableDirections.Count == 0)
        {
            // if we're completely boxed in, just stay put and wait for the next update to check again
            currentDirection = Vector2.zero;
            return;
        }

        // check if we can keep going in the same direction
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
            if (Random.value <= data.SpontaneousTurnChance)
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
                else if (Random.value <= 0.1f) // small chance to reverse direction if no side paths available
                {
                    currentDirection = -currentDirection;
                }
            }
        }
        
        targetPosition += currentDirection;
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
                if (hit.collider.CompareTag("Bomb") && data.CanPassBombs)
                {
                    Physics2D.IgnoreCollision(myCollider, hit.collider, true);
                    continue;
                }

                if(hit.collider.CompareTag("Crate") && data.CanPassCrates)
                {
                    Physics2D.IgnoreCollision(myCollider, hit.collider, true);
                    continue;
                }

                if (hit.collider.CompareTag("Enemy") && data.CanPassEnemies)
                {
                    Physics2D.IgnoreCollision(myCollider, hit.collider, true);
                    continue;
                }

                // if we hit a non-trigger collider that isn't passable, the path is blocked
                return true;
            }
        }
        return false;
    }    

    public void Die()
    {
        if (isDead) return;
        isDead = true;

        if (GameManager.Instance != null) GameManager.Instance.EnemyDefeated(transform.position, data.ScoreValue);

        Destroy(gameObject);
    }
}