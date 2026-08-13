using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenuController : MonoBehaviour
{
    [Header("Scene Transition Settings")]
    [Tooltip("The exact name of your main gameplay scene as listed in Build Settings.")]
    [SerializeField] private string playSceneName = "GameplayScene";

    [Header("How To Play Panel Setup")]
    [Tooltip("The parent GameObject containing the instructions UI.")]
    [SerializeField] private GameObject howToPlayPanel;

    [Tooltip("Array of slide GameObjects in order (Slide 1, Slide 2, etc.).")]
    [SerializeField] private GameObject[] tutorialSlides;

    private int currentSlideIndex = 0;

    private void Start()
    {
        // Ensure the panel starts hidden when opening the main menu
        if (howToPlayPanel != null)
        {
            howToPlayPanel.SetActive(false);
        }
    }

    #region Main Menu Actions

    /// <summary>
    /// Loads the main game scene. Attach to the Play Button OnClick event.
    /// </summary>
    public void PlayGame()
    {
        SceneManager.LoadScene(playSceneName);
    }

    /// <summary>
    /// Toggles the How to Play panel open or closed. Attach to the How to Play Button OnClick event.
    /// </summary>
    public void ToggleHowToPlay()
    {
        if (howToPlayPanel == null) return;

        bool isCurrentlyActive = howToPlayPanel.activeSelf;
        bool shouldActivate = !isCurrentlyActive;

        howToPlayPanel.SetActive(shouldActivate);

        // If we are opening the panel, reset slides to the first one
        if (shouldActivate)
        {
            ResetTutorialSlides();
        }
    }

    /// <summary>
    /// Closes the application. Attach to the Quit Button OnClick event.
    /// </summary>
    public void QuitGame()
    {
        Debug.Log("Quit Game requested.");
        Application.Quit();
    }

    #endregion

    #region Tutorial Slide System

    /// <summary>
    /// Advances to the next slide, or loops back to the start if on the final slide. Attach to the Next Button OnClick event.
    /// </summary>
    public void NextSlide()
    {
        if (tutorialSlides == null || tutorialSlides.Length == 0) return;

        // Hide current slide
        tutorialSlides[currentSlideIndex].SetActive(false);

        // Increment index with loop-around logic
        currentSlideIndex = (currentSlideIndex + 1) % tutorialSlides.Length;

        // Show new slide
        tutorialSlides[currentSlideIndex].SetActive(true);
    }

    /// <summary>
    /// Resets tutorial view to show only the first slide (index 0).
    /// </summary>
    private void ResetTutorialSlides()
    {
        if (tutorialSlides == null || tutorialSlides.Length == 0) return;

        currentSlideIndex = 0;

        for (int i = 0; i < tutorialSlides.Length; i++)
        {
            if (tutorialSlides[i] != null)
            {
                tutorialSlides[i].SetActive(i == 0);
            }
        }
    }

    #endregion
}