using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class DisplayStats : MonoBehaviour
{
    public GameObject scoreObject;
    public GameObject levelObject;

    // Start is called before the first frame update
    void Start()
    {
        TextMeshProUGUI scoreText = scoreObject.GetComponent<TextMeshProUGUI>();
        if (scoreText != null)
        {
            scoreText.text = "Score : " + PlayerPrefs.GetInt("TotalScore").ToString();
        }
        TextMeshProUGUI levelText = levelObject.GetComponent<TextMeshProUGUI>();
        if (levelText != null)
        {
            levelText.text = "Level : " + PlayerPrefs.GetInt("Level").ToString();
        }
    }
}
