using UnityEngine;
using static GridPositionTypes;
using static LaneTypes;

public class FrogController : MonoBehaviour
{
    [Header("Movement Settings")]
    [SerializeField] private float moveDuration = 0.2f;
    [SerializeField] private AudioSource jumpSound;

    [Header("Facing Visuals")]
    [SerializeField] private GameObject visualUp;
    [SerializeField] private GameObject visualDown;
    [SerializeField] private GameObject visualLeft;
    [SerializeField] private GameObject visualRight;

    [Header("Lives & Goal Systems")]
    [SerializeField] private GoalManager goalManager;

    [Header("Vehicle Detection")]
    [SerializeField] private LayerMask vehicleLayerMask = 1;

    [Header("Log Detection")]
    [SerializeField] private LayerMask logLayerMask = 1;

    private GridManager gridManager;
    private Vector2Int currentGridPosition;
    private Vector2Int targetGridPosition;
    private bool isMoving = false;
    private float moveTimer = 0f;
    private Vector2 startWorldPosition;
    private Vector2 targetWorldPosition;

    // Log tracking variables
    private bool isOnLog = false;
    private Transform currentLog = null;
    private Vector2 lastLogPosition = Vector2.zero;

    // Cache the cell size
    private float cellSize = 1f;

    private bool isDead = false;

    [Header("Car Detection")]
    [SerializeField] private float detectionRadius = 0.4f;
    private Collider2D frogCollider;

    public Vector2Int CurrentGridPosition => currentGridPosition;
    public bool IsMoving => isMoving;

    private void Start()
    {
        gridManager = FindObjectOfType<GridManager>();

        if (gridManager == null)
        {
            Debug.LogError("GridManager not found in the scene!");
            return;
        }

        CalculateCellSize();

        if (goalManager == null)
        {
            goalManager = FindObjectOfType<GoalManager>();
        }

        int startX = gridManager.width / 2;
        int startY = 0;

        currentGridPosition = new Vector2Int(startX, startY);
        targetGridPosition = currentGridPosition;
        transform.position = gridManager.GetWorldPosition(startX, startY);
        SetFacing(Vector2Int.up);

        isDead = false;

        // Get or add collider
        frogCollider = GetComponent<Collider2D>();
        if (frogCollider == null)
        {
            BoxCollider2D boxCollider = gameObject.AddComponent<BoxCollider2D>();
            boxCollider.isTrigger = true;
            boxCollider.size = new Vector2(0.6f, 0.6f);
            frogCollider = boxCollider;
        }
        else
        {
            frogCollider.isTrigger = true;
        }

        Debug.Log($"Frog initialized at grid position ({startX}, {startY})");
    }

    private void CalculateCellSize()
    {
        if (gridManager == null) return;

        Vector2 pos1 = gridManager.GetWorldPosition(0, 0);
        Vector2 pos2 = gridManager.GetWorldPosition(1, 0);
        cellSize = Vector2.Distance(pos1, pos2);

        Debug.Log($"Cell size calculated: {cellSize}");
    }

    private void Update()
    {
        // Check for car collisions every frame (manual detection)
        if (!isDead && !isMoving)
        {
            CheckForCarCollision();
        }

        // Don't process input if moving, no gridManager, or game is over/won
        if (isMoving || gridManager == null || isDead)
            return;

        if (LivesManager.Instance != null)
        {
            if (LivesManager.Instance.IsGameOver || LivesManager.Instance.HasWon)
                return;
        }

        Vector2Int direction = Vector2Int.zero;

        if (Input.GetKeyDown(KeyCode.W) || Input.GetKeyDown(KeyCode.UpArrow))
            direction = new Vector2Int(0, 1);
        else if (Input.GetKeyDown(KeyCode.S) || Input.GetKeyDown(KeyCode.DownArrow))
            direction = new Vector2Int(0, -1);
        else if (Input.GetKeyDown(KeyCode.A) || Input.GetKeyDown(KeyCode.LeftArrow))
            direction = new Vector2Int(-1, 0);
        else if (Input.GetKeyDown(KeyCode.D) || Input.GetKeyDown(KeyCode.RightArrow))
            direction = new Vector2Int(1, 0);

        if (direction == Vector2Int.zero)
            return;

        Vector2Int newGridPosition = currentGridPosition + direction;

        if (isOnLog)
        {
            Vector2 targetWorldPos = (Vector2)transform.position + new Vector2(direction.x, direction.y) * cellSize;

            Vector2Int targetGrid = gridManager.GetGridPosition(targetWorldPos);
            if (IsValidGridPosition(targetGrid))
            {
                isOnLog = false;
                currentLog = null;

                targetGridPosition = targetGrid;
                startWorldPosition = transform.position;
                targetWorldPosition = targetWorldPos;
                SetFacing(direction);
                isMoving = true;
                moveTimer = 0f;

                if (jumpSound != null)
                    jumpSound.Play();

                Debug.Log($"Frog jumping from world position ({startWorldPosition.x}, {startWorldPosition.y}) to ({targetWorldPos.x}, {targetWorldPos.y})");
            }
            else
            {
                Debug.Log($"Cannot jump to position - out of bounds");
            }
        }
        else
        {
            if (IsValidGridPosition(newGridPosition))
            {
                StartJump(newGridPosition, direction);
            }
            else
            {
                Debug.Log($"Cannot move to grid position ({newGridPosition.x}, {newGridPosition.y}) - out of bounds");
            }
        }
    }

    private void CheckForCarCollision()
    {
        // Use Physics2D.OverlapCircle to check for cars near the frog
        Collider2D[] hitColliders = Physics2D.OverlapCircleAll(
            transform.position,
            detectionRadius,
            vehicleLayerMask
        );

        foreach (Collider2D collider in hitColliders)
        {
            if (collider.CompareTag("Car") || collider.CompareTag("Vehicle"))
            {
                Debug.Log($"Frog detected car nearby: {collider.name}");
                HandleDeath("hit by a car");
                break;
            }
        }
    }
 
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (isDead)
            return;

        if (LivesManager.Instance != null && (LivesManager.Instance.IsGameOver || LivesManager.Instance.HasWon))
            return;

        if (((1 << other.gameObject.layer) & vehicleLayerMask) != 0)
        {
            if (other.CompareTag("Car") || other.CompareTag("Vehicle"))
            {
                Debug.Log($"Frog collided with {other.name}!");
                HandleDeath("hit by a car");
            }
        }
    }

    private void StartJump(Vector2Int targetGridPos, Vector2Int direction)
    {
        targetGridPosition = targetGridPos;
        startWorldPosition = transform.position;
        targetWorldPosition = gridManager.GetWorldPosition(targetGridPos.x, targetGridPos.y);
        SetFacing(direction);
        isMoving = true;
        moveTimer = 0f;

        if (jumpSound != null)
            jumpSound.Play();

        Debug.Log($"Frog jumping from ({currentGridPosition.x}, {currentGridPosition.y}) to ({targetGridPosition.x}, {targetGridPosition.y})");
    }

    private void SetFacing(Vector2Int direction)
    {
        if (visualUp == null && visualDown == null && visualLeft == null && visualRight == null)
            return;

        bool up = direction == Vector2Int.up;
        bool down = direction == Vector2Int.down;
        bool left = direction == Vector2Int.left;
        bool right = direction == Vector2Int.right;

        if (visualUp != null) visualUp.SetActive(up);
        if (visualDown != null) visualDown.SetActive(down);
        if (visualLeft != null) visualLeft.SetActive(left);
        if (visualRight != null) visualRight.SetActive(right);
    }

    private void LateUpdate()
    {
        // Move frog with log (if on log and not jumping)
        if (isOnLog && currentLog != null && !isMoving && !isDead)
        {
            Vector2 currentLogPos = currentLog.position;
            Vector2 logDelta = currentLogPos - lastLogPosition;
            transform.position += (Vector3)logDelta;
            lastLogPosition = currentLogPos;
        }

        if (!isMoving || isDead)
            return;

        moveTimer += Time.deltaTime;
        float progress = Mathf.Clamp01(moveTimer / moveDuration);
        float easedProgress = progress * progress * (3f - 2f * progress);

        Vector2 currentPosition = Vector2.Lerp(startWorldPosition, targetWorldPosition, easedProgress);
        float arcHeight = Mathf.Sin(progress * Mathf.PI) * 0.3f;
        currentPosition.y += arcHeight;

        transform.position = currentPosition;

        if (progress >= 1f)
        {
            transform.position = targetWorldPosition;

            if (isOnLog)
            {
                Vector2Int newGridPos = gridManager.GetGridPosition(transform.position);
                if (IsValidGridPosition(newGridPos))
                {
                    currentGridPosition = newGridPos;
                    targetGridPosition = newGridPos;
                }
            }
            else
            {
                currentGridPosition = targetGridPosition;
            }

            isMoving = false;
            CheckLandingTile();
            Debug.Log($"Frog landed at grid position ({currentGridPosition.x}, {currentGridPosition.y})");
        }
    }

    private bool IsValidGridPosition(Vector2Int gridPos)
    {
        if (gridPos.x < 0 || gridPos.x >= gridManager.width)
            return false;
        if (gridPos.y < 0 || gridPos.y >= gridManager.height)
            return false;
        return true;
    }

    private void CheckLandingTile()
    {
        if (isDead)
            return;

        GridPositionType tileType = gridManager.GetPositionType(
            currentGridPosition.x,
            currentGridPosition.y
        );

        switch (tileType)
        {
            case GridPositionType.Safe:
                CheckIfOnLog();
                break;

            case GridPositionType.Road:
                isOnLog = false;
                currentLog = null;
                if (IsCarAtPosition(currentGridPosition))
                    HandleDeath("hit by a car");
                break;

            case GridPositionType.River:
                if (IsLogAtPosition(currentGridPosition))
                {
                    Debug.Log("Frog landed on a log! Safe for now...");
                    CheckIfOnLog();
                }
                else
                {
                    isOnLog = false;
                    currentLog = null;
                    HandleDeath("drowned in the river");
                }
                break;

            case GridPositionType.Goal:
                isOnLog = false;
                currentLog = null;
                HandleGoalReached();
                break;
        }
    }

    private void CheckIfOnLog()
    {
        Vector2 worldPos = transform.position;
        Collider2D[] hitColliders = Physics2D.OverlapCircleAll(worldPos, 0.3f, logLayerMask);

        bool foundLog = false;

        foreach (Collider2D collider in hitColliders)
        {
            if (collider.CompareTag("Log"))
            {
                foundLog = true;
                currentLog = collider.transform;
                isOnLog = true;
                lastLogPosition = currentLog.position;
                Debug.Log("Frog is on a log!");
                break;
            }
        }

        if (!foundLog)
        {
            isOnLog = false;
            currentLog = null;
        }
    }

    private bool IsCarAtPosition(Vector2Int gridPos)
    {
        Vector2 worldPos = gridManager.GetWorldPosition(gridPos.x, gridPos.y);
        Collider2D[] hitColliders = Physics2D.OverlapCircleAll(worldPos, 0.3f, vehicleLayerMask);

        foreach (Collider2D collider in hitColliders)
        {
            if (collider.CompareTag("Car") || collider.CompareTag("Vehicle"))
                return true;
        }
        return false;
    }

    private bool IsLogAtPosition(Vector2Int gridPos)
    {
        Vector2 worldPos = gridManager.GetWorldPosition(gridPos.x, gridPos.y);
        Collider2D[] hitColliders = Physics2D.OverlapCircleAll(worldPos, 0.3f, logLayerMask);

        foreach (Collider2D collider in hitColliders)
        {
            if (collider.CompareTag("Log"))
                return true;
        }
        return false;
    }

    private void HandleDeath(string reason)
    {
        if (isDead)
            return;

        isDead = true;

        Debug.LogWarning($"Frog died - {reason}");

        isOnLog = false;
        currentLog = null;

        if (LivesManager.Instance != null)
        {
            LivesManager.Instance.LoseLife();
        }

        if (LivesManager.Instance != null && !LivesManager.Instance.IsGameOver && !LivesManager.Instance.HasWon)
        {
            RespawnAtStart();
        }
        else
        {
            Debug.Log("Game Over - frog will not respawn");
        }
    }

    private void HandleGoalReached()
    {
        if (goalManager == null)
        {
            Debug.LogError("GoalManager is null!");
            return;
        }

        bool filledNewSlot = goalManager.TryFillGoal(currentGridPosition);

        if (filledNewSlot)
        {
            Debug.Log($"Frog reached an empty goal slot at ({currentGridPosition.x}, {currentGridPosition.y})!");

            if (goalManager.FilledSlots >= goalManager.TotalSlots)
            {
                if (LivesManager.Instance != null)
                {
                    LivesManager.Instance.GameWon();
                }
                return;
            }

            if (LivesManager.Instance != null)
            {
                LivesManager.Instance.LoseLife();
            }

            if (LivesManager.Instance != null && !LivesManager.Instance.IsGameOver && !LivesManager.Instance.HasWon)
            {
                RespawnAtStart();
            }
        }
        else
        {
            Debug.LogWarning($"Goal at ({currentGridPosition.x}, {currentGridPosition.y}) is already filled!");
            HandleDeath("landed on an already-filled goal slot");
        }
    }

    private void RespawnAtStart()
    {
        if (LivesManager.Instance != null && (LivesManager.Instance.IsGameOver || LivesManager.Instance.HasWon))
        {
            Debug.Log("Game is over or won - frog won't respawn");
            return;
        }

        int startX = gridManager.width / 2;
        int startY = 0;
        TeleportToGridPosition(new Vector2Int(startX, startY));
        SetFacing(Vector2Int.up);
        isDead = false;
    }

    public void TeleportToGridPosition(Vector2Int gridPos)
    {
        if (!IsValidGridPosition(gridPos))
        {
            Debug.LogWarning($"Cannot teleport to invalid position ({gridPos.x}, {gridPos.y})");
            return;
        }

        isMoving = false;
        isOnLog = false;
        currentLog = null;
        currentGridPosition = gridPos;
        targetGridPosition = gridPos;
        transform.position = gridManager.GetWorldPosition(gridPos.x, gridPos.y);
        isDead = false;

        Debug.Log($"Frog teleported to grid position ({gridPos.x}, {gridPos.y})");
    }

    public void ResetFrog()
    {
        int startX = gridManager.width / 2;
        int startY = 0;
        TeleportToGridPosition(new Vector2Int(startX, startY));
        SetFacing(Vector2Int.up);
        isDead = false;
    }
}