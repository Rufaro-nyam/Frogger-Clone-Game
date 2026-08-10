//using System;
//using UnityEngine;
//using UnityEngine.UI;

//public class LivesManager : MonoBehaviour
//{
//    public static LivesManager Instance { get; private set; }

//    [Header("Lives Settings")]
//    [SerializeField] private int startingLives = 5;

//    [Header("UI (optional)")]
//    [Tooltip("Text element showing the current life count, e.g. 'Lives: 5'.")]
//    [SerializeField] private Text livesText;
//    [Tooltip("Panel/GameObject to show when lives reach 0. Left inactive until game over.")]
//    [SerializeField] private GameObject gameOverPanel;

//    private int currentLives;

//    // Fired whenever lives change, passing the new value. UI can subscribe
//    // instead of polling every frame.
//    public event Action<int> OnLivesChanged;

//    // Fired once when lives hit 0.
//    public event Action OnGameOver;

//    public int CurrentLives => currentLives;
//    public int MaxLives => startingLives;
//    public bool IsGameOver { get; private set; }

//    private void Awake()
//    {
//        // Simple singleton - keep the first one, remove duplicates.
//        if (Instance != null && Instance != this)
//        {
//            Destroy(gameObject);
//            return;
//        }

//        Instance = this;
//    }

//    private void Start()
//    {
//        currentLives = startingLives;
//        IsGameOver = false;

//        if (gameOverPanel != null)
//        {
//            gameOverPanel.SetActive(false);
//        }

//        UpdateUI();
//    }

//    public void LoseLife()
//    {
//        if (IsGameOver)
//        {
//            return;
//        }

//        currentLives = Mathf.Max(0, currentLives - 1);
//        UpdateUI();

//        Debug.Log($"Life lost - {currentLives} remaining");

//        if (currentLives <= 0)
//        {
//            TriggerGameOver();
//        }
//    }

//    // Capped at startingLives - the bonus frog can only refund lives you've
//    // already lost, never push you above your original 5.
//    public void AddLife(int amount = 1)
//    {
//        if (IsGameOver)
//        {
//            return;
//        }

//        int previous = currentLives;
//        currentLives = Mathf.Min(startingLives, currentLives + amount);

//        if (currentLives != previous)
//        {
//            Debug.Log($"Life gained - {currentLives} now");
//            UpdateUI();
//        }
//    }

//    public void ResetLives()
//    {
//        currentLives = startingLives;
//        IsGameOver = false;

//        if (gameOverPanel != null)
//        {
//            gameOverPanel.SetActive(false);
//        }

//        UpdateUI();
//    }

//    private void TriggerGameOver()
//    {
//        IsGameOver = true;
//        Debug.LogWarning("Game over - out of lives");

//        if (gameOverPanel != null)
//        {
//            gameOverPanel.SetActive(true);
//        }

//        OnGameOver?.Invoke();
//    }

//    private void UpdateUI()
//    {
//        if (livesText != null)
//        {
//            livesText.text = $"Lives: {currentLives}";
//        }

//        OnLivesChanged?.Invoke(currentLives);
//    }
//}
using System;
using UnityEngine;
using UnityEngine.UI;

public class LivesManager : MonoBehaviour
{
    public static LivesManager Instance { get; private set; }

    [Header("Lives Settings")]
    [SerializeField] private int startingLives = 5;

    [Header("UI (optional)")]
    [Tooltip("Text element showing the current life count, e.g. 'Lives: 5'.")]
    [SerializeField] private Text livesText;
    [Tooltip("Panel/GameObject to show when lives reach 0. Left inactive until game over.")]
    [SerializeField] private GameObject gameOverPanel;

    private int currentLives;

    // Fired whenever lives change, passing the new value. UI can subscribe
    // instead of polling every frame.
    public event Action<int> OnLivesChanged;

    // Fired once when lives hit 0.
    public event Action OnGameOver;

    public int CurrentLives => currentLives;
    public int MaxLives => startingLives;
    public bool IsGameOver { get; private set; }
    public bool HasWon { get; private set; } // NEW: Track if player has won

    private void Awake()
    {
        // Simple singleton - keep the first one, remove duplicates.
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
    }

    private void Start()
    {
        currentLives = startingLives;
        IsGameOver = false;
        HasWon = false;

        if (gameOverPanel != null)
        {
            gameOverPanel.SetActive(false);
        }

        UpdateUI();
    }

    public void LoseLife()
    {
        // Don't lose lives if game is over OR player has won
        if (IsGameOver || HasWon)
        {
            return;
        }

        currentLives = Mathf.Max(0, currentLives - 1);
        UpdateUI();

        Debug.Log($"Life lost - {currentLives} remaining");

        // Only trigger game over if we haven't won and lives are 0
        if (currentLives <= 0 && !HasWon)
        {
            TriggerGameOver();
        }
    }

    // Capped at startingLives - the bonus frog can only refund lives you've
    // already lost, never push you above your original 5.
    public void AddLife(int amount = 1)
    {
        if (IsGameOver || HasWon)
        {
            return;
        }

        int previous = currentLives;
        currentLives = Mathf.Min(startingLives, currentLives + amount);

        if (currentLives != previous)
        {
            Debug.Log($"Life gained - {currentLives} now");
            UpdateUI();
        }
    }

    // NEW: Called when player wins the game
    public void GameWon()
    {
        if (HasWon) return;

        HasWon = true;
        IsGameOver = false; // Ensure game over is false

        // Hide game over panel if it was showing
        if (gameOverPanel != null)
        {
            gameOverPanel.SetActive(false);
        }

        Debug.Log("Player won the game!");
        UpdateUI();
    }

    public void ResetLives()
    {
        currentLives = startingLives;
        IsGameOver = false;
        HasWon = false;

        if (gameOverPanel != null)
        {
            gameOverPanel.SetActive(false);
        }

        UpdateUI();
    }

    private void TriggerGameOver()
    {
        // Don't trigger game over if player has already won
        if (HasWon)
        {
            Debug.Log("Player already won - skipping game over");
            return;
        }

        IsGameOver = true;
       

        if (gameOverPanel != null)
        {
            gameOverPanel.SetActive(true);
        }

        OnGameOver?.Invoke();
    }

    private void UpdateUI()
    {
        if (livesText != null)
        {
            if (HasWon)
            {
                //livesText.text = "YOU WIN!";
            }
            else if (IsGameOver)
            {
                //livesText.text = "GAME OVER";
            }
            else
            {
                livesText.text = $"Lives: {currentLives}";
            }
        }

        OnLivesChanged?.Invoke(currentLives);
    }
}