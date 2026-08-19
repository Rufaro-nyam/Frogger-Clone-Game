using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class TimeScoreManager : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private Image timeBarImage;
    [SerializeField] private TextMeshProUGUI scoreText;

    [Tooltip("Text component to display the numerical count of frogs that reached the end during gameplay.")]
    [SerializeField] private TextMeshProUGUI frogsCountText;

    [Header("End Game UI References")]
    [Tooltip("UI canvas/panel to enable if the player finishes with a score > 0.")]
    [SerializeField] private GameObject winCanvasUI;

    [Tooltip("Text component inside the Win Canvas to display only the numerical score.")]
    [SerializeField] private TextMeshProUGUI winScoreText;

    [Tooltip("Text component inside the Win Canvas to display only the numerical count of rescued frogs.")]
    [SerializeField] private TextMeshProUGUI winFrogsCountText;

    [Tooltip("UI canvas/panel to enable if the player finishes with 0 score.")]
    [SerializeField] private GameObject gameOverUI;

    [Header("Game References")]
    [SerializeField] private FrogController frogController;
    [SerializeField] private GoalManager goalManager;

    [Header("Timer Settings")]
    [Tooltip("Total duration in seconds for the round.")]
    [SerializeField] private float maxTime = 30f;

    [Tooltip("Gradient for timer color (Key at 1.0 = full time, Key at 0.0 = zero time).")]
    [SerializeField] private Gradient timeBarGradient;

    [Header("Scoring Settings")]
    [Tooltip("Base score awarded for reaching the end goal.")]
    [SerializeField] private int baseGoalScore = 1000;

    [Tooltip("Multiplier applied to remaining seconds to convert into bonus points (e.g., 50 pts per second remaining).")]
    [SerializeField] private int pointsPerRemainingSecond = 50;

    private float currentTime;
    private bool isTimerRunning = false;
    private int currentTotalScore = 0;

    // Frog tracking
    private int frogsReachedCount = 0;

    // State tracking variables
    private int lastFilledSlots = 0;
    private Vector2Int lastFrogPosition;
    private bool isGameEndHandled = false;

    public int CurrentTotalScore => currentTotalScore;
    public int FrogsReachedCount => frogsReachedCount;

    private void Start()
    {
        // Ensure end-game UI screens start inactive
        if (winCanvasUI != null) winCanvasUI.SetActive(false);
        if (gameOverUI != null) gameOverUI.SetActive(false);

        // Auto-find references if not set in Inspector
        if (frogController == null)
            frogController = FindObjectOfType<FrogController>();

        if (goalManager == null)
            goalManager = FindObjectOfType<GoalManager>();

        if (goalManager != null)
            lastFilledSlots = goalManager.FilledSlots;

        if (frogController != null)
            lastFrogPosition = frogController.CurrentGridPosition;

        UpdateScoreUI();
        UpdateFrogsUI();
        ResetTimer();
    }

    private void Update()
    {
        // Debug key for goal testing
        if (Input.GetKeyDown(KeyCode.R))
        {
            ReachGoal();
        }

        // Process pending events FIRST (so the 5th goal increment happens before handling win UI)
        DetectPlayerEvents();

        // Check if the game state is over (Win or Game Over)
        if (IsGameFinished())
        {
            if (isTimerRunning)
            {
                StopTimer();
                DisableTimeBar();
            }

            if (!isGameEndHandled)
            {
                HandleEndGameUI();
            }

            return; // Halt timer processing completely
        }

        if (!isTimerRunning) return;

        currentTime -= Time.deltaTime;

        if (currentTime <= 0f)
        {
            currentTime = 0f;
            isTimerRunning = false;
            OnTimeExpired();
        }

        UpdateTimerUI();
    }

    /// <summary>
    /// Evaluates score condition upon game finish and activates the appropriate UI panel.
    /// </summary>
    private void HandleEndGameUI()
    {
        isGameEndHandled = true;

        if (currentTotalScore > 0)
        {
            // Player scored something -> Enable Win Canvas and display numerical values
            if (winCanvasUI != null)
            {
                winCanvasUI.SetActive(true);
            }

            if (winScoreText != null)
            {
                winScoreText.text = currentTotalScore.ToString();
            }

            if (winFrogsCountText != null)
            {
                winFrogsCountText.text = frogsReachedCount.ToString();
            }

            if (gameOverUI != null)
            {
                gameOverUI.SetActive(false);
            }

            Debug.Log($"TimeScoreManager: Game finished with {currentTotalScore} points and {frogsReachedCount} frogs. Enabling Win Canvas UI.");
        }
        else
        {
            // Player scored nothing -> Enable Game Over UI
            if (gameOverUI != null)
            {
                gameOverUI.SetActive(true);
            }

            if (winCanvasUI != null)
            {
                winCanvasUI.SetActive(false);
            }

            Debug.Log("TimeScoreManager: Game finished with 0 points. Enabling Game Over UI.");
        }
    }

    /// <summary>
    /// Returns true if either a Game Over or Win condition has been met in LivesManager.
    /// </summary>
    private bool IsGameFinished()
    {
        if (LivesManager.Instance != null)
        {
            return LivesManager.Instance.IsGameOver || LivesManager.Instance.HasWon;
        }
        return false;
    }

    /// <summary>
    /// Disables the time bar visual UI component when the game ends.
    /// </summary>
    private void DisableTimeBar()
    {
        if (timeBarImage != null && timeBarImage.gameObject.activeSelf)
        {
            timeBarImage.gameObject.SetActive(false);
            Debug.Log("TimeScoreManager: Game finished. Time bar UI disabled.");
        }
    }

    /// <summary>
    /// Monitors GoalManager slots and FrogController position for goals and deaths.
    /// </summary>
    private void DetectPlayerEvents()
    {
        // Goal Reached Detection
        if (goalManager != null)
        {
            if (goalManager.FilledSlots > lastFilledSlots)
            {
                lastFilledSlots = goalManager.FilledSlots;
                ReachGoal();

                if (frogController != null)
                    lastFrogPosition = frogController.CurrentGridPosition;

                return;
            }
        }

        // Death / Respawn Detection
        if (frogController != null)
        {
            Vector2Int currentPos = frogController.CurrentGridPosition;

            bool respawnedAtStart = (currentPos.y == 0 && lastFrogPosition.y > 0 && !frogController.IsMoving);

            if (respawnedAtStart && !IsGameFinished())
            {
                Debug.Log("TimeScoreManager: Player death detected. Resetting timer.");
                ResetTimer();
            }

            lastFrogPosition = currentPos;
        }
    }

    /// <summary>
    /// Awards score for reaching a goal slot, increments the rescued frog count, and resets the timer for the next run.
    /// </summary>
    public void ReachGoal()
    {
        // Increment frog counter regardless of timer state
        frogsReachedCount++;
        UpdateFrogsUI();

        int timeBonus = Mathf.FloorToInt(currentTime * pointsPerRemainingSecond);
        int pointsAwarded = baseGoalScore + timeBonus;

        AddScore(pointsAwarded);

        Debug.Log($"Goal Reached! Frogs Saved: {frogsReachedCount} | Base: {baseGoalScore} | Time Bonus: {timeBonus} (from {currentTime:F1}s left) | Total Score: {currentTotalScore}");

        // Only reset timer if the game hasn't just been won
        if (!IsGameFinished())
        {
            ResetTimer();
        }
        else
        {
            StopTimer();
            DisableTimeBar();
            if (!isGameEndHandled)
            {
                HandleEndGameUI();
            }
        }
    }

    public void AddScore(int points)
    {
        currentTotalScore += points;
        UpdateScoreUI();
    }

    private void UpdateScoreUI()
    {
        if (scoreText != null)
        {
            scoreText.text = currentTotalScore.ToString();
        }
    }

    private void UpdateFrogsUI()
    {
        if (frogsCountText != null)
        {
            frogsCountText.text = frogsReachedCount.ToString();
        }

        if (winFrogsCountText != null)
        {
            winFrogsCountText.text = frogsReachedCount.ToString();
        }
    }

    private void UpdateTimerUI()
    {
        if (timeBarImage == null) return;

        float normalizedTime = Mathf.Clamp01(currentTime / maxTime);
        timeBarImage.fillAmount = normalizedTime;
        timeBarImage.color = timeBarGradient.Evaluate(normalizedTime);
    }

    public void ResetTimer()
    {
        // Re-enable time bar image if it was previously hidden
        if (timeBarImage != null && !timeBarImage.gameObject.activeSelf)
        {
            timeBarImage.gameObject.SetActive(true);
        }

        currentTime = maxTime;
        isTimerRunning = true;
        UpdateTimerUI();
    }

    public void StopTimer()
    {
        isTimerRunning = false;
    }

    public float GetRemainingTime()
    {
        return currentTime;
    }

    private void OnTimeExpired()
    {
        if (IsGameFinished())
        {
            if (!isGameEndHandled)
            {
                HandleEndGameUI();
            }
            return;
        }

        Debug.Log("Time expired! Deducting life and respawning frog.");

        if (LivesManager.Instance != null)
        {
            LivesManager.Instance.LoseLife();
        }

        if (frogController != null)
        {
            frogController.ResetFrog();
        }

        if (!IsGameFinished())
        {
            ResetTimer();
        }
        else
        {
            DisableTimeBar();
            if (!isGameEndHandled)
            {
                HandleEndGameUI();
            }
        }
    }
}