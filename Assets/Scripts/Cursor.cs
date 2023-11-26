using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CursorController : MonoBehaviour
{
    public Texture2D cursorTexture; // Reference to your cursor image

    void Start()
    {
        // Make the texture readable by the CPU
        cursorTexture.filterMode = FilterMode.Point;
        cursorTexture.anisoLevel = 0;

        Cursor.SetCursor(cursorTexture, Vector2.zero, CursorMode.Auto);
    }
}
