using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MainMenu : MonoBehaviour
{
    void Start(){
        PlayerPrefs.SetInt("Lives", 3);
    }
    public void PlayGame()
    {
        // Load the scene named "BaseGame"
        UnityEngine.SceneManagement.SceneManager.LoadScene("BaseGame");
    }

    public void Options()
    {
        // Load the scene named "Options"
        UnityEngine.SceneManagement.SceneManager.LoadScene(2);
    }
    public void QuitGame()
    {
    #if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
    #else
        Application.Quit();
    #endif
    }
    public void Restart()
    {
        // Reset values here
        // Load the scene named "BaseGame"
        UnityEngine.SceneManagement.SceneManager.LoadScene("BaseGame");
    }
    public void Main()
    {
        // Load the scene named "MainMenu"
        UnityEngine.SceneManagement.SceneManager.LoadScene(0);
    }
}
