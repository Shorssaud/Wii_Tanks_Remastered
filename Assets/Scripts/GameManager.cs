using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class GameManager : MonoBehaviour
{
    private GameObject loseMessage;
    private GameObject winMessage;

    private bool isLoose;

    void Start()
    {
        loseMessage = GameObject.Find("Destroyed");
        winMessage = GameObject.Find("MissionCleared");
        loseMessage.SetActive(false);
        winMessage.SetActive(false);
    }

    void Update()
    {
        checkLose();
        checkWin();
    }

    private void checkLose()
    {
        if (GameObject.FindGameObjectsWithTag("Player").Length == 0) {
            //player.RemoveLife();
            if ((PlayerPrefs.GetInt("Lives") - 1) <= 0) StartCoroutine(LoseAndEnd());
            else StartCoroutine(LoseAndRetry());
        }
    }

    private void checkWin()
    {
        if (GameObject.FindGameObjectsWithTag("AI").Length == 0) {
            int newLevel = PlayerPrefs.GetInt("Level") + 1;
            // TODO : mettre à jour en fonction du nombre de niveaux
            if (newLevel >= 3) StartCoroutine(WinAndEnd());
            else {
                PlayerPrefs.SetInt("Level", newLevel);
                StartCoroutine(WinAndNext());

                FindObjectOfType<AudioManager>().PauseEverything();
                FindObjectOfType<AudioManager>().Play("Round Start");
            }
        }
    }

    IEnumerator LoseAndRetry()
    {
        loseMessage.SetActive(true);
        yield return new WaitForSeconds(2f); // Attendre 1 seconde
        PlayerPrefs.SetInt("Lives", PlayerPrefs.GetInt("Lives") - 1);
        UnityEngine.SceneManagement.SceneManager.LoadScene("Scenes/Menus/Transition");
    }

    IEnumerator LoseAndEnd()
    {
        loseMessage.SetActive(true);
        yield return new WaitForSeconds(2f); // Attendre 1 seconde
        PlayerPrefs.SetInt("Lives", PlayerPrefs.GetInt("Lives") - 1);
        UnityEngine.SceneManagement.SceneManager.LoadScene("Scenes/Menus/LoseMenu");
    }

    IEnumerator WinAndNext()
    {
        winMessage.SetActive(true);
        //winMessage.gameObject.SetActive(true);
        yield return new WaitForSeconds(2f); // Attendre 1 seconde
        UnityEngine.SceneManagement.SceneManager.LoadScene("Scenes/Menus/Transition");
    }

    IEnumerator WinAndEnd()
    {
        winMessage.SetActive(true);
        //winMessage.gameObject.SetActive(true);
        yield return new WaitForSeconds(2f); // Attendre 1 seconde
        UnityEngine.SceneManagement.SceneManager.LoadScene("Scenes/Menus/WinMenu");
    }
}
