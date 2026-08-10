////using System;
////using UnityEngine;
////using UnityEngine.UI;

////public class LivesManager : MonoBehaviour
////{
////    public static LivesManager Instance { get; private set; }

////    [Header("Lives Settings")]
////    [SerializeField] private int startingLives = 5;

////    [Header("UI (optional)")]
////    [Tooltip("Text element showing the current life count, e.g. 'Lives: 5'.")]
////    [SerializeField] private Text livesText;
////    [Tooltip("Panel/GameObject to show when lives reach 0. Left inactive until game over.")]
////    [SerializeField] private GameObject gameOverPanel;

////    private int currentLives;

////    // Fired whenever lives change, passing the new value. UI can subscribe
////    // instead of polling every frame.
////    public event Action<int> OnLivesChanged;

////    // Fired once when lives hit 0.
////    public event Action OnGameOver;

////    public int CurrentLives => currentLives;
////    public int MaxLives => startingLives;
////    public bool IsGameOver { get; private set; }

////    private void Awake()
////    {
////        // Simple singleton - keep the first one, remove duplicates.
////        if (Instance != null && Instance != this)
////        {
////            Destroy(gameObject);
////            return;
////        }

////        Instance = this;
////    }

////    private void Start()
////    {
////        currentLives = startingLives;
////        IsGameOver = false;

////        if (gameOverPanel != null)
////        {
////            gameOverPanel.SetActive(false);
////        }

////        UpdateUI();
////    }

////    public void LoseLife()
////    {
////        if (IsGameOver)
////        {
////            return;
////        }

////        currentLives = Mathf.Max(0, currentLives - 1);
////        UpdateUI();

////        Debug.Log($"Life lost - {currentLives} remaining");

////        if (currentLives <= 0)
////        {
////            TriggerGameOver();
////        }
////    }

////    // Capped at startingLives - the bonus frog can only refund lives you've
////    // already lost, never push you above your original 5.
////    public void AddLife(int amount = 1)
////    {
////        if (IsGameOver)
////        {
////            return;
////        }

////        int previous = currentLives;
////        currentLives = Mathf.Min(startingLives, currentLives + amount);

////        if (currentLives != previous)
////        {
////            Debug.Log($"Life gained - {currentLives} now");
////            UpdateUI();
////        }
////    }

////    public void ResetLives()
////    {
////        currentLives = startingLives;
////        IsGameOver = false;

////        if (gameOverPanel != null)
////        {
////            gameOverPanel.SetActive(false);
////        }

////        UpdateUI();
////    }

////    private void TriggerGameOver()
////    {
////        IsGameOver = true;
////        Debug.LogWarning("Game over - out of lives");

////        if (gameOverPanel != null)
////        {
////            gameOverPanel.SetActive(true);
////        }

////        OnGameOver?.Invoke();
////    }

////    private void UpdateUI()
////    {
////        if (livesText != null)
////        {
////            livesText.text = $"Lives: {currentLives}";
////        }

////        OnLivesChanged?.Invoke(currentLives);
////    }
////}
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
//    public bool HasWon { get; private set; } // NEW: Track if player has won

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
//        HasWon = false;

//        if (gameOverPanel != null)
//        {
//            gameOverPanel.SetActive(false);
//        }

//        UpdateUI();
//    }

//    public void LoseLife()
//    {
//        // Don't lose lives if game is over OR player has won
//        if (IsGameOver || HasWon)
//        {
//            return;
//        }

//        currentLives = Mathf.Max(0, currentLives - 1);
//        UpdateUI();

//        Debug.Log($"Life lost - {currentLives} remaining");

//        // Only trigger game over if we haven't won and lives are 0
//        if (currentLives <= 0 && !HasWon)
//        {
//            TriggerGameOver();
//        }
//    }

//    // Capped at startingLives - the bonus frog can only refund lives you've
//    // already lost, never push you above your original 5.
//    public void AddLife(int amount = 1)
//    {
//        if (IsGameOver || HasWon)
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

//    // NEW: Called when player wins the game
//    public void GameWon()
//    {
//        if (HasWon) return;

//        HasWon = true;
//        IsGameOver = false; // Ensure game over is false

//        // Hide game over panel if it was showing
//        if (gameOverPanel != null)
//        {
//            gameOverPanel.SetActive(false);
//        }

//        Debug.Log("Player won the game!");
//        UpdateUI();
//    }

//    public void ResetLives()
//    {
//        currentLives = startingLives;
//        IsGameOver = false;
//        HasWon = false;

//        if (gameOverPanel != null)
//        {
//            gameOverPanel.SetActive(false);
//        }

//        UpdateUI();
//    }

//    private void TriggerGameOver()
//    {
//        // Don't trigger game over if player has already won
//        if (HasWon)
//        {
//            Debug.Log("Player already won - skipping game over");
//            return;
//        }

//        IsGameOver = true;


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
//            if (HasWon)
//            {
//                //livesText.text = "YOU WIN!";
//            }
//            else if (IsGameOver)
//            {
//                //livesText.text = "GAME OVER";
//            }
//            else
//            {
//                livesText.text = $"Lives: {currentLives}";
//            }
//        }

//        OnLivesChanged?.Invoke(currentLives);
//    }
//}
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class LivesManager : MonoBehaviour
{
    public static LivesManager Instance { get; private set; }

    [Header("Lives Settings")]
    [SerializeField] private int startingLives = 5;

    [Header("UI - Frog Icons")]
    [Tooltip("The frog icon prefab to display in the UI")]
    [SerializeField] private GameObject frogIconPrefab;
    [Tooltip("The parent transform where frog icons will be placed")]
    [SerializeField] private Transform frogIconsParent;
    [Tooltip("Spacing between frog icons")]
    [SerializeField] private float iconSpacing = 30f;

    [Header("UI - Panels")]
    [Tooltip("Panel/GameObject to show when lives reach 0. Left inactive until game over.")]
    [SerializeField] private GameObject gameOverPanel;
    [Tooltip("Optional: Text to show when player wins")]
    //[SerializeField] private GameObject winTextObject;

    private int currentLives;
    private List<GameObject> frogIcons = new List<GameObject>();

    // Events
    public event Action<int> OnLivesChanged;
    public event Action OnGameOver;

    public int CurrentLives => currentLives;
    public int MaxLives => startingLives;
    public bool IsGameOver { get; private set; }
    public bool HasWon { get; private set; }

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

        // Create frog icons in UI
        CreateFrogIcons();
        UpdateUI();
    }

    private void CreateFrogIcons()
    {
        // Clear existing icons
        foreach (GameObject icon in frogIcons)
        {
            if (icon != null)
                Destroy(icon);
        }
        frogIcons.Clear();

        if (frogIconPrefab == null || frogIconsParent == null)
        {
            Debug.LogWarning("Frog icon prefab or parent not assigned in LivesManager! Using text fallback.");
            return;
        }

        // Show startingLives - 1 icons (one frog is active in the game)
        int iconsToShow = startingLives - 1;

        for (int i = 0; i < iconsToShow; i++)
        {
            GameObject icon = Instantiate(frogIconPrefab, frogIconsParent);

            // Position icons horizontally with spacing
            RectTransform rectTransform = icon.GetComponent<RectTransform>();
            if (rectTransform != null)
            {
                rectTransform.anchoredPosition = new Vector2(i * iconSpacing, 0);
            }

            frogIcons.Add(icon);
        }

        Debug.Log($"Created {iconsToShow} frog icons in UI (1 active in game)");
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

    // Called when player wins the game
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

        // Recreate frog icons
        CreateFrogIcons();
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
        // Update frog icons
        UpdateFrogIcons();

        // Fire events
        OnLivesChanged?.Invoke(currentLives);
    }

    private void UpdateFrogIcons()
    {
        // If frog icons aren't set up, skip
        if (frogIconPrefab == null || frogIconsParent == null)
            return;

        // Show currentLives - 1 icons (one is active in game)
        // But if game is over or won, show 0 icons
        int iconsToShow = 0;

        if (HasWon || IsGameOver)
        {
            iconsToShow = 0;
        }
        else
        {
            iconsToShow = Mathf.Max(0, currentLives - 1);
        }

        // Update visibility of icons
        for (int i = 0; i < frogIcons.Count; i++)
        {
            if (frogIcons[i] != null)
            {
                frogIcons[i].SetActive(i < iconsToShow);
            }
        }

        Debug.Log($"UI Frog Icons: Showing {iconsToShow} (Lives: {currentLives}, Active in game: 1)");
    }
}