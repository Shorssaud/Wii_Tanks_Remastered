using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.InputSystem;

public class DisplayTransition : MonoBehaviour
{
    public GameObject levelObject;
    private Gamepad gamepad;

    // Start is called before the first frame update
    void Start()
    {
        TextMeshProUGUI levelText = levelObject.GetComponent<TextMeshProUGUI>();
        if (levelText != null)
        {
            levelText.text = "Mission " + PlayerPrefs.GetInt("Level").ToString();
        }
        Debug.Log(PlayerPrefs.GetInt("Lives").ToString());
        GameObject.Find("UILifeLeft").GetComponent<TextMeshProUGUI>().text = "x " + PlayerPrefs.GetInt("Lives").ToString();

        gamepad = Gamepad.current; // Initialize the gamepad
    }

    void Update()
    {
        // Check for mouse click or gamepad button press
        if (Input.GetMouseButton(0) || (gamepad != null && gamepad.buttonSouth.wasPressedThisFrame))
        {
            string newLevel = "Scenes/Levels/Level" + PlayerPrefs.GetInt("Level").ToString();
            UnityEngine.SceneManagement.SceneManager.LoadScene(newLevel);
        }
    }
}
