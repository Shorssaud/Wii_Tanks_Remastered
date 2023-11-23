using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MainMenu : MonoBehaviour
{
    public void PlayGame()
    {
        // Load the scene named "BaseGame"
        UnityEngine.SceneManagement.SceneManager.LoadScene("BaseGame");
    }

    public void Options()
    {
        // Load the scene named "Options"
        UnityEngine.SceneManagement.SceneManager.LoadScene("Options");
    }
    public void QuitGame()
    {
        // Quit the game
        Application.Quit();
    }
}
