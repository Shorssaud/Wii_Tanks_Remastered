using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class GameDisplayTankLeft : MonoBehaviour
{
    private void Start()
    {
        GameObject.Find("UIMission").GetComponent<TextMeshProUGUI>().text = "Mission " + PlayerPrefs.GetInt("Level").ToString();
    }

    void Update()
    {
        GameObject[] prefabInstances = GameObject.FindGameObjectsWithTag("AI");
        GameObject.Find("UITankLeft").GetComponent<TextMeshProUGUI>().text = "x " + prefabInstances.Length.ToString();
    }
}
