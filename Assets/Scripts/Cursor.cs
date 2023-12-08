using UnityEngine;

public class MouseCursor : MonoBehaviour
{
    private Camera mainCamera;
    public float cursorPlaneDistance = 5f; // Distance of the cursor plane from the camera
    public float cursorScale = 15f; // Uniform scale for the cursor

    void Start()
    {
        mainCamera = Camera.main; // Find the main camera
        Cursor.visible = false; // Hide the system cursor
    }

    void LateUpdate()
    {
        // Translate mouse position into world coordinates
        Vector3 screenPosition = Input.mousePosition;
        screenPosition.z = cursorPlaneDistance; // Set the distance from the camera

        Vector3 worldPosition = mainCamera.ScreenToWorldPoint(screenPosition);
        // Set the cursor's position to the mouse position with the z distance from the camera
        transform.position = new Vector3(worldPosition.x, worldPosition.y, worldPosition.z);

        // Set the cursor's local scale uniformly based on the cursorScale value
        transform.localScale = Vector3.one * cursorScale;

        // Make the sprite face the camera
        BillboardSprite();
    }

    void BillboardSprite()
    {
        // The sprite should face the camera directly
        transform.forward = -mainCamera.transform.forward;
    }
}
