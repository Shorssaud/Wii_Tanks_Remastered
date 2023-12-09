using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.InputSystem;

public class DisplayTransition : MonoBehaviour
{
    public GameObject levelObject;
    // Start is called before the first frame update
    void Start()
    {
        TextMeshProUGUI levelText = levelObject.GetComponent<TextMeshProUGUI>();
        if (levelText != null)
        {
            levelText.text = "Mission " + PlayerPrefs.GetInt("Level").ToString();
        }
        GameObject.Find("UILifeLeft").GetComponent<TextMeshProUGUI>().text = "x " + PlayerPrefs.GetInt("Lives").ToString();
    }

    void Update()
    {
        if (Input.GetMouseButton(0))
        {
            string newLevel = "Scenes/Levels/Level" + PlayerPrefs.GetInt("Level").ToString();
            UnityEngine.SceneManagement.SceneManager.LoadScene(newLevel);
        }
    }
}
