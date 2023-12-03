using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Tank : MonoBehaviour
{
    public float maxSpeed;
    public float rotSpeed;
    public float bulletSpeed;
    public int bulletRicochetMax;
    public int currentBullets;
    public int maxBullets;
    public float fireRate;
    public float nextFire;

    public GameObject explosionPrefab;

    private Vector3 vel = Vector3.zero;
    private Quaternion targetRotation;

    public Rigidbody rb;

    public GameObject bulletPrefab;
    public Transform bulletSpawn;
    public Transform cannon;
    public Transform tankBase;

    // Start is called before the first frame update
    void Start()
    {
        rb = GetComponent<Rigidbody>();
        nextFire = 0;
        currentBullets = 0;
    }

    protected void RotateBase(float angleDegrees)
    {
        // Target Y-axis rotation only
        Quaternion baseTargetRotation = Quaternion.Euler(0f, angleDegrees, 0f);
        // Apply the rotation only on the Y axis
        tankBase.rotation = Quaternion.Euler(tankBase.rotation.eulerAngles.x,
                                             Quaternion.RotateTowards(tankBase.rotation, baseTargetRotation, rotSpeed * Time.deltaTime).eulerAngles.y,
                                             tankBase.rotation.eulerAngles.z);
    }


    // Sets the movement vector based on the interger provided
    // horizontal = -1 is left, 1 is right, 0 is no horizontal movement
    // vertical = -1 is down, 1 is up, 0 is no vertical movement
    // the horizontal and vertical are flipped and modified because of the camera angle
    protected void Move(float vertical, float horizontal)
    {
        // If there is no input on the horizontal or vertical axis set the velocity to 0
        if (horizontal == 0 && vertical == 0)
        {
            vel = Vector3.zero;
            return;
        }
        vertical = vertical * (-1);

        // Calculate the angle in radians using Mathf.Atan2
        float angle = Mathf.Atan2(horizontal, vertical);

        // Convert the angle to degrees
        float angleDegrees = angle * Mathf.Rad2Deg;
        RotateBase(angleDegrees);

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
    // TODO change this to accomodate different bullet types
    protected void Shoot()
    {
        if (nextFire > 0 || currentBullets >= maxBullets)
            return;
        // Instantiate the projectile at the position and rotation of this transform with the layer of the tank
        GameObject bullet = Instantiate(bulletPrefab, bulletSpawn.position, bulletSpawn.rotation);

        // Apply the tanks bullet settings
        bullet.GetComponent<BulletBase>().speed = bulletSpeed;
        bullet.GetComponent<BulletBase>().ricochetMax = bulletRicochetMax;

        //set the parent tank
        bullet.GetComponent<BulletBase>().parentTank = gameObject;

        // reset firing timer
        nextFire = fireRate;
    }



    // TankUpdate is called once per frame
    protected void TankUpdate()
    {
        // Set the velocity of the rigidbody to the movement vector
        rb.velocity = vel;

        nextFire -= Time.deltaTime;
    }

    public void DestroyTank(float explosionSize = 1.0f)
    {
        if (explosionPrefab != null)
        {
            GameObject explosion = Instantiate(explosionPrefab, transform.position, Quaternion.identity);
            explosion.transform.localScale *= explosionSize; // Scale the explosion effect
        }
        Destroy(gameObject);
    }
}
