using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public TankPlayer player;

    void Start()
    {
    }

    void Update()
    {
        checkLose();
        checkWin();
    }

    private void checkLose()
    {
        if (GameObject.FindGameObjectsWithTag("Player").Length == 0) {
            Debug.Log("CheckLose");
            player.RemoveLife();
            if (player.GetLives() <= 0) UnityEngine.SceneManagement.SceneManager.LoadScene("Scenes/Menus/LoseMenu");
            else UnityEngine.SceneManagement.SceneManager.LoadScene("Scenes/Menus/Transition");
        }
    }

    private void checkWin()
    {
        if (GameObject.FindGameObjectsWithTag("AI").Length == 0) {
            int newLevel = PlayerPrefs.GetInt("Level") + 1;
            // TODO : mettre à jour en fonction du nombre de niveaux
            if (newLevel >= 3) UnityEngine.SceneManagement.SceneManager.LoadScene("Scenes/Menus/WinMenu");
            else {
                PlayerPrefs.SetInt("Level", newLevel);
                UnityEngine.SceneManagement.SceneManager.LoadScene("Scenes/Menus/Transition");

                FindObjectOfType<AudioManager>().PauseEverything();
                FindObjectOfType<AudioManager>().Play("Round Start");
            }
        }
    }
}
