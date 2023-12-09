using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.EventSystems;
using System.Collections.Generic;
using UnityEngine.UI; // Include this for the Image component

public class UICursorController : MonoBehaviour
{
    private RectTransform rectTransform;
    private Canvas canvas;
    private Camera mainCamera;
    private Gamepad gamepad;
    private Vector2 joystickInput;
    private Vector3 lastMousePosition;
    private PointerEventData pointerEventData;
    private Image cursorImage; // Image component for the cursor
    public float cursorScale = 0.5f; // Adjustable cursor scale
    private float lastMoveTime; // Track the last time the cursor moved
    private const float cursorInactivityThreshold = 4f; // Time in seconds after which cursor disappears

    void Start()
    {
        rectTransform = GetComponent<RectTransform>();
        canvas = GetComponentInParent<Canvas>();
        mainCamera = canvas.worldCamera;
        Cursor.visible = false;
        gamepad = Gamepad.current;
        lastMousePosition = Input.mousePosition;
        pointerEventData = new PointerEventData(EventSystem.current);

        cursorImage = GetComponent<Image>(); // Get the Image component
        if (cursorImage == null)
        {
            Debug.LogError("Cursor Image component not found on the GameObject.");
            return;
        }

        rectTransform.localScale = Vector3.one * cursorScale; // Set the initial scale of the cursor

        // Initially set the cursor image to be invisible
        cursorImage.enabled = false;
    }

    void Update()
    {
        UpdateCursorPosition();
        HandleInput();
        HandleHover();
        CheckCursorVisibility();
    }

    private void UpdateCursorPosition()
    {
        Vector3 mousePosition = Input.mousePosition;
        bool hasMoved = false;

        // Check if mouse has moved
        if (mousePosition != lastMousePosition)
        {
            // Mouse is active, use mouse position
            Vector2 canvasPosition;
            RectTransformUtility.ScreenPointToLocalPointInRectangle(canvas.GetComponent<RectTransform>(), mousePosition, mainCamera, out canvasPosition);
            rectTransform.anchoredPosition = canvasPosition;
            lastMousePosition = mousePosition;
            hasMoved = true;
        }
        else if (gamepad != null)
        {
            // Mouse is inactive, use joystick input
            joystickInput = gamepad.rightStick.ReadValue();
            if (joystickInput.magnitude > 0)
            {
                Vector3 joystickDelta = new Vector3(joystickInput.x, joystickInput.y, 0) * joystickInput.magnitude * Time.deltaTime * 1100; // Adjusted for joystick pressure
                Vector3 newPosition = mainCamera.WorldToScreenPoint(rectTransform.position) + joystickDelta;
                Vector2 newCanvasPosition;
                RectTransformUtility.ScreenPointToLocalPointInRectangle(canvas.GetComponent<RectTransform>(), newPosition, mainCamera, out newCanvasPosition);
                rectTransform.anchoredPosition = newCanvasPosition;
                hasMoved = true;
            }
        }

        // Update the last move time and enable cursor image if the cursor has moved
        if (hasMoved)
        {
            lastMoveTime = Time.time;
            cursorImage.enabled = true;
        }
    }

    private void CheckCursorVisibility()
    {
        // Hide the cursor if it hasn't moved for the threshold duration
        if (Time.time - lastMoveTime > cursorInactivityThreshold)
        {
            cursorImage.enabled = false;
        }
    }

    private void HandleInput()
    {
        // Check for mouse click or gamepad button press
        if (Input.GetMouseButtonUp(0) || (gamepad != null && gamepad.buttonSouth.wasPressedThisFrame))
        {
            ExecuteClick();
        }
    }

    private void ExecuteClick()
    {
        pointerEventData.position = RectTransformUtility.WorldToScreenPoint(mainCamera, rectTransform.position);

        List<RaycastResult> raycastResults = new List<RaycastResult>();
        EventSystem.current.RaycastAll(pointerEventData, raycastResults);

        foreach (var result in raycastResults)
        {
            GameObject clickedObject = result.gameObject;
            if (clickedObject != null && clickedObject.GetComponent<UnityEngine.UI.Button>())
            {
                // Execute the click event
                ExecuteEvents.Execute(clickedObject, pointerEventData, ExecuteEvents.pointerClickHandler);
                break;
            }
        }
    }

        private void HandleHover()
    {
        pointerEventData.position = RectTransformUtility.WorldToScreenPoint(mainCamera, rectTransform.position);

        List<RaycastResult> raycastResults = new List<RaycastResult>();
        EventSystem.current.RaycastAll(pointerEventData, raycastResults);

        GameObject hoveredObject = null;
        foreach (var result in raycastResults)
        {
            if (result.gameObject.GetComponent<UnityEngine.UI.Button>() != null)
            {
                hoveredObject = result.gameObject;
                break;
            }
        }

        if (hoveredObject != EventSystem.current.currentSelectedGameObject)
        {
            if (EventSystem.current.currentSelectedGameObject != null)
            {
                // Trigger pointer exit event on the previously hovered object
                ExecuteEvents.Execute(EventSystem.current.currentSelectedGameObject, pointerEventData, ExecuteEvents.pointerExitHandler);
            }

            if (hoveredObject != null)
            {
                // Trigger pointer enter event on the newly hovered object
                ExecuteEvents.Execute(hoveredObject, pointerEventData, ExecuteEvents.pointerEnterHandler);
            }

            EventSystem.current.SetSelectedGameObject(hoveredObject);
        }
    }
}
