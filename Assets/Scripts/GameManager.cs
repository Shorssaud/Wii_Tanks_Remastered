using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    private int lives;
    public TankPlayer player;

    void Start()
    {
        lives = player.GetLives();
    }

    void Update()
    {
        checkLose();
        checkWin();
    }

    private void checkLose()
    {
        if (lives <= 0) UnityEngine.SceneManagement.SceneManager.LoadScene("Scenes/Menus/LoseMenu");
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
            }
        }
    }
}
