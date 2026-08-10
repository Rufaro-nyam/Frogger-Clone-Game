//using UnityEngine;
//using static GridPositionTypes;
//using static LaneTypes;

//public class FrogController : MonoBehaviour
//{
//    [Header("Movement Settings")]
//    [SerializeField] private float moveDuration = 0.2f; 
//    [SerializeField] private AudioSource jumpSound; 

//    [Header("Facing Visuals")]
//    [Tooltip("Child GameObject pre-rotated to face Up. Assign in the Inspector.")]
//    [SerializeField] private GameObject visualUp;
//    [Tooltip("Child GameObject pre-rotated to face Down. Assign in the Inspector.")]
//    [SerializeField] private GameObject visualDown;
//    [Tooltip("Child GameObject pre-rotated to face Left. Assign in the Inspector.")]
//    [SerializeField] private GameObject visualLeft;
//    [Tooltip("Child GameObject pre-rotated to face Right. Assign in the Inspector.")]
//    [SerializeField] private GameObject visualRight;

//    private GridManager gridManager;
//    private Vector2Int currentGridPosition;
//    private Vector2Int targetGridPosition;
//    private bool isMoving = false;
//    private float moveTimer = 0f;
//    private Vector2 startWorldPosition;
//    private Vector2 targetWorldPosition;

//    // Public properties to access frog's state
//    public Vector2Int CurrentGridPosition => currentGridPosition;
//    public bool IsMoving => isMoving;

//    private void Start()
//    {
//        // Find the GridManager in the scene
//        gridManager = FindObjectOfType<GridManager>();

//        if (gridManager == null)
//        {
//            Debug.LogError("GridManager not found in the scene!");
//            return;
//        }

//        // Initialize frog at the bottom-middle of the grid
//        int startX = gridManager.width / 2;
//        int startY = 0; // Bottom row

//        // Set initial position
//        currentGridPosition = new Vector2Int(startX, startY);
//        targetGridPosition = currentGridPosition;

//        // Move frog to starting position immediately
//        transform.position = gridManager.GetWorldPosition(startX, startY);

//        // Frog starts facing "up"
//        SetFacing(Vector2Int.up);

//        Debug.Log($"Frog initialized at grid position ({startX}, {startY})");
//    }

//    private void Update()
//    {
//        // Don't process input while moving or if gridManager is null
//        if (isMoving || gridManager == null)
//            return;

//        // Get input for movement
//        Vector2Int direction = Vector2Int.zero;

//        if (Input.GetKeyDown(KeyCode.W) || Input.GetKeyDown(KeyCode.UpArrow))
//        {
//            direction = new Vector2Int(0, 1); 
//        }
//        else if (Input.GetKeyDown(KeyCode.S) || Input.GetKeyDown(KeyCode.DownArrow))
//        {
//            direction = new Vector2Int(0, -1); 
//        }
//        else if (Input.GetKeyDown(KeyCode.A) || Input.GetKeyDown(KeyCode.LeftArrow))
//        {
//            direction = new Vector2Int(-1, 0); 
//        }
//        else if (Input.GetKeyDown(KeyCode.D) || Input.GetKeyDown(KeyCode.RightArrow))
//        {
//            direction = new Vector2Int(1, 0); 
//        }

//        // If no direction was pressed, return
//        if (direction == Vector2Int.zero)
//            return;

//        // Calculate target grid position
//        Vector2Int newPosition = currentGridPosition + direction;

//        // Check if the new position is within grid bounds
//        if (IsValidGridPosition(newPosition))
//        {
//            // Start the jump animation
//            StartJump(newPosition, direction);
//        }
//        else
//        {
//            Debug.Log($"Cannot move to grid position ({newPosition.x}, {newPosition.y}) - out of bounds");
//            // Could play a blocked sound effect here
//        }
//    }

//    private void StartJump(Vector2Int targetGridPos, Vector2Int direction)
//    {
//        // Set up the jump animation
//        targetGridPosition = targetGridPos;
//        startWorldPosition = transform.position;
//        targetWorldPosition = gridManager.GetWorldPosition(targetGridPos.x, targetGridPos.y);


//        SetFacing(direction);

//        isMoving = true;
//        moveTimer = 0f;

//        if (jumpSound != null)
//        {
//            jumpSound.Play();
//        }

//        Debug.Log($"Frog jumping from ({currentGridPosition.x}, {currentGridPosition.y}) to ({targetGridPosition.x}, {targetGridPosition.y})");
//    }

//    // Enables the child visual matching the given direction and disables
//    // the other three. Each visual is a duplicate of the frog square,
//    // pre-rotated in the Editor to face that direction.
//    private void SetFacing(Vector2Int direction)
//    {
//        if (visualUp == null && visualDown == null && visualLeft == null && visualRight == null)
//        {
//            // No visuals assigned - nothing to toggle, skip silently.
//            return;
//        }

//        bool up = direction == Vector2Int.up;
//        bool down = direction == Vector2Int.down;
//        bool left = direction == Vector2Int.left;
//        bool right = direction == Vector2Int.right;

//        if (visualUp != null) visualUp.SetActive(up);
//        if (visualDown != null) visualDown.SetActive(down);
//        if (visualLeft != null) visualLeft.SetActive(left);
//        if (visualRight != null) visualRight.SetActive(right);
//    }

//    private void LateUpdate()
//    {
//        // Handle the jumping animation
//        if (!isMoving)
//            return;

//        // Advance the timer
//        moveTimer += Time.deltaTime;

//        // Calculate the progress (0 to 1) with easing
//        float progress = Mathf.Clamp01(moveTimer / moveDuration);

//        // Apply easing for a more natural jump (ease-in-out)
//        float easedProgress = progress * progress * (3f - 2f * progress);

//        // Interpolate position
//        Vector2 currentPosition = Vector2.Lerp(startWorldPosition, targetWorldPosition, easedProgress);

//        // Add a small arc to the jump (parabolic)
//        float arcHeight = Mathf.Sin(progress * Mathf.PI) * 0.3f;
//        currentPosition.y += arcHeight;

//        transform.position = currentPosition;

//        // Check if the jump is complete
//        if (progress >= 1f)
//        {
//            // Snap to exact position
//            transform.position = targetWorldPosition;

//            // Update current grid position
//            currentGridPosition = targetGridPosition;

//            // Reset movement state
//            isMoving = false;

//            // Check what type of tile the frog landed on
//            CheckLandingTile();

//            Debug.Log($"Frog landed at grid position ({currentGridPosition.x}, {currentGridPosition.y})");
//        }
//    }

//    private bool IsValidGridPosition(Vector2Int gridPos)
//    {
//        // Check if the position is within grid bounds
//        if (gridPos.x < 0 || gridPos.x >= gridManager.width)
//            return false;

//        if (gridPos.y < 0 || gridPos.y >= gridManager.height)
//            return false;

//        return true;
//    }

//    private void CheckLandingTile()
//    {
//        // Check what type of tile the frog is on
//        GridPositionType tileType = gridManager.GetPositionType(
//            currentGridPosition.x,
//            currentGridPosition.y
//        );

//        LaneType laneType = gridManager.GetLaneType(currentGridPosition.y);

//        // Handle different tile types
//        switch (tileType)
//        {
//            case GridPositionType.Safe:
//                Debug.Log("Frog landed on safe tile!");
//                break;

//            case GridPositionType.Road:
//                Debug.LogWarning("Frog landed on road tile - should get hit by car!");

//                break;

//            case GridPositionType.River:
//                Debug.LogWarning("Frog landed in water - should drown!");

//                break;

//            case GridPositionType.Goal:
//                Debug.Log("Frog reached the goal!");

//                break;
//        }

//        // Lane type handling
//        if (laneType == LaneType.Safe)
//        {
//            Debug.Log("Frog is in a safe lane");
//        }
//        else if (laneType == LaneType.Road)
//        {
//            Debug.Log("Frog is in a road lane");
//        }
//        else if (laneType == LaneType.River)
//        {
//            Debug.Log("Frog is in a river lane");
//        }
//    }

//    // Public method to manually teleport the frog (useful for resetting)
//    public void TeleportToGridPosition(Vector2Int gridPos)
//    {
//        if (!IsValidGridPosition(gridPos))
//        {
//            Debug.LogWarning($"Cannot teleport to invalid position ({gridPos.x}, {gridPos.y})");
//            return;
//        }

//        // Cancel any current movement
//        isMoving = false;

//        // Update positions
//        currentGridPosition = gridPos;
//        targetGridPosition = gridPos;

//        // Teleport
//        transform.position = gridManager.GetWorldPosition(gridPos.x, gridPos.y);

//        Debug.Log($"Frog teleported to grid position ({gridPos.x}, {gridPos.y})");
//    }

//    // Public method to reset frog to starting position
//    public void ResetFrog()
//    {
//        int startX = gridManager.width / 2;
//        int startY = 0;
//        TeleportToGridPosition(new Vector2Int(startX, startY));

//        // Reset facing direction back to "up"
//        SetFacing(Vector2Int.up);
//    }
//}
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

        Debug.Log($"Frog initialized at grid position ({startX}, {startY})");
    }

    private void Update()
    {
        // Don't process input if moving, no gridManager, or game is over/won
        if (isMoving || gridManager == null)
            return;

        if (LivesManager.Instance != null)
        {
            // Block input if game is over OR player has won
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

        Vector2Int newPosition = currentGridPosition + direction;

        if (IsValidGridPosition(newPosition))
        {
            StartJump(newPosition, direction);
        }
        else
        {
            Debug.Log($"Cannot move to grid position ({newPosition.x}, {newPosition.y}) - out of bounds");
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
        if (!isMoving)
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
            currentGridPosition = targetGridPosition;
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
        GridPositionType tileType = gridManager.GetPositionType(
            currentGridPosition.x,
            currentGridPosition.y
        );

        switch (tileType)
        {
            case GridPositionType.Safe:
                break;

            case GridPositionType.Road:
                if (IsCarAtPosition(currentGridPosition))
                    HandleDeath("hit by a car");
                break;

            case GridPositionType.River:
                if (IsLogAtPosition(currentGridPosition))
                    Debug.Log("Frog landed on a log! Safe for now...");
                else
                    HandleDeath("drowned in the river");
                break;

            case GridPositionType.Goal:
                HandleGoalReached();
                break;
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
        Debug.LogWarning($"Frog died - {reason}");

        if (LivesManager.Instance != null)
        {
            LivesManager.Instance.LoseLife();
        }

        // Only respawn if game is NOT over and player hasn't won
        if (LivesManager.Instance != null && !LivesManager.Instance.IsGameOver && !LivesManager.Instance.HasWon)
        {
            RespawnAtStart();
        }
        else
        {
            Debug.Log("Game Over or Won - frog will not respawn");
        }
    }

    private void HandleGoalReached()
    {
        if (goalManager == null)
        {
            Debug.LogError("GoalManager is null!");
            return;
        }

        // Try to fill the goal
        bool filledNewSlot = goalManager.TryFillGoal(currentGridPosition);

        if (filledNewSlot)
        {
            Debug.Log($"Frog reached an empty goal slot at ({currentGridPosition.x}, {currentGridPosition.y})!");

            // CHECK WIN CONDITION FIRST - before losing a life!
            if (goalManager.FilledSlots >= goalManager.TotalSlots)
            {
                Debug.Log("ALL GOALS FILLED - YOU WIN! ");

                // Tell LivesManager that player won (this prevents game over)
                if (LivesManager.Instance != null)
                {
                    LivesManager.Instance.GameWon();
                }

                // Don't lose a life or respawn - game is won!
                return;
            }

            // Not all goals filled yet - lose a life and respawn
            if (LivesManager.Instance != null)
            {
                LivesManager.Instance.LoseLife();
            }

            // Only respawn if game is NOT over and player hasn't won
            if (LivesManager.Instance != null && !LivesManager.Instance.IsGameOver && !LivesManager.Instance.HasWon)
            {
                RespawnAtStart();
            }
        }
        else
        {
            // Goal already filled - treat as death
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
    }

    public void TeleportToGridPosition(Vector2Int gridPos)
    {
        if (!IsValidGridPosition(gridPos))
        {
            Debug.LogWarning($"Cannot teleport to invalid position ({gridPos.x}, {gridPos.y})");
            return;
        }

        isMoving = false;
        currentGridPosition = gridPos;
        targetGridPosition = gridPos;
        transform.position = gridManager.GetWorldPosition(gridPos.x, gridPos.y);

        Debug.Log($"Frog teleported to grid position ({gridPos.x}, {gridPos.y})");
    }

    public void ResetFrog()
    {
        int startX = gridManager.width / 2;
        int startY = 0;
        TeleportToGridPosition(new Vector2Int(startX, startY));
        SetFacing(Vector2Int.up);
    }
}