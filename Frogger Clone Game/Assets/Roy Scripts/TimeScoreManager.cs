using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class TimeScoreManager : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private Image timeBarImage;
    [SerializeField] private TextMeshProUGUI scoreText; // TextMeshPro component showing purely the score number

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

    public int CurrentTotalScore => currentTotalScore;

    private void Start()
    {
        UpdateScoreUI();
        ResetTimer();
    }

    private void Update()
    {
        // Testing trigger: Press R to simulate reaching the end goal
        if (Input.GetKeyDown(KeyCode.R))
        {
            ReachGoal();
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
    /// Called when the player successfully reaches a goal slot.
    /// Calculates points based on remaining time, adds to total, and resets timer for next goal.
    /// </summary>
    public void ReachGoal()
    {
        if (!isTimerRunning) return;

        // Calculate time bonus: remaining seconds multiplied by points rate
        int timeBonus = Mathf.FloorToInt(currentTime * pointsPerRemainingSecond);
        int pointsAwarded = baseGoalScore + timeBonus;

        AddScore(pointsAwarded);

        Debug.Log($"Goal Reached! Base: {baseGoalScore} | Time Bonus: {timeBonus} (from {currentTime:F1}s left) | Awarded: {pointsAwarded} | Total Score: {currentTotalScore}");

        // Reset the timer for the next goal
        ResetTimer();
    }

    /// <summary>
    /// Adds points to the total score and updates the UI text.
    /// </summary>
    public void AddScore(int points)
    {
        currentTotalScore += points;
        UpdateScoreUI();
    }

    /// <summary>
    /// Updates the score UI display to strictly show the numerical value.
    /// </summary>
    private void UpdateScoreUI()
    {
        if (scoreText != null)
        {
            scoreText.text = currentTotalScore.ToString();
        }
    }

    /// <summary>
    /// Recalculates fill amount and updates the image color via the gradient.
    /// </summary>
    private void UpdateTimerUI()
    {
        if (timeBarImage == null) return;

        float normalizedTime = Mathf.Clamp01(currentTime / maxTime);
        timeBarImage.fillAmount = normalizedTime;
        timeBarImage.color = timeBarGradient.Evaluate(normalizedTime);
    }

    /// <summary>
    /// Resets the timer back to maximum time and starts counting down again.
    /// </summary>
    public void ResetTimer()
    {
        currentTime = maxTime;
        isTimerRunning = true;
        UpdateTimerUI();
    }

    /// <summary>
    /// Stops the timer.
    /// </summary>
    public void StopTimer()
    {
        isTimerRunning = false;
    }

    /// <summary>
    /// Returns the remaining time in seconds.
    /// </summary>
    public float GetRemainingTime()
    {
        return currentTime;
    }

    private void OnTimeExpired()
    {
        Debug.Log("Time expired!");
    }
}