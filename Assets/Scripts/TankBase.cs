using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Tank : MonoBehaviour
{
    public float maxSpeed;
    public float rotSpeed;
    public float bulletSpeed;

    private Vector3 vel = Vector3.zero;
    private Vector3 rot = Vector3.zero;
    private Quaternion targetRotation;

    public Rigidbody rb;

    public GameObject bulletPrefab;
    public Transform bulletSpawn;
    public Transform cannon;

    // Start is called before the first frame update
    void Start()
    {
        rb = GetComponent<Rigidbody>();
    }
    // Sets the movement vector based on the interger provided
    protected void Move(float horizontal, float vertical)
    {
        // If there is no input on the horizontal or vertical axis set the velocity to 0
        if (horizontal == 0 && vertical == 0)
        {
            vel = Vector3.zero;
            return;
        }
        // Calculate the angle in radians using Mathf.Atan2
        float angle = Mathf.Atan2(horizontal, vertical);

        // Convert the angle to degrees
        float angleDegrees = angle * Mathf.Rad2Deg;

        // Create a direction vector based on the angle
        Vector3 moveDirection = new Vector3(Mathf.Sin(angle), 0f, Mathf.Cos(angle));

        // Apply movement based on input and speed using the direction vector
        vel = moveDirection * maxSpeed;

        // Calculate the target rotation based on the angle
        targetRotation = Quaternion.Euler(0f, angleDegrees, 0f);

        // if the target rotation is more than 90 degrees from the current rotation rotate the rear instead
        if (Mathf.Abs(Quaternion.Angle(rb.rotation, targetRotation)) > 90)
        {
            // Calculate the target rotation based on the angle
            targetRotation = Quaternion.Euler(0f, angleDegrees + 180, 0f);
        }
    }

    // Shoots a bullet
    protected void Shoot()
    {
        // Instantiate the projectile at the position and rotation of this transform
        GameObject bullet = Instantiate(bulletPrefab, bulletSpawn.position, bulletSpawn.rotation);

        // Add velocity to the bullet
        bullet.GetComponent<Rigidbody>().velocity = bullet.transform.forward * bulletSpeed;

        // Destroy the bullet after 2 seconds
        Destroy(bullet, 2.0f);
    }

    // Makes the cannon always point at the mouse
    private void CannonTracer()
    {
        // Create a ray from the mouse cursor on screen in the direction of the camera
        Ray camRay = Camera.main.ScreenPointToRay(Input.mousePosition);

        // Create a RaycastHit variable to store information about what was hit by the ray
        RaycastHit floorHit;

        // Perform the raycast and if it hits something on the floor layer...
        if (Physics.Raycast(camRay, out floorHit, 100f))
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

    // TankUpdate is called once per frame
    protected void TankUpdate()
    {
        // Set the velocity of the rigidbody to the movement vector
        rb.velocity = vel;
        // if the current rotation is not the target rotation set velocity to 0
        if (rb.rotation != targetRotation)
            rb.velocity = Vector3.zero;
        // Set the rotation of the rigidbody to the target rotation
        rb.rotation = Quaternion.RotateTowards(rb.rotation, targetRotation, rotSpeed * Time.deltaTime);
        // Make the cannon always point at the mouse
        CannonTracer();
    }
}
