using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class GameDisplayScore : MonoBehaviour
{
    void Update()
    {
        GameObject.Find("UIScore").GetComponent<TextMeshProUGUI>().text = PlayerPrefs.GetInt("TotalScore").ToString();
    }
}
