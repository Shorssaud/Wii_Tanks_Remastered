using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.EventSystems;
using System.Collections.Generic;

public class UICursorController : MonoBehaviour
{
    private RectTransform rectTransform;
    private Canvas canvas;
    private Camera mainCamera;
    private Gamepad gamepad;
    private Vector2 joystickInput;
    private Vector3 lastMousePosition;
    private PointerEventData pointerEventData;

    void Start()
    {
        rectTransform = GetComponent<RectTransform>();
        canvas = GetComponentInParent<Canvas>();
        mainCamera = canvas.worldCamera;
        Cursor.visible = false;
        gamepad = Gamepad.current;
        lastMousePosition = Input.mousePosition;
        pointerEventData = new PointerEventData(EventSystem.current);
    }

    void Update()
    {
        UpdateCursorPosition();
        HandleInput();
        HandleHover();
    }

    private void UpdateCursorPosition()
    {
        Vector3 mousePosition = Input.mousePosition;

        // Check if mouse has moved
        if (mousePosition != lastMousePosition)
        {
            // Mouse is active, use mouse position
            Vector2 canvasPosition;
            RectTransformUtility.ScreenPointToLocalPointInRectangle(canvas.GetComponent<RectTransform>(), mousePosition, mainCamera, out canvasPosition);
            rectTransform.anchoredPosition = canvasPosition;
            lastMousePosition = mousePosition;
        }
        else if (gamepad != null)
        {
            // Mouse is inactive, use joystick input
            joystickInput = gamepad.rightStick.ReadValue();
            Vector3 joystickDelta = new Vector3(joystickInput.x, joystickInput.y, 0) * joystickInput.magnitude * Time.deltaTime * 1100; // Adjusted for joystick pressure
            Vector3 newPosition = mainCamera.WorldToScreenPoint(rectTransform.position) + joystickDelta;
            Vector2 newCanvasPosition;
            RectTransformUtility.ScreenPointToLocalPointInRectangle(canvas.GetComponent<RectTransform>(), newPosition, mainCamera, out newCanvasPosition);
            rectTransform.anchoredPosition = newCanvasPosition;
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
