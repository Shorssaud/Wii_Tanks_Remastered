using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MainMenu : MonoBehaviour
{
    void Start(){
        // TODO : Ajouter une condition pour vérifier qu'on est pas en mode 1 life
        PlayerPrefs.SetInt("Lives", 3);
        PlayerPrefs.SetInt("TotalScore", 0);
    }
    public void PlayGame()
    {
        PlayerPrefs.SetInt("Level", 1);
        UnityEngine.SceneManagement.SceneManager.LoadScene(5);
        FindObjectOfType<AudioManager>().PauseEverything();
        FindObjectOfType<AudioManager>().Play("MenuSelection");
    }

    public void Options()
    {
        // Load the scene named "Options"
        UnityEngine.SceneManagement.SceneManager.LoadScene(2);
        FindObjectOfType<AudioManager>().Play("MenuSelection");
    }
    public void QuitGame()
    {
    #if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
    #else
        Application.Quit();
    #endif
    }
    public void Main()
    {
        // TODO : Ajouter une condition pour vérifier qu'on est pas en mode 1 life
        // Load the scene named "MainMenu"
        UnityEngine.SceneManagement.SceneManager.LoadScene(0);
    }
}
