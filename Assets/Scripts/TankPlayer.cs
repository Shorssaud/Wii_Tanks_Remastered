using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TankPlayer : Tank
{
    
    // Start is called before the first frame update
    void Start()
    {
        maxSpeed = 10f;
        rotSpeed = 270f;
        bulletSpeed = 10f;
        bulletRicochetMax = 2;
        maxBullets = 5;
        fireRate = 0.3f;
    }

    // Update is called once per frame
    void Update()
    {
        // Get horizontal and vertical input values
        float horizontal = Input.GetAxis("Horizontal");
        float vertical = Input.GetAxis("Vertical");
        if (Input.GetKeyDown(KeyCode.Space))
        {
            Shoot();
        }
        
        Move(horizontal, vertical);
        // Make the cannon always point at the mouse
        CannonTracer();
    }

        // Makes the cannon always point at the mouse
    private void CannonTracer()
    {
        // Set up a layer mask to include only the floor layer
        int floorLayerMask = 1 << LayerMask.NameToLayer("Floor");

        // Create a ray from the mouse cursor on screen in the direction of the camera
        Ray camRay = Camera.main.ScreenPointToRay(Input.mousePosition);

        // Create a RaycastHit variable to store information about what was hit by the ray
        RaycastHit floorHit;

        // Perform the raycast and if it hits something on the floor layer...
        if (Physics.Raycast(camRay, out floorHit, 1000f, floorLayerMask))
        {
            // Create a vector from the player to the point on the floor the raycast from the mouse hit
            Vector3 playerToMouse = floorHit.point - cannon.position;

            // Ensure the vector is entirely along the floor plane
            playerToMouse.y = 0f;

            // Create a quaternion (rotation) based on looking down the vector from the player to the mouse
            Quaternion newRotation = Quaternion.LookRotation(playerToMouse);

            // Extract the y-axis rotation from the newRotation quaternion
            float yRotation = newRotation.eulerAngles.y;

            // Create a new quaternion with only the y-axis rotation
            Quaternion newYRotation = Quaternion.Euler(0f, yRotation, 270f);

            // Set the player's rotation to this new rotation
            cannon.rotation = newYRotation;
        }
    }
}
