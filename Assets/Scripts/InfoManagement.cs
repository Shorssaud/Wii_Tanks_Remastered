using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InfoManagement : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        PlayerPrefs.SetInt("TotalScore", totalScore);
        PlayerPrefs.Save();

    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
