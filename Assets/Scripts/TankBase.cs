using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Exploder.Utils;

public class Tank : MonoBehaviour
{
    public float maxSpeed;
    public float rotSpeed;
    public float bulletSpeed;
    public int bulletRicochetMax;
    public int currentBullets;
    public int maxBullets;
    public float fireRate;
    public float mineRate;

    private float nextFire;
    private float nextMine;


    public GameObject explosionPrefab;

    private Vector3 vel = Vector3.zero;
    private Quaternion targetRotation;

    public GameObject bulletPrefab;
    public GameObject minePrefab;
    

    // Start is called before the first frame update
    void Start()
    {
        nextFire = 0;
        nextMine = 0;
        currentBullets = 0;
    }

    private void FixedUpdate()
    {
        nextFire -= Time.deltaTime; // Decrement the nextFire timer
        nextMine -= Time.deltaTime; // Decrement the nextMine timer
    }

    // Sets the movement vector based on the interger provided
    // horizontal = -1 is left, 1 is right, 0 is no horizontal movement
    // vertical = -1 is down, 1 is up, 0 is no vertical movement
    // the horizontal is flipped and modified because of the camera angle
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
        if (transform.rotation.eulerAngles.y - targetRotation.eulerAngles.y > 90 || transform.rotation.eulerAngles.y - targetRotation.eulerAngles.y < -90)
        {
            // Calculate the target rotation based on the angle
            targetRotation = Quaternion.Euler(0f, angleDegrees + 180, 0f);
        }
        if (transform.rotation != targetRotation)
        {
            vel = Vector3.zero;
        }
        // Set the rotation of the rigidbody to the target rotation
        transform.rotation = Quaternion.RotateTowards(transform.rotation, targetRotation, rotSpeed * Time.deltaTime);

        //Checking for wall collisions
        //calculate the tanks future position after moving
        Vector3 futurePosition = transform.position + vel * Time.deltaTime;
        // Get the tank's collider to measure its size
        SphereCollider tankCollider = GetComponent<SphereCollider>();

        // Define the layer mask for the "Default" layer (replace "Default" with your wall layer name)
        LayerMask defaultLayerMask = LayerMask.GetMask("Default");

        // Ignore collisions between the tank's collider and itself
        Physics.IgnoreCollision(tankCollider, tankCollider);

        // Perform a check to see if the tank would collide with any object in default layer at the future position
        if (Physics.CheckSphere(futurePosition, tankCollider.radius, defaultLayerMask) && vel != Vector3.zero)
        {
            // Get all colliders overlapping the CheckSphere
            Collider[] colliders = Physics.OverlapSphere(futurePosition, tankCollider.radius/4, defaultLayerMask);

            // Iterate through the colliders to check if any have the tank's own tag
            foreach (Collider collider in colliders)
            {
                if (collider.gameObject == gameObject)
                {
                    // Ignore collisions with objects having the same tag as the tank
                    Physics.IgnoreCollision(tankCollider, collider, true);
                }
                else
                {
                    // If the collider does not have the tank's tag and there's a collision, set the velocity to zero
                    vel = Vector3.zero;
                }
            }
        }
        // Move the tank while avoiding wall collisions
        transform.position += vel * Time.deltaTime;
    }

    // Shoots a basic bullet
    virtual protected void Shoot(Transform bulletSpawn)
    {
        if (nextFire > 0 || currentBullets >= maxBullets)
            return;
        // If bulletSpawn is in a wall, don't shoot
        if (Physics.CheckBox(bulletSpawn.position, bulletSpawn.localScale / 2, bulletSpawn.rotation, LayerMask.GetMask("Default")))
        {
            // check the collisions and only shoot if the bulletSpawn is not in a object tagged "Wall"
            Collider[] colliders = Physics.OverlapBox(bulletSpawn.position, bulletSpawn.localScale / 2, bulletSpawn.rotation, LayerMask.GetMask("Default"));
            foreach (Collider collider in colliders)
            {
                if (collider.gameObject.tag == "Wall")
                {
                    return;
                }
            }
        }
        currentBullets++;
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

    // Places a mine at the position of the tank
    protected void PlaceMine()
    {
        if (nextMine > 0)
            return;
        // If the tank is in a wall, don't place a mine
        if (Physics.CheckBox(transform.position, transform.localScale / 2, transform.rotation, LayerMask.GetMask("Default")))
        {
            // check the collisions and only place a mine if the tank is not in a object tagged "Wall"
            Collider[] colliders = Physics.OverlapBox(transform.position, transform.localScale / 2, transform.rotation, LayerMask.GetMask("Default"));
            foreach (Collider collider in colliders)
            {
                if (collider.gameObject.tag == "Wall")
                {
                    return;
                }
                if (collider.gameObject.tag == "Mine")
                {
                    return;
                }
            }
        }
        // Instantiate the projectile at the position and rotation of this transform with the layer of the tank
        GameObject mine = Instantiate(minePrefab, transform.position, transform.rotation);

        //set the parent tank
        mine.GetComponent<Mine>().parentTank = gameObject;

        // reset mine timer
        nextMine = mineRate;
    }


    public void RemoveBullet()
    {
        currentBullets -= 1;
    }

    virtual public void DestroyTank(float explosionSize = 1.0f)
    {
        if (explosionPrefab != null)
        {
            GameObject explosion = Instantiate(explosionPrefab, transform.position, Quaternion.identity);
            explosion.transform.localScale *= explosionSize; // Scale the explosion effect
        }
        if (ExploderSingleton.ExploderInstance != null) {
            ExploderSingleton.ExploderInstance.ExplodeObject(gameObject);
        } else {
            Destroy(gameObject);
        }
    }
}
