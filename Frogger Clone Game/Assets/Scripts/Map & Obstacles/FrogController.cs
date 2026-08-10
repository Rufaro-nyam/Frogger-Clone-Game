using UnityEngine;
using static GridPositionTypes;
using static LaneTypes;

public class FrogController : MonoBehaviour
{
    [Header("Movement Settings")]
    [SerializeField] private float moveDuration = 0.2f; // Time for each jump
    [SerializeField] private AudioSource jumpSound; // Optional jump sound

    [Header("Facing Visuals")]
    [Tooltip("Child GameObject pre-rotated to face Up. Assign in the Inspector.")]
    [SerializeField] private GameObject visualUp;
    [Tooltip("Child GameObject pre-rotated to face Down. Assign in the Inspector.")]
    [SerializeField] private GameObject visualDown;
    [Tooltip("Child GameObject pre-rotated to face Left. Assign in the Inspector.")]
    [SerializeField] private GameObject visualLeft;
    [Tooltip("Child GameObject pre-rotated to face Right. Assign in the Inspector.")]
    [SerializeField] private GameObject visualRight;

    private GridManager gridManager;
    private Vector2Int currentGridPosition;
    private Vector2Int targetGridPosition;
    private bool isMoving = false;
    private float moveTimer = 0f;
    private Vector2 startWorldPosition;
    private Vector2 targetWorldPosition;

    // Public properties to access frog's state
    public Vector2Int CurrentGridPosition => currentGridPosition;
    public bool IsMoving => isMoving;

    private void Start()
    {
        // Find the GridManager in the scene
        gridManager = FindObjectOfType<GridManager>();

        if (gridManager == null)
        {
            Debug.LogError("GridManager not found in the scene!");
            return;
        }

        // Initialize frog at the bottom-middle of the grid
        int startX = gridManager.width / 2;
        int startY = 0; // Bottom row

        // Set initial position
        currentGridPosition = new Vector2Int(startX, startY);
        targetGridPosition = currentGridPosition;

        // Move frog to starting position immediately
        transform.position = gridManager.GetWorldPosition(startX, startY);

        // Frog starts facing "up"
        SetFacing(Vector2Int.up);

        Debug.Log($"Frog initialized at grid position ({startX}, {startY})");
    }

    private void Update()
    {
        // Don't process input while moving or if gridManager is null
        if (isMoving || gridManager == null)
            return;

        // Get input for movement
        Vector2Int direction = Vector2Int.zero;

        if (Input.GetKeyDown(KeyCode.W) || Input.GetKeyDown(KeyCode.UpArrow))
        {
            direction = new Vector2Int(0, 1); // Up
        }
        else if (Input.GetKeyDown(KeyCode.S) || Input.GetKeyDown(KeyCode.DownArrow))
        {
            direction = new Vector2Int(0, -1); // Down
        }
        else if (Input.GetKeyDown(KeyCode.A) || Input.GetKeyDown(KeyCode.LeftArrow))
        {
            direction = new Vector2Int(-1, 0); // Left
        }
        else if (Input.GetKeyDown(KeyCode.D) || Input.GetKeyDown(KeyCode.RightArrow))
        {
            direction = new Vector2Int(1, 0); // Right
        }

        // If no direction was pressed, return
        if (direction == Vector2Int.zero)
            return;

        // Calculate target grid position
        Vector2Int newPosition = currentGridPosition + direction;

        // Check if the new position is within grid bounds
        if (IsValidGridPosition(newPosition))
        {
            // Start the jump animation
            StartJump(newPosition, direction);
        }
        else
        {
            Debug.Log($"Cannot move to grid position ({newPosition.x}, {newPosition.y}) - out of bounds");
            // Could play a blocked sound effect here
        }
    }

    private void StartJump(Vector2Int targetGridPos, Vector2Int direction)
    {
        // Set up the jump animation
        targetGridPosition = targetGridPos;
        startWorldPosition = transform.position;
        targetWorldPosition = gridManager.GetWorldPosition(targetGridPos.x, targetGridPos.y);

        // Instantly switch to the visual that faces the direction we're
        // jumping toward - no interpolation, just a clean swap.
        SetFacing(direction);

        isMoving = true;
        moveTimer = 0f;

        // Play jump sound if available
        if (jumpSound != null)
        {
            jumpSound.Play();
        }

        Debug.Log($"Frog jumping from ({currentGridPosition.x}, {currentGridPosition.y}) to ({targetGridPosition.x}, {targetGridPosition.y})");
    }

    // Enables the child visual matching the given direction and disables
    // the other three. Each visual is a duplicate of the frog square,
    // pre-rotated in the Editor to face that direction.
    private void SetFacing(Vector2Int direction)
    {
        if (visualUp == null && visualDown == null && visualLeft == null && visualRight == null)
        {
            // No visuals assigned - nothing to toggle, skip silently.
            return;
        }

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
        // Handle the jumping animation
        if (!isMoving)
            return;

        // Advance the timer
        moveTimer += Time.deltaTime;

        // Calculate the progress (0 to 1) with easing
        float progress = Mathf.Clamp01(moveTimer / moveDuration);

        // Apply easing for a more natural jump (ease-in-out)
        float easedProgress = progress * progress * (3f - 2f * progress);

        // Interpolate position
        Vector2 currentPosition = Vector2.Lerp(startWorldPosition, targetWorldPosition, easedProgress);

        // Add a small arc to the jump (parabolic)
        float arcHeight = Mathf.Sin(progress * Mathf.PI) * 0.3f;
        currentPosition.y += arcHeight;

        transform.position = currentPosition;

        // Check if the jump is complete
        if (progress >= 1f)
        {
            // Snap to exact position
            transform.position = targetWorldPosition;

            // Update current grid position
            currentGridPosition = targetGridPosition;

            // Reset movement state
            isMoving = false;

            // Check what type of tile the frog landed on
            CheckLandingTile();

            Debug.Log($"Frog landed at grid position ({currentGridPosition.x}, {currentGridPosition.y})");
        }
    }

    private bool IsValidGridPosition(Vector2Int gridPos)
    {
        // Check if the position is within grid bounds
        if (gridPos.x < 0 || gridPos.x >= gridManager.width)
            return false;

        if (gridPos.y < 0 || gridPos.y >= gridManager.height)
            return false;

        return true;
    }

    private void CheckLandingTile()
    {
        // Check what type of tile the frog is on
        GridPositionType tileType = gridManager.GetPositionType(
            currentGridPosition.x,
            currentGridPosition.y
        );

        LaneType laneType = gridManager.GetLaneType(currentGridPosition.y);

        // Handle different tile types
        switch (tileType)
        {
            case GridPositionType.Safe:
                Debug.Log("Frog landed on safe tile!");
                break;

            case GridPositionType.Road:
                Debug.LogWarning("Frog landed on road tile - should get hit by car!");
                // You can implement death logic here
                break;

            case GridPositionType.River:
                Debug.LogWarning("Frog landed in water - should drown!");
                // You can implement death logic here
                break;

            case GridPositionType.Goal:
                Debug.Log("Frog reached the goal!");
                // You can implement goal reached logic here
                break;
        }

        // Lane type handling
        if (laneType == LaneType.Safe)
        {
            Debug.Log("Frog is in a safe lane");
        }
        else if (laneType == LaneType.Road)
        {
            Debug.Log("Frog is in a road lane");
        }
        else if (laneType == LaneType.River)
        {
            Debug.Log("Frog is in a river lane");
        }
    }

    // Public method to manually teleport the frog (useful for resetting)
    public void TeleportToGridPosition(Vector2Int gridPos)
    {
        if (!IsValidGridPosition(gridPos))
        {
            Debug.LogWarning($"Cannot teleport to invalid position ({gridPos.x}, {gridPos.y})");
            return;
        }

        // Cancel any current movement
        isMoving = false;

        // Update positions
        currentGridPosition = gridPos;
        targetGridPosition = gridPos;

        // Teleport
        transform.position = gridManager.GetWorldPosition(gridPos.x, gridPos.y);

        Debug.Log($"Frog teleported to grid position ({gridPos.x}, {gridPos.y})");
    }

    // Public method to reset frog to starting position
    public void ResetFrog()
    {
        int startX = gridManager.width / 2;
        int startY = 0;
        TeleportToGridPosition(new Vector2Int(startX, startY));

        // Reset facing direction back to "up"
        SetFacing(Vector2Int.up);
    }
}