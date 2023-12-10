using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using UnityEngine;
using UnityEngine.AI;
using Unity.MLAgents;
using Unity.MLAgents.Sensors;
using Unity.MLAgents.Actuators;
using Exploder.Utils;


public class TankAIMachineLearning : Agent
{
    // From this
    public Transform target;
    private float prevTankToTargetDistance;
    private float tankToTargetDistance;
    public Rigidbody rb;

    // From TankAIBrown
    private GameObject player;
    private Quaternion currentCannonRot;
    private Transform cannon;
    private Transform bulletSpawn;
    private Quaternion aimAngle;
    Stopwatch stopwatch = new Stopwatch();

    // From TankBase
    public float maxSpeed;
    public float rotSpeed;
    public float bulletSpeed;
    public int bulletRicochetMax;
    public int maxBullets;
    public int maxMines;
    public float fireRate;
    public float mineRate;

    private int currentBullets;
    private int currentMines;
    private float nextFire;
    private float nextMine;
    private int shotPause;

    public GameObject explosionPrefab;
    private Vector3 vel = Vector3.zero;
    private Quaternion targetRotation;
    public GameObject bulletPrefab;
    public GameObject minePrefab;
    public TrailRenderer leftTrailRenderer;
    public TrailRenderer rightTrailRenderer;


    // Start is called before the first frame update
    void Start()
    {
        player = GameObject.FindGameObjectWithTag("Player");
        cannon = transform.Find("cannon");
        bulletSpawn = cannon.Find("bulletSpawn");

        currentCannonRot = cannon.rotation;
        aimAngle = currentCannonRot;
    }

    private void FixedUpdate()
    {
        nextFire -= Time.deltaTime; // Decrement the nextFire timer
        nextMine -= Time.deltaTime; // Decrement the nextMine timer
    }

    public override void OnEpisodeBegin()
    {
        stopwatch.Reset();
        stopwatch.Start();
        this.rb.angularVelocity = Vector3.zero;
        this.rb.velocity = Vector3.zero;
        this.rb.rotation = Quaternion.identity;
        this.transform.position = new Vector3(18.8f, 0.0f, 24.0f);

        prevTankToTargetDistance = tankToTargetDistance;
        tankToTargetDistance = Vector3.Distance(this.transform.localPosition, target.localPosition);
    }

    public override void CollectObservations(VectorSensor sensor)
    {
        // Target and Agent positions
        sensor.AddObservation(target.localPosition);
        sensor.AddObservation(this.transform.localPosition);
    }

    public override void OnActionReceived(ActionBuffers actions)
    {
        Move(actions.ContinuousActions[0], actions.ContinuousActions[1]);

        /*if (actions.ContinuousActions.Array[2] == 1.0f)
        {
            PlaceMine();
        }*/
        /*if (actions.ContinuousActions.Array[3] == 1.0f)
        {
            Shoot(bulletSpawn);
        }*/
        // punish the longer it takes
        if (true)
        {
            AddReward(-0.0001f);
        }
        if (tankToTargetDistance < prevTankToTargetDistance)
        {
            AddReward(0.01f);
        }
        if (tankToTargetDistance > prevTankToTargetDistance)
        {
            AddReward(-0.02f);
        }
        if (HasDirectVision())
        {
            UnityEngine.Debug.Log("Has vision of target");
            AddReward(10.0f);
            printReward();
            EndEpisode();
        }
        /*if (tankToTargetDistance < 9.0f)
        {
            UnityEngine.Debug.Log("Tank reached the target");
            AddReward(10.0f);
            printReward();
            EndEpisode();
        }*/
        /*if (stopwatch.ElapsedMilliseconds > 10000)
        {
            UnityEngine.Debug.Log("Tank ran out of time");
            AddReward(-10.0f);
            printReward();
            EndEpisode();
        }*/
        prevTankToTargetDistance = tankToTargetDistance;
        tankToTargetDistance = Vector3.Distance(this.transform.localPosition, target.localPosition);
    }

    private void printReward()
    {
        UnityEngine.Debug.Log(GetCumulativeReward());
    }

    public override void Heuristic(in ActionBuffers actionsOut)
    {
        // Get movement input from the MlAgents system
        actionsOut.ContinuousActions.Array[0] = Input.GetAxis("Vertical");
        actionsOut.ContinuousActions.Array[1] = Input.GetAxis("Horizontal");
        /*actionsOut.ContinuousActions.Array[2] = Input.GetKey(KeyCode.Space) ? 1.0f : 0.0f;
        actionsOut.ContinuousActions.Array[3] = Input.GetKey(KeyCode.Mouse0) ? 1.0f : 0.0f;*/
    }

    bool IsOutsideNavMesh(Vector3 position)
    {
        NavMeshHit hit;
        return !NavMesh.SamplePosition(position, out hit, 0.1f, NavMesh.AllAreas);
    }

    private void Move(float horizontal, float vertical)
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
        // Set the rotation of the rigidbody to the target rotation
        transform.rotation = Quaternion.RotateTowards(transform.rotation, targetRotation, rotSpeed * Time.deltaTime);
        if (shotPause > 0)
        {
            shotPause--;
            return;
        }
        //Checking for wall collisions
        //calculate the tanks future position after moving
        Vector3 futurePosition = transform.position + vel * Time.deltaTime;
        // Get the tank's collider to measure its size
        SphereCollider tankCollider = GetComponent<SphereCollider>();

        // Define the layer mask for the "Default" layer 
        LayerMask defaultLayerMask = LayerMask.GetMask("Default");

        // Ignore collisions between the tank's collider and itself
        Physics.IgnoreCollision(tankCollider, tankCollider);

        // Perform a check to see if the tank would collide with any object in default layer at the future position
        if (Physics.CheckSphere(futurePosition, tankCollider.radius, defaultLayerMask) && vel != Vector3.zero)
        {
            // Get all colliders overlapping the CheckSphere
            Collider[] colliders = Physics.OverlapSphere(futurePosition, tankCollider.radius / 4, defaultLayerMask);

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

        if (vel != Vector3.zero)
        {
            // Enable the trail renderers if the tank is moving
            if (leftTrailRenderer != null) leftTrailRenderer.emitting = true;
            if (rightTrailRenderer != null) rightTrailRenderer.emitting = true;
        }
        else
        {
            // Disable the trail renderers if the tank is not moving
            if (leftTrailRenderer != null) leftTrailRenderer.emitting = false;
            if (rightTrailRenderer != null) rightTrailRenderer.emitting = false;
        }

        // Move the tank while avoiding wall collisions
        transform.position += vel * Time.deltaTime;


    }

    // Shoots a basic bullet
    private void Shoot(Transform bulletSpawn)
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
        shotPause = 50;
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
    private void PlaceMine()
    {
        if (nextMine > 0 || currentMines >= maxMines)
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

        // increase mine count
        currentMines++;

        // reset mine timer
        nextMine = mineRate;
    }


    public void RemoveBullet()
    {
        currentBullets -= 1;
    }

    public void RemoveMine()
    {
        currentMines -= 1;
    }

    virtual public void DestroyTank(float explosionSize = 1.0f)
    {
        UnityEngine.Debug.Log("Tank was destroyed");
        AddReward(-10.0f);
        printReward();
        EndEpisode();
        return;
        if (explosionPrefab != null)
        {
            GameObject explosion = Instantiate(explosionPrefab, transform.position, Quaternion.identity);
            explosion.transform.localScale *= explosionSize; // Scale the explosion effect
        }
        if (ExploderSingleton.ExploderInstance != null)
        {
            ExploderSingleton.ExploderInstance.ExplodeObject(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }
    private bool HasDirectVision()
    {
        // Calculate the direction from this object to the target
        Vector3 direction = target.position - transform.position;

        // Create a ray from this object's position in the direction of the target
        Ray ray = new Ray(transform.position, direction);

        // Set the maximum distance for the raycast (adjust as needed)
        float maxDistance = direction.magnitude;

        // Perform the raycast
        RaycastHit hit;
        bool hitSomething = Physics.Raycast(ray, out hit, maxDistance);
        UnityEngine.Debug.DrawRay(ray.origin, ray.direction * hit.distance, Color.red, 1.0f);

        // Check if the ray hit the target or if there was an obstacle in between
        return hitSomething && hit.transform == target;
    }
}
