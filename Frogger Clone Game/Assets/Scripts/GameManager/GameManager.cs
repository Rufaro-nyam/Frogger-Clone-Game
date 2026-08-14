using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    // Call this from Retry button
    public void RetryGame()
    {
        Debug.Log("Retrying game...");
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    // Call this from Menu button
    public void GoToMenu()
    {
        Debug.Log("Going to menu...");
        SceneManager.LoadScene("Main Menu Scene"); 
    }

    // Call this from Quit button
    public void QuitGame()
    {
        Debug.Log("Quitting game...");
        Application.Quit();

//#if UNITY_EDITOR
//        UnityEditor.EditorApplication.isPlaying = false;
//#endif
    }
}