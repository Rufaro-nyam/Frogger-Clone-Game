using UnityEngine;
using static GridPositionTypes;
using static LaneTypes;

public class ExtraLifeFrog : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private GridManager gridManager;
    [SerializeField] private FrogController player;

    [Header("Log Selection")]
    [SerializeField] private GameObject targetLog;
    [SerializeField] private Vector2 offsetFromLog = Vector2.zero;

    [Header("Merge Detection")]
    [SerializeField] private float mergeDistance = 0.5f;

    [Header("Visual")]
    [SerializeField] private GameObject visual;

    [Header("Audio")]
    [SerializeField] private AudioSource collectSound;

    private Vector2 lastLogPosition = Vector2.zero;
    private bool isOnLog = false;
    private bool collected = false;
    private SpriteRenderer spriteRenderer;

    public bool IsCollected() => collected;

    private void Awake()
    {
       
        if (gridManager == null)
        {
            gridManager = FindObjectOfType<GridManager>();
        }

        if (player == null)
        {
            player = FindObjectOfType<FrogController>();
        }

        spriteRenderer = GetComponent<SpriteRenderer>();
    }

    private void Start()
    {
        if (targetLog != null)
        {
            SnapToLog(targetLog);
        }

        ShowVisual(true);
    }

    private void Update()
    {
        if (collected)
        {
            return;
        }

        // Follow the log smoothly
        if (isOnLog && targetLog != null)
        {
            Vector2 currentLogPos = targetLog.transform.position;
            Vector2 logDelta = currentLogPos - lastLogPosition;

            // Move with the log
            transform.position += (Vector3)logDelta;

            lastLogPosition = currentLogPos;
        }

        CheckMergeWithPlayer();
    }

    private void SnapToLog(GameObject log)
    {
        if (log == null) return;

        // Defensive fallback in case Awake() somehow hasn't run yet
        // (e.g. this method gets called from another script's Awake()).
        if (gridManager == null)
        {
            gridManager = FindObjectOfType<GridManager>();
            if (gridManager == null)
            {
                Debug.LogError("ExtraLifeFrog: No GridManager found — cannot snap to log.");
                return;
            }
        }

        targetLog = log;
        isOnLog = true;

        // Snap to the log's position
        Vector2Int gridPos = gridManager.GetGridPosition(log.transform.position);
        Vector2 exactPos = gridManager.GetWorldPosition(gridPos.x, gridPos.y);
        transform.position = exactPos + offsetFromLog;

        // Store initial log position for delta tracking
        lastLogPosition = log.transform.position;

        Debug.Log($"ExtraLifeFrog: Snapped to log");
    }

    private void CheckMergeWithPlayer()
    {
        if (player == null || collected || player.IsMoving) return;

        float distance = Vector2.Distance(transform.position, player.transform.position);

        if (distance <= mergeDistance)
        {
            Collect();
        }
    }

    private void Collect()
    {
        if (collected) return;

        collected = true;
        isOnLog = false;

        // Change the player's frog to blue
        if (player != null)
        {
            player.SetBlueFrogColor();
        }

        if (LivesManager.Instance != null)
        {
            LivesManager.Instance.AddLife(1);
            Debug.Log("ExtraLifeFrog collected - +1 life!");
        }

        if (collectSound != null)
        {
            collectSound.Play();
        }

        ShowVisual(false);

        Debug.Log("ExtraLifeFrog collected! Player is now blue.");

        Destroy(gameObject, 0.5f);
    }

    private void ShowVisual(bool show)
    {
        if (visual != null)
        {
            visual.SetActive(show);
            return;
        }

        if (spriteRenderer != null)
        {
            spriteRenderer.enabled = show;
            return;
        }

        SpriteRenderer childSR = GetComponentInChildren<SpriteRenderer>();
        if (childSR != null)
        {
            childSR.enabled = show;
        }
    }

    public void SetLog(GameObject log)
    {
        if (log == null) return;

        if (!gameObject.activeSelf)
        {
            gameObject.SetActive(true);
        }

        SnapToLog(log);
        collected = false;
        ShowVisual(true);
    }
}