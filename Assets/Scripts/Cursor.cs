using UnityEngine;
using UnityEngine.InputSystem;

public class MouseCursor : MonoBehaviour
{
    private Camera mainCamera;
    public float cursorPlaneDistance = 5f;
    public float cursorScale = 15f;

    private Gamepad gamepad;
    private Vector2 joystickInput;
    private Vector3 lastMousePosition;

    void Start()
    {
        mainCamera = Camera.main;
        Cursor.visible = false;
        gamepad = Gamepad.current;
        lastMousePosition = Input.mousePosition;
    }

    void LateUpdate()
    {
        UpdateCursorPosition();
        UpdateCursorScale();
        BillboardSprite();
    }

    private void UpdateCursorPosition()
    {
        Vector3 currentMousePosition = Input.mousePosition;

        // Check if the mouse has moved
        if (currentMousePosition != lastMousePosition)
        {
            // Mouse has moved, update cursor position using mouse input
            Vector3 screenPosition = currentMousePosition;
            screenPosition.z = cursorPlaneDistance;
            transform.position = mainCamera.ScreenToWorldPoint(screenPosition);
            lastMousePosition = currentMousePosition; // Update last mouse position
        }
        else
        {
            // Mouse is stationary, use joystick input to move the cursor
            joystickInput = gamepad?.rightStick.ReadValue() ?? Vector2.zero;
            if (joystickInput != Vector2.zero)
            {
                Vector3 screenPosition = mainCamera.WorldToScreenPoint(transform.position);
                screenPosition += new Vector3(joystickInput.x, joystickInput.y, 0) * 10f; // Adjust cursor speed
                screenPosition.z = cursorPlaneDistance;
                transform.position = mainCamera.ScreenToWorldPoint(screenPosition);
            }
        }
    }

    private void UpdateCursorScale()
    {
        transform.localScale = Vector3.one * cursorScale;
    }

    private void BillboardSprite()
    {
        transform.forward = -mainCamera.transform.forward;
    }
}
