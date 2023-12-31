using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;
using System.Threading;
using System.Diagnostics;

public class GameManager : MonoBehaviour
{
    private GameObject loseMessage;
    private GameObject winMessage;
    private bool onTransition = false;
    Stopwatch GameTime;
    Stopwatch LevelTime;

    public GameManager instance;

    void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject); // Don't destroy this object when loading new scenes
        }
        loseMessage = GameObject.Find("Destroyed");
        winMessage = GameObject.Find("MissionCleared");
        loseMessage.SetActive(false);
        winMessage.SetActive(false);

        GameTime = new Stopwatch();
        LevelTime = new Stopwatch();
        GameTime.Start();
        LevelTime.Start();
    }

    void Update()
    {
        checkLose();
        checkWin();
    }

    private void checkLose()
    {
        if (GameObject.FindGameObjectsWithTag("Player").Length == 0 && !onTransition) {
            onTransition = true;
            //player.RemoveLife();
            if ((PlayerPrefs.GetInt("Lives") - 1) <= 0) StartCoroutine(LoseAndEnd());
            else StartCoroutine(LoseAndRetry());
        }
    }

    private void checkWin()
    {
        if (GameObject.FindGameObjectsWithTag("AI").Length == 0 && !onTransition) {
            onTransition = true;
            int newLevel = PlayerPrefs.GetInt("Level") + 1;
            UnityEngine.Debug.Log(newLevel);
            string newLevelPath = "Scenes/Levels/Level" + newLevel;
            UnityEngine.Debug.Log(newLevelPath);
            if (!IsSceneInBuildSettings(newLevelPath)) StartCoroutine(WinAndEnd());
            else {
                PlayerPrefs.SetInt("Level", newLevel);
                StartCoroutine(WinAndNext());

                FindObjectOfType<AudioManager>().PauseEverything();
                FindObjectOfType<AudioManager>().Play("Explosion");
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
        UnityEngine.Debug.Log("Player lost at level" + PlayerPrefs.GetInt("Level"));
        UnityEngine.SceneManagement.SceneManager.LoadScene("Scenes/Menus/LoseMenu");
    }

    IEnumerator WinAndNext()
    {
        winMessage.SetActive(true);
        //winMessage.gameObject.SetActive(true);
        yield return new WaitForSeconds(2f); // Attendre 1 seconde
        UnityEngine.Debug.Log("Finished Level in " + LevelTime.Elapsed);
        UnityEngine.SceneManagement.SceneManager.LoadScene("Scenes/Menus/Transition");
    }

    IEnumerator WinAndEnd()
    {
        winMessage.SetActive(true);
        //winMessage.gameObject.SetActive(true);
        yield return new WaitForSeconds(2f); // Attendre 1 seconde
        UnityEngine.SceneManagement.SceneManager.LoadScene("Scenes/Menus/WinMenu");
    }

    bool IsSceneInBuildSettings(string sceneName)
    {
        for (int i = 0; i < SceneManager.sceneCountInBuildSettings; i++)
        {
            string scenePath = SceneUtility.GetScenePathByBuildIndex(i);
            if (scenePath.Contains(sceneName))
            {
                return true;
            }
        }
        return false;
    }

    private void OnDestroy()
    {
        Destroy(gameObject);
        UnityEngine.Debug.Log("Total Game Time: " +  GameTime.Elapsed);
    }
}
