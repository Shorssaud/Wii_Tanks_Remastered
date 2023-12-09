using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class DisplayMode : MonoBehaviour
{
    public TextMeshProUGUI modeText;

    void Update()
    {
        modeText.text = PlayerPrefs.GetString("GameMode");
    }
}