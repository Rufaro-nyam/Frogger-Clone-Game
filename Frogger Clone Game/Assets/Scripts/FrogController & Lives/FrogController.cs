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

    [Header("Blue Frog Power")]
    [SerializeField] private Color blueFrogColor = Color.blue;

    private bool hasBlueColor = false;
    private SpriteRenderer[] frogRenderers;
    private Color[] originalFrogColors;

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

    private Vector2 jumpStartLogPosition = Vector2.zero;

    // Cache the cell size
    private float cellSize = 1f;

    private bool isDead = false;

    [Header("Car Detection")]
    [SerializeField] private float detectionRadius = 0.4f;
    private Collider2D frogCollider;

    public Vector2Int CurrentGridPosition => currentGridPosition;
    public bool IsMoving => isMoving;

    public void SetBlueFrogColor()
    {
        hasBlueColor = true;

        if (frogRenderers == null)
        {
            frogRenderers =
                GetComponentsInChildren<SpriteRenderer>(true);

            originalFrogColors =
                new Color[frogRenderers.Length];

            for (int i = 0; i < frogRenderers.Length; i++)
            {
                originalFrogColors[i] =
                    frogRenderers[i].color;
            }
        }

        // Change ALL directional frog sprites to blue.
        foreach (SpriteRenderer renderer in frogRenderers)
        {
            if (renderer != null)
            {
                renderer.color = blueFrogColor;
            }
        }

        Debug.Log("Player frog is now BLUE.");
    }


    public void ResetFrogColor()
    {
        hasBlueColor = false;

        if (frogRenderers == null ||
            originalFrogColors == null)
        {
            return;
        }

        // Restore every directional sprite's original color.
        for (int i = 0;
             i < frogRenderers.Length;
             i++)
        {
            if (frogRenderers[i] != null)
            {
                frogRenderers[i].color =
                    originalFrogColors[i];
            }
        }

        Debug.Log("Player frog color restored.");
    }


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

        transform.position =
            gridManager.GetWorldPosition(startX, startY);

        SetFacing(Vector2Int.up);

        isDead = false;

        // Get or add collider
        frogCollider = GetComponent<Collider2D>();

        if (frogCollider == null)
        {
            BoxCollider2D boxCollider =
                gameObject.AddComponent<BoxCollider2D>();

            boxCollider.isTrigger = true;
            boxCollider.size = new Vector2(0.6f, 0.6f);

            frogCollider = boxCollider;
        }
        else
        {
            frogCollider.isTrigger = true;
        }

        Debug.Log(
            $"Frog initialized at grid position ({startX}, {startY})"
        );

        // Find every SpriteRenderer belonging to the frog,
        // including the different directional visuals.
        frogRenderers = GetComponentsInChildren<SpriteRenderer>(true);

        // Remember each renderer's original color.
        originalFrogColors = new Color[frogRenderers.Length];

        for (int i = 0; i < frogRenderers.Length; i++)
        {
            originalFrogColors[i] = frogRenderers[i].color;
        }
    }


    private void CalculateCellSize()
    {
        if (gridManager == null)
            return;

        Vector2 pos1 =
            gridManager.GetWorldPosition(0, 0);

        Vector2 pos2 =
            gridManager.GetWorldPosition(1, 0);

        cellSize =
            Vector2.Distance(pos1, pos2);

        Debug.Log(
            $"Cell size calculated: {cellSize}"
        );
    }


    private void Update()
    {
        // Check for car collisions every frame
        if (!isDead && !isMoving)
        {
            CheckForCarCollision();
        }

        // Don't process input if moving,
        // no gridManager, or game is over/won
        if (isMoving || gridManager == null || isDead)
            return;

        if (LivesManager.Instance != null)
        {
            if (LivesManager.Instance.IsGameOver ||
                LivesManager.Instance.HasWon)
            {
                return;
            }
        }

        Vector2Int direction = Vector2Int.zero;

        if (Input.GetKeyDown(KeyCode.W) ||
            Input.GetKeyDown(KeyCode.UpArrow))
        {
            direction = new Vector2Int(0, 1);
        }
        else if (Input.GetKeyDown(KeyCode.S) ||
                 Input.GetKeyDown(KeyCode.DownArrow))
        {
            direction = new Vector2Int(0, -1);
        }
        else if (Input.GetKeyDown(KeyCode.A) ||
                 Input.GetKeyDown(KeyCode.LeftArrow))
        {
            direction = new Vector2Int(-1, 0);
        }
        else if (Input.GetKeyDown(KeyCode.D) ||
                 Input.GetKeyDown(KeyCode.RightArrow))
        {
            direction = new Vector2Int(1, 0);
        }

        if (direction == Vector2Int.zero)
            return;

        Vector2Int newGridPosition =
            currentGridPosition + direction;


        if (isOnLog && currentLog != null)
        {
            // The frog's own jump starts from its CURRENT
            // world position.
            Vector2 targetWorldPos =
                (Vector2)transform.position +
                new Vector2(direction.x, direction.y) *
                cellSize;

            Vector2Int targetGrid =
                gridManager.GetGridPosition(targetWorldPos);


            if (targetGrid.x < 0 ||
                targetGrid.x >= gridManager.width)
            {
                HandleDeath(
                    "tried to move beyond the left/right edge of the board"
                );

                return;
            }



            if (targetGrid.y < 0 ||
                targetGrid.y >= gridManager.height)
            {
                Debug.Log(
                    "Cannot jump to position - out of bounds"
                );

                return;
            }


            targetGridPosition =
                targetGrid;

            startWorldPosition =
                transform.position;

            targetWorldPosition =
                targetWorldPos;

            // Remember exactly where the log was when
            // this jump started.
            jumpStartLogPosition =
                currentLog.position;

            // Keep tracking from this exact position.
            lastLogPosition =
                currentLog.position;

            SetFacing(direction);

            isMoving = true;
            moveTimer = 0f;

            if (jumpSound != null)
                jumpSound.Play();

            Debug.Log(
                $"Frog jumping ON LOG from world position " +
                $"({startWorldPosition.x}, " +
                $"{startWorldPosition.y}) to " +
                $"({targetWorldPos.x}, " +
                $"{targetWorldPos.y})"
            );
        }
        else
        {

            // Horizontal movement beyond the grid kills frog.
            if (direction.x != 0)
            {
                if (newGridPosition.x < 0 ||
                    newGridPosition.x >= gridManager.width)
                {
                    HandleDeath(
                        "tried to move beyond the left/right edge of the board"
                    );

                    return;
                }
            }

            if (IsValidGridPosition(newGridPosition))
            {
                StartJump(
                    newGridPosition,
                    direction
                );
            }
            else
            {
                Debug.Log(
                    $"Cannot move to grid position " +
                    $"({newGridPosition.x}, " +
                    $"{newGridPosition.y}) - out of bounds"
                );
            }
        }
    }


    private void CheckForCarCollision()
    {
        Collider2D[] hitColliders =
            Physics2D.OverlapCircleAll(
                transform.position,
                detectionRadius,
                vehicleLayerMask
            );

        foreach (Collider2D collider in hitColliders)
        {
            if (collider.CompareTag("Car") ||
                collider.CompareTag("Vehicle"))
            {
                Debug.Log(
                    $"Frog detected car nearby: {collider.name}"
                );

                HandleDeath("hit by a car");

                break;
            }
        }
    }


    private void OnTriggerEnter2D(Collider2D other)
    {
        if (isDead)
            return;

        if (LivesManager.Instance != null &&
            (LivesManager.Instance.IsGameOver ||
             LivesManager.Instance.HasWon))
        {
            return;
        }

        if (((1 << other.gameObject.layer) &
             vehicleLayerMask) != 0)
        {
            if (other.CompareTag("Car") ||
                other.CompareTag("Vehicle"))
            {
                Debug.Log(
                    $"Frog collided with {other.name}!"
                );

                HandleDeath("hit by a car");
            }
        }
    }


    private void StartJump(
        Vector2Int targetGridPos,
        Vector2Int direction)
    {
        targetGridPosition =
            targetGridPos;

        startWorldPosition =
            transform.position;

        targetWorldPosition =
            gridManager.GetWorldPosition(
                targetGridPos.x,
                targetGridPos.y
            );

        SetFacing(direction);

        isMoving = true;
        moveTimer = 0f;

        if (jumpSound != null)
            jumpSound.Play();

        Debug.Log(
            $"Frog jumping from " +
            $"({currentGridPosition.x}, " +
            $"{currentGridPosition.y}) to " +
            $"({targetGridPosition.x}, " +
            $"{targetGridPosition.y})"
        );
    }


    private void SetFacing(Vector2Int direction)
    {
        if (visualUp == null &&
            visualDown == null &&
            visualLeft == null &&
            visualRight == null)
        {
            return;
        }

        bool up =
            direction == Vector2Int.up;

        bool down =
            direction == Vector2Int.down;

        bool left =
            direction == Vector2Int.left;

        bool right =
            direction == Vector2Int.right;

        if (visualUp != null)
            visualUp.SetActive(up);

        if (visualDown != null)
            visualDown.SetActive(down);

        if (visualLeft != null)
            visualLeft.SetActive(left);

        if (visualRight != null)
            visualRight.SetActive(right);
    }


    private void LateUpdate()
    {
       

        if (isOnLog &&
            currentLog != null &&
            !isDead)
        {
            Vector2 currentLogPos =
                currentLog.position;


            if (!isMoving)
            {
                Vector2 logDelta =
                    currentLogPos -
                    lastLogPosition;

                transform.position +=
                    (Vector3)logDelta;

                lastLogPosition =
                    currentLogPos;


                // Check whether the log has carried the frog
                // beyond the board.
                CheckHorizontalBoundary();

                if (isDead)
                    return;
            }
        }


        if (!isMoving || isDead)
            return;

        moveTimer +=
            Time.deltaTime;

        float progress =
            Mathf.Clamp01(
                moveTimer / moveDuration
            );

        float easedProgress =
            progress *
            progress *
            (3f - 2f * progress);


        Vector2 currentPosition =
            Vector2.Lerp(
                startWorldPosition,
                targetWorldPosition,
                easedProgress
            );

        float arcHeight =
            Mathf.Sin(
                progress * Mathf.PI
            ) * 0.1f;

        currentPosition.y +=
            arcHeight;


        if (isOnLog &&
            currentLog != null)
        {
            Vector2 logMovementSinceJumpStarted =
                (Vector2)currentLog.position -
                jumpStartLogPosition;

            currentPosition +=
                logMovementSinceJumpStarted;
        }


        transform.position =
            currentPosition;

        CheckHorizontalBoundary();

        if (isDead)
            return;


        if (progress >= 1f)
        {
          
            transform.position =
                currentPosition;


            Vector2Int newGridPos =
                gridManager.GetGridPosition(
                    transform.position
                );

            if (newGridPos.x < 0 ||
                newGridPos.x >= gridManager.width)
            {
                HandleDeath(
                    "frog was carried beyond the grid while jumping"
                );

                return;
            }

            if (IsValidGridPosition(newGridPos))
            {
                currentGridPosition =
                    newGridPos;

                targetGridPosition =
                    newGridPos;
            }


            isMoving = false;

            CheckLandingTile();

            if (isOnLog &&
                currentLog != null)
            {
                lastLogPosition =
                    currentLog.position;
            }


            Debug.Log(
                $"Frog landed at grid position " +
                $"({currentGridPosition.x}, " +
                $"{currentGridPosition.y})"
            );
        }
    }

    private void CheckHorizontalBoundary()
    {
        if (gridManager == null ||
            isDead)
        {
            return;
        }

        float leftEdge =
            gridManager.GetWorldPosition(
                0,
                0
            ).x;

        float rightEdge =
            gridManager.GetWorldPosition(
                gridManager.width - 1,
                0
            ).x;


        if (frogCollider != null)
        {
            float frogLeft =
                frogCollider.bounds.min.x;

            float frogRight =
                frogCollider.bounds.max.x;


            if (frogRight < leftEdge ||
                frogLeft > rightEdge)
            {
                HandleDeath(
                    "frog went beyond the left/right edge of the grid"
                );
            }
        }
        else
        {
            // Fallback if the collider doesn't exist.
            if (transform.position.x < leftEdge ||
                transform.position.x > rightEdge)
            {
                HandleDeath(
                    "frog went beyond the left/right edge of the grid"
                );
            }
        }
    }


    private bool IsValidGridPosition(
        Vector2Int gridPos)
    {
        if (gridPos.x < 0 ||
            gridPos.x >= gridManager.width)
        {
            return false;
        }

        if (gridPos.y < 0 ||
            gridPos.y >= gridManager.height)
        {
            return false;
        }

        return true;
    }


    private void CheckLandingTile()
    {
        if (isDead)
            return;

        GridPositionType tileType =
            gridManager.GetPositionType(
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

                if (IsCarAtPosition(
                    currentGridPosition))
                {
                    HandleDeath(
                        "hit by a car"
                    );
                }

                break;


            case GridPositionType.River:

                if (IsLogAtPosition(
                    currentGridPosition))
                {
                    Debug.Log(
                        "Frog landed on a log! Safe for now..."
                    );

                    CheckIfOnLog();
                }
                else
                {
                    isOnLog = false;
                    currentLog = null;

                    HandleDeath(
                        "drowned in the river"
                    );
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
        Vector2 worldPos =
            transform.position;

        Collider2D[] hitColliders =
            Physics2D.OverlapCircleAll(
                worldPos,
                0.3f,
                logLayerMask
            );

        bool foundLog = false;

        foreach (Collider2D collider in hitColliders)
        {
            if (collider.CompareTag("Log"))
            {
                foundLog = true;

                currentLog =
                    collider.transform;

                isOnLog = true;

                lastLogPosition =
                    currentLog.position;

                Debug.Log(
                    "Frog is on a log!"
                );

                break;
            }
        }

        if (!foundLog)
        {
            isOnLog = false;
            currentLog = null;
        }
    }


    private bool IsCarAtPosition(
        Vector2Int gridPos)
    {
        Vector2 worldPos =
            gridManager.GetWorldPosition(
                gridPos.x,
                gridPos.y
            );

        Collider2D[] hitColliders =
            Physics2D.OverlapCircleAll(
                worldPos,
                0.3f,
                vehicleLayerMask
            );

        foreach (Collider2D collider in hitColliders)
        {
            if (collider.CompareTag("Car") ||
                collider.CompareTag("Vehicle"))
            {
                return true;
            }
        }

        return false;
    }


    private bool IsLogAtPosition(
        Vector2Int gridPos)
    {
        Vector2 worldPos =
            gridManager.GetWorldPosition(
                gridPos.x,
                gridPos.y
            );

        Collider2D[] hitColliders =
            Physics2D.OverlapCircleAll(
                worldPos,
                0.3f,
                logLayerMask
            );

        foreach (Collider2D collider in hitColliders)
        {
            if (collider.CompareTag("Log"))
            {
                return true;
            }
        }

        return false;
    }


    private void HandleDeath(
        string reason)
    {
        if (isDead)
            return;

        isDead = true;

        Debug.LogWarning(
            $"Frog died - {reason}"
        );

        isOnLog = false;
        currentLog = null;

        if (LivesManager.Instance != null)
        {
            LivesManager.Instance.LoseLife();
        }

        if (LivesManager.Instance != null &&
            !LivesManager.Instance.IsGameOver &&
            !LivesManager.Instance.HasWon)
        {
            RespawnAtStart();
        }
        else
        {
            Debug.Log(
                "Game Over - frog will not respawn"
            );
        }
    }


    private void HandleGoalReached()
    {
        if (isDead) return;
        // Return frog to its normal colour
        ResetFrogColor();

        if (goalManager == null)
        {
            Debug.LogError(
                "GoalManager is null!"
            );

            return;
        }

        bool filledNewSlot =
            goalManager.TryFillGoal(
                currentGridPosition
            );

        if (filledNewSlot)
        {
            Debug.Log(
                $"Frog reached an empty goal slot " +
                $"at ({currentGridPosition.x}, " +
                $"{currentGridPosition.y})!"
            );

            if (goalManager.FilledSlots >=
                goalManager.TotalSlots)
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

            if (LivesManager.Instance != null &&
                !LivesManager.Instance.IsGameOver &&
                !LivesManager.Instance.HasWon)
            {
                RespawnAtStart();
            }
        }
        else
        {
            Debug.LogWarning(
                $"Goal at ({currentGridPosition.x}, " +
                $"{currentGridPosition.y}) " +
                $"is already filled!"
            );

            HandleDeath(
                "landed on an already-filled goal slot"
            );
        }
    }


    private void RespawnAtStart()
    {
        if (LivesManager.Instance != null &&
            (LivesManager.Instance.IsGameOver ||
             LivesManager.Instance.HasWon))
        {
            Debug.Log(
                "Game is over or won - frog won't respawn"
            );

            return;
        }

        // Reset frog's colour when respawning after death
        ResetFrogColor();

        int startX =
            gridManager.width / 2;

        int startY = 0;

        TeleportToGridPosition(
            new Vector2Int(
                startX,
                startY
            )
        );

        SetFacing(
            Vector2Int.up
        );

        isDead = false;
    }


    public void TeleportToGridPosition(
        Vector2Int gridPos)
    {
        if (!IsValidGridPosition(gridPos))
        {
            Debug.LogWarning(
                $"Cannot teleport to invalid position " +
                $"({gridPos.x}, {gridPos.y})"
            );

            return;
        }

        isMoving = false;

        isOnLog = false;
        currentLog = null;

        currentGridPosition =
            gridPos;

        targetGridPosition =
            gridPos;

        transform.position =
            gridManager.GetWorldPosition(
                gridPos.x,
                gridPos.y
            );

        isDead = false;

        Debug.Log(
            $"Frog teleported to grid position " +
            $"({gridPos.x}, {gridPos.y})"
        );
    }


    public void ResetFrog()
    {
        int startX =
            gridManager.width / 2;

        int startY = 0;

        TeleportToGridPosition(
            new Vector2Int(
                startX,
                startY
            )
        );

        SetFacing(
            Vector2Int.up
        );

        isDead = false;
    }
}