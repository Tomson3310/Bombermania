using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyAI : MonoBehaviour
{
    [Header("Identity Profile")]
    [SerializeField] private EnemyData data;

    private Animator animator;

    public void Initialize(EnemyData profileData)
    {
        data = profileData;

        SpriteRenderer spriteRenderer = GetComponentInChildren<SpriteRenderer>();
        if (spriteRenderer != null)
        {
            spriteRenderer.sprite = data.EnemySprite;
            spriteRenderer.sortingOrder = data.SortingOrder;
        }
                
        animator = GetComponentInChildren<Animator>();
                
        if (animator != null && data.animatorController != null)
        {
            animator.runtimeAnimatorController = data.animatorController;
        }
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
        if (GameManager.Instance != null && !GameManager.Instance.isLevelActive)
        {
            rb.linearVelocity = Vector2.zero;
            UpdateAnimations();
            return;
        }

        if (isDead) return;

        Collider2D playerCollider = Physics2D.OverlapCircle(transform.position, data.KillRadius, data.PlayerLayer);
        if (playerCollider != null)
        {
            PlayerMovement player = playerCollider.GetComponent<PlayerMovement>();
            if (player != null) player.Die(DeathType.Normal);
        }

        if (bounceCooldown > 0) bounceCooldown -= Time.fixedDeltaTime;

        rb.linearVelocity = Vector2.zero;
        rb.angularVelocity = 0f;

        Vector2 alignedPosition = rb.position;
        if (currentDirection.x != 0)
        {
            alignedPosition.y = targetPosition.y;
        }
        else if (currentDirection.y != 0)
        {
            alignedPosition.x = targetPosition.x;
        }

        Vector2 newPos = Vector2.MoveTowards(alignedPosition, targetPosition, data.Speed * Time.fixedDeltaTime);
        rb.MovePosition(newPos);

        if (Vector2.Distance(rb.position, targetPosition) < 0.05f)
        {
            rb.position = targetPosition;
            ChooseNextTarget();
        }
        
        UpdateAnimations();
    }

    private void UpdateAnimations()
    {
        if (animator == null || isDead) return;
                
        if (currentDirection == Vector2.zero)
        {
            animator.SetFloat("Speed", 0f);
        }        
        else
        {
            animator.SetFloat("Speed", 1f);
            animator.SetFloat("Horizontal", currentDirection.x);
            animator.SetFloat("Vertical", currentDirection.y);
        }
    }


    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (currentDirection == Vector2.zero) return;
        if (bounceCooldown > 0f) return;

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

        bool isHeadOnCollision = false;
        foreach (ContactPoint2D contact in collision.contacts)
        {
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
        List<Vector2> availableDirections = GetAvailableDirections(targetPosition);

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
                else if (Random.value <= 0.1f)
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

                if (hit.collider.CompareTag("Crate") && data.CanPassCrates)
                {
                    Physics2D.IgnoreCollision(myCollider, hit.collider, true);
                    continue;
                }

                if (hit.collider.CompareTag("Enemy") && data.CanPassEnemies)
                {
                    Physics2D.IgnoreCollision(myCollider, hit.collider, true);
                    continue;
                }

                return true;
            }
        }
        return false;
    }

    public void Die()
    {
        if (isDead) return;
        isDead = true;
                
        currentDirection = Vector2.zero;
        rb.linearVelocity = Vector2.zero;
        if (myCollider != null) myCollider.enabled = false;
        
        if (animator != null)
        {            
            animator.Play("Death", -1, 0f);
        }

        StartCoroutine(DeathSequenceCoroutine());
    }

    private IEnumerator DeathSequenceCoroutine()
    {        
        yield return null;

        float waitTime = 1f;
        if (animator != null)
        {
            AnimatorStateInfo stateInfo = animator.GetCurrentAnimatorStateInfo(0);
            waitTime = stateInfo.length;
        }
                
        yield return new WaitForSeconds(waitTime);

        if (GameManager.Instance != null) GameManager.Instance.EnemyDefeated(transform.position, data.ScoreValue);

        Destroy(gameObject);
    }
}