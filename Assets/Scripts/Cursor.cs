using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CursorController : MonoBehaviour
{
    public Texture2D cursorTexture; // Reference to your cursor image
    private Vector2 hotspot;


    void Start()
    {
        // Make the texture readable by the CPU
        cursorTexture.filterMode = FilterMode.Point;
        cursorTexture.anisoLevel = 0;

        // Calculate the center of the texture
        hotspot = new Vector2(cursorTexture.width / 2, cursorTexture.height / 2);

        // Set the cursor with the center hotspot
        Cursor.SetCursor(cursorTexture, hotspot, CursorMode.Auto);
    }
}
