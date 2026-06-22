using UnityEngine;
using UnityEngine.InputSystem;


public enum DeathType
{
    Normal, // Dying by enemy or time running out
    Burn    // Dying by explosion
}

[RequireComponent(typeof(Rigidbody2D))]
public class PlayerMovement : MonoBehaviour
{
    [Header("Movement Settings")]
    [SerializeField] private LayerMask obstacleLayer;

    private Rigidbody2D rb;
    private Vector2 movementInput;

    private Vector2 primaryDirection;
    private Vector2 secondaryDirection;
    private Vector2 lastRawInput;

    private bool wasBlockedX;
    private bool wasBlockedY;

    private PlayerControls controls;
    private PlayerStats playerStats;

    [Header("Animation")]
    public Animator animator;
    public SpriteRenderer spriteRenderer;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        controls = new PlayerControls();
        playerStats = GetComponent<PlayerStats>();
    }

    private void OnEnable() => controls.Enable();
    private void OnDisable() => controls.Disable();

    private void Update()
    {
        if (GameManager.Instance != null && !GameManager.Instance.isLevelActive)
        {        
            if (animator != null)
            {
                animator.SetFloat("Speed", 0f);
            }
            return;
        }


        Vector2 rawInput = controls.Player.Move.ReadValue<Vector2>();

        Vector2 input = new Vector2(
            Mathf.Abs(rawInput.x) > 0.1f ? Mathf.Sign(rawInput.x) : 0,
            Mathf.Abs(rawInput.y) > 0.1f ? Mathf.Sign(rawInput.y) : 0
        );

        Vector2 dirX = new Vector2(input.x, 0);
        Vector2 dirY = new Vector2(0, input.y);

        bool isBlockedX = input.x != 0 && IsDirectionBlocked(dirX);
        bool isBlockedY = input.y != 0 && IsDirectionBlocked(dirY);

        if (input.x != 0 && wasBlockedX && !isBlockedX)
        {
            primaryDirection = dirX;
            secondaryDirection = dirY;
        }
        else if (input.y != 0 && wasBlockedY && !isBlockedY)
        {
            primaryDirection = dirY;
            secondaryDirection = dirX;
        }
        else if (input != lastRawInput)
        {
            if (input.x != 0 && lastRawInput.x == 0)
            {
                primaryDirection = dirX;
                secondaryDirection = dirY;
            }
            else if (input.y != 0 && lastRawInput.y == 0)
            {
                primaryDirection = dirY;
                secondaryDirection = dirX;
            }
            else if (input == Vector2.zero)
            {
                primaryDirection = Vector2.zero;
                secondaryDirection = Vector2.zero;
            }
            else
            {
                if (input.x != 0) { primaryDirection = dirX; secondaryDirection = Vector2.zero; }
                else if (input.y != 0) { primaryDirection = dirY; secondaryDirection = Vector2.zero; }
            }
        }

        lastRawInput = input;
        wasBlockedX = isBlockedX;
        wasBlockedY = isBlockedY;

        movementInput = primaryDirection;

        if (primaryDirection != Vector2.zero && secondaryDirection != Vector2.zero)
        {
            if (IsDirectionBlocked(primaryDirection))
            {
                if (!IsDirectionBlocked(secondaryDirection))
                {
                    movementInput = secondaryDirection;
                }
                else
                {
                    movementInput = Vector2.zero;
                }
            }
        }

        if (animator != null)
        {
            if (movementInput != Vector2.zero)
            {
                animator.SetFloat("MoveX", movementInput.x);
                animator.SetFloat("MoveY", movementInput.y);
            }
            animator.SetFloat("Speed", movementInput.sqrMagnitude);
        }

        if (spriteRenderer != null)
        {
            if (movementInput.x < 0) spriteRenderer.flipX = true;
            else if (movementInput.x > 0) spriteRenderer.flipX = false;
        }
    }

    private bool IsDirectionBlocked(Vector2 direction)
    {
        RaycastHit2D[] hits = Physics2D.CircleCastAll(transform.position, 0.15f, direction, 0.4f, obstacleLayer);

        foreach (RaycastHit2D hit in hits)
        {
            if (hit.collider.gameObject == gameObject) continue;

            if (hit.collider.CompareTag("Bomb"))
            {
                if (playerStats.HasBombPass) continue;
                Collider2D playerCollider = GetComponent<Collider2D>();
                if (playerCollider != null && hit.collider.bounds.Intersects(playerCollider.bounds)) continue;
            }

            if (hit.collider.gameObject.layer == LayerMask.NameToLayer("Crate"))
            {
                if (playerStats.HasCratePass) continue;
            }
            return true;
        }
        return false;
    }

    private void FixedUpdate()
    {
        if (GameManager.Instance != null && !GameManager.Instance.isLevelActive)
        {
            rb.linearVelocity = Vector2.zero;
            return;
        }
        rb.linearVelocity = movementInput * playerStats.PlayerMoveSpeed;
    }

    
    public void Die(DeathType cause)
    {
        Debug.Log($"<color=magenta>[PlayerMovement]</color> Gracz zginął! Powód: {cause}. Zatrzymuję fizykę.");

        controls.Disable();
        rb.linearVelocity = Vector2.zero;

        if (playerStats != null)
        {
            playerStats.LoseLife(cause);
        }
    }
}