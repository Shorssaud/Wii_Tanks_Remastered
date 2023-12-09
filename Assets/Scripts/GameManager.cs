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
        if (lives <= 0) UnityEngine.SceneManagement.SceneManager.LoadScene(2);
    }
    private void checkWin()
    {
        if (GameObject.FindGameObjectsWithTag("AI").Length == 0) UnityEngine.SceneManagement.SceneManager.LoadScene(3);
    }
}
