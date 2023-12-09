using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class DisplayVolume : MonoBehaviour
{
    public TextMeshProUGUI textMeshPro;

    void Start()
    {
        PlayerPrefs.SetInt("volume", 50);
        updateVolume();
    }

    void updateVolume()
    {
        textMeshPro.text = PlayerPrefs.GetInt("volume").ToString();
        FindObjectOfType<AudioManager>().SetVolume();
    }

    public void addVolume()
    {
        if (PlayerPrefs.GetInt("volume") + 10 > 100)
            return;
        PlayerPrefs.SetInt("volume", PlayerPrefs.GetInt("volume") + 10);
        updateVolume();
    }

    public void lowerVolume()
    {
        if (PlayerPrefs.GetInt("volume") - 10 < 0)
            return;
        PlayerPrefs.SetInt("volume", PlayerPrefs.GetInt("volume") - 10);
        updateVolume();
    }
}
