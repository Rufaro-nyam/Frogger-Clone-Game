using UnityEngine;

public class ExtraLifeFrog : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private GridManager gridManager;
    [SerializeField] private FrogController player;

    [Header("Row Settings")]
    [Tooltip("Which grid row (river lane) this frog drifts across. Leave at -1 to auto-pick the first River lane.")]
    [SerializeField] private int riverRow = -1;

    [Header("Movement")]
    [SerializeField] private float moveSpeed = 1.5f;
    [Tooltip("True = drifts left to right, False = right to left.")]
    [SerializeField] private bool moveRight = true;

    [Header("Merge Detection")]
    [Tooltip("How close (world units) the player frog needs to be to merge with this one.")]
    [SerializeField] private float mergeDistance = 0.4f;

    [Header("Respawn Timing")]
    [Tooltip("Seconds to wait, hidden, before drifting back across again after being collected.")]
    [SerializeField] private float respawnDelay = 8f;

    [Header("Visual")]
    [Tooltip("Child object representing the visible frog. Hidden while waiting to respawn.")]
    [SerializeField] private GameObject visual;

    private float leftEdgeX;
    private float rightEdgeX;
    private float rowY;
    private bool collected = false;
    private float respawnTimer = 0f;

    private void Start()
    {
        if (gridManager == null)
        {
            gridManager = FindObjectOfType<GridManager>();
        }

        if (player == null)
        {
            player = FindObjectOfType<FrogController>();
        }

        if (gridManager == null)
        {
            Debug.LogError("ExtraLifeFrog: no GridManager found in the scene!");
            enabled = false;
            return;
        }

        if (riverRow < 0)
        {
            riverRow = gridManager.GetFirstRiverRow();

            if (riverRow < 0)
            {
                Debug.LogWarning("ExtraLifeFrog: no River lane configured, defaulting to row 0.");
                riverRow = 0;
            }
        }

        Vector2 leftEdge = gridManager.GetWorldPosition(0, riverRow);
        Vector2 rightEdge = gridManager.GetWorldPosition(gridManager.width - 1, riverRow);

        leftEdgeX = leftEdge.x;
        rightEdgeX = rightEdge.x;
        rowY = leftEdge.y;

        transform.position = new Vector2(moveRight ? leftEdgeX : rightEdgeX, rowY);
    }

    private void Update()
    {
        if (collected)
        {
            HandleRespawnCountdown();
            return;
        }

        Drift();
        CheckMergeWithPlayer();
    }

    private void Drift()
    {
        float direction = moveRight ? 1f : -1f;
        Vector3 position = transform.position;
        position.x += direction * moveSpeed * Time.deltaTime;

        // Wrap around to the opposite edge, like a log carrying it across.
        if (moveRight && position.x > rightEdgeX)
        {
            position.x = leftEdgeX;
        }
        else if (!moveRight && position.x < leftEdgeX)
        {
            position.x = rightEdgeX;
        }

        position.y = rowY;
        transform.position = position;
    }

    private void CheckMergeWithPlayer()
    {
        if (player == null)
        {
            return;
        }

        float distance = Vector2.Distance(transform.position, player.transform.position);

        if (distance <= mergeDistance)
        {
            Collect();
        }
    }

    private void Collect()
    {
        collected = true;
        respawnTimer = 0f;

        if (LivesManager.Instance != null)
        {
            LivesManager.Instance.AddLife(1);
        }

        if (visual != null)
        {
            visual.SetActive(false);
        }

        Debug.Log("ExtraLifeFrog collected - life refunded (capped at starting lives)");
    }

    private void HandleRespawnCountdown()
    {
        respawnTimer += Time.deltaTime;

        if (respawnTimer < respawnDelay)
        {
            return;
        }

        // Send it back to its starting edge and let it drift again.
        transform.position = new Vector2(moveRight ? leftEdgeX : rightEdgeX, rowY);
        collected = false;

        if (visual != null)
        {
            visual.SetActive(true);
        }
    }
}