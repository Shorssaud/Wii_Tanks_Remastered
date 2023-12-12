using System.Diagnostics;
using UnityEngine;
using UnityEngine.AI;
using Unity.MLAgents;
using Unity.MLAgents.Sensors;
using Unity.MLAgents.Actuators;
using Exploder.Utils;
using UnityEngine.UIElements;

public class TankAIMachineLearning : Agent
{
    // From this
    private float prevTankToTargetDistance;
    private float tankToTargetDistance;
    public Rigidbody rb;
    float maxRaycastDistance = 50.0f;
    public GameObject otherTankPrefab;
    private GameObject otherTank;
    Stopwatch stopwatch = new Stopwatch();

    // From TankAIBrown
    private Quaternion currentCannonRot;
    private Transform cannon;
    private Transform bulletSpawn;

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

    public SphereCollider tankCollider;

    public GameObject arena;


    // Start is called before the first frame update
    void Start()
    {
        cannon = transform.Find("cannon");
        nextFire = 0;
        nextMine = 0;
        currentBullets = 0;
        shotPause = 0;
        currentCannonRot = cannon.rotation;
        arena = this.transform.parent.gameObject;
    }

    private void Update()
    {
        nextFire -= Time.deltaTime; // Decrement the nextFire timer
        nextMine -= Time.deltaTime; // Decrement the nextMine timer
    }

    public override void OnEpisodeBegin()
    {
        // Create the player tank again
        if (!otherTank)
        {
            otherTank = Instantiate(otherTankPrefab, transform.position, Quaternion.identity);
            otherTank.transform.SetParent(transform.parent);
            otherTank.transform.localPosition = new Vector3(-100.0f, 0.0f,1.5f);

            cannon = transform.Find("cannon");
            bulletSpawn = cannon.Find("bulletSpawn");
        }
        stopwatch.Reset();
        stopwatch.Start();
        this.rb.angularVelocity = Vector3.zero;
        this.rb.velocity = Vector3.zero;
        this.rb.rotation = Quaternion.identity;
        this.transform.localPosition = new Vector3(0, 0, 0);
        // destroy all bullets in the arena gameobject
        foreach (Transform child in arena.transform)
        {
            if (child.gameObject.tag == "Bullet" || child.gameObject.tag == "Mine")
            {
                Destroy(child.gameObject);
            }
        }

        prevTankToTargetDistance = tankToTargetDistance;
        tankToTargetDistance = Vector3.Distance(this.transform.localPosition, otherTank.transform.localPosition);
    }

    public override void CollectObservations(VectorSensor sensor)
    {
        // Target and Agent positions
        if (otherTank)
        {
            sensor.AddObservation(otherTank.transform.localPosition);
            sensor.AddObservation(this.transform.localPosition);
        }
        else
        {
            sensor.AddObservation(new Vector3(0, 0.0f, 0f));
            sensor.AddObservation(this.transform.localPosition);
        }
        sensor.AddObservation(this.transform.localPosition);
    }
    public override void Heuristic(in ActionBuffers actionsOut)
    {
        // Get movement input from the MlAgents system
        actionsOut.ContinuousActions.Array[0] = Input.GetAxis("Horizontal");
        actionsOut.ContinuousActions.Array[1] = Input.GetAxis("Vertical");
        actionsOut.ContinuousActions.Array[2] = Input.GetAxis("Forward");
        // Get turret rotation input from the MlAgents system
        actionsOut.ContinuousActions.Array[3] = Input.GetAxis("CannonHorizontal");

        actionsOut.ContinuousActions.Array[4] = Input.GetAxis("Shoot");
        actionsOut.ContinuousActions.Array[5] = Input.GetAxis("PlaceMine");
    }

    public override void OnActionReceived(ActionBuffers actions)
    {
        Move(actions.ContinuousActions[0], actions.ContinuousActions[1], actions.ContinuousActions[2]);
        RotateCannon(actions.ContinuousActions[3]);
        if (actions.ContinuousActions[4] > 0.9)
        {
            Shoot(bulletSpawn);
            
        }
        if (actions.ContinuousActions.Array[5] > 0.9f)
        {
            PlaceMine();
        }
        // Get all colliders overlapping the CheckSphere
        Collider[] colliders = Physics.OverlapSphere(this.transform.position, tankCollider.radius / 2, LayerMask.GetMask("Default"));
        //show this sphere in debug

        // Iterate through the colliders to check if any have the tank's own tag
        foreach (Collider collider in colliders)
        {
            if (collider.gameObject.tag == "Wall" || collider.gameObject.tag == "WallBreakable")
            {
                AddReward(-0.01f);
                //print("Tank Collided With Wall");
                break;
            }
        }
        if (SimulateBouncingRay(transform.position, cannon.forward, bulletRicochetMax) != 0)
        {
            AddReward(0.01f);
        }
        if (!otherTank)
        {
            AddReward(10.0f);
            printReward();
            EndEpisode();
        }
        if (stopwatch.ElapsedMilliseconds > 7000)
        {
            print("Tank ran out of time");
            printReward();
            EndEpisode();
        }
        prevTankToTargetDistance = tankToTargetDistance;
        tankToTargetDistance = Vector3.Distance(this.transform.localPosition, otherTank.transform.localPosition);
    }

    private void printReward()
    {
        UnityEngine.Debug.Log(GetCumulativeReward());
    }


    private void Move(float horizontal, float vertical, float forward)
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

        // Calculate the otherTank.transform rotation based on the angle
        targetRotation = Quaternion.Euler(0f, angleDegrees, 0f);
        // if the otherTank.transform rotation is more than 90 degrees from the current rotation rotate the rear instead
        if (transform.rotation.eulerAngles.y - targetRotation.eulerAngles.y > 90 || transform.rotation.eulerAngles.y - targetRotation.eulerAngles.y < -90)
        {
            // Calculate the otherTank.transform rotation based on the angle
            targetRotation = Quaternion.Euler(0f, angleDegrees + 180, 0f);
        }
        // Set the rotation of the rigidbody to the otherTank.transform rotation
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
        if (forward > 0.1f)
            transform.position += vel * Time.deltaTime * forward;
    }

    private void RotateCannon(float horizontal)
    {
        cannon.rotation = currentCannonRot; //Always keep the cannon facing the desired direction
        float angle = horizontal * 360;
        Quaternion newRotation = Quaternion.Euler(0f, angle, -90f);
        float yRotation = newRotation.eulerAngles.y;
        // Set the cannon's rotation
        // Preserve the current X and Z rotations, only change the Y rotation
        cannon.rotation = Quaternion.Euler(cannon.rotation.eulerAngles.x, yRotation, cannon.rotation.eulerAngles.z);
        currentCannonRot = cannon.rotation;
    }

    // Shoots a basic bullet
    private void Shoot(Transform bulletSpawn)
    {
        if (nextFire > 0 || currentBullets >= maxBullets)
            return;
        AddReward(0.1f);
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
        
        //make the bullet a child of the arena
        bullet.transform.SetParent(transform.parent);

        // Apply the tanks bullet settings
        bullet.GetComponent<BulletBase>().speed = bulletSpeed;
        bullet.GetComponent<BulletBase>().ricochetMax = bulletRicochetMax;

        //set the parent tank
        bullet.GetComponent<BulletBase>().parentTank = gameObject;

        // reset firing timer
        nextFire = fireRate;

        //FindObjectOfType<AudioManager>().Play("TankShot");
    }

    // Places a mine at the position of the tank
    private void PlaceMine()
    {
        if (nextMine > 0 || currentMines >= maxMines)
            return;
        AddReward(0.05f);
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
        // set the parent to the arena
        mine.transform.SetParent(transform.parent);
        //set the parent tank
        mine.GetComponent<Mine>().parentTank = gameObject;

        // increase mine count
        currentMines++;

        // reset mine timer
        nextMine = mineRate;
    }

    int SimulateBouncingRay(Vector3 origin, Vector3 direction, int remainingBounces)
    {
        // Perform a raycast
        RaycastHit hit;
        if (Physics.Raycast(origin, direction, out hit))
        {
            // Check if the ray hit the player
            if (hit.collider.CompareTag("Player"))
                return 0;
            if (hit.collider.CompareTag("AI"))
            {
                // Perform actions when the player is hit by the bouncing ray
                UnityEngine.Debug.DrawRay(origin, direction * 50, Color.red);
                return 1;
            }
            UnityEngine.Debug.DrawRay(origin, direction * 50, Color.blue);
            // If the ray hit a wall, calculate the reflection direction
            Vector3 reflection = Vector3.Reflect(direction, hit.normal);

            // Continue the ray with the reflection direction
            if (remainingBounces > 0)
            {
                int temp = SimulateBouncingRay(hit.point, reflection, remainingBounces - 1);
                if (temp != 0)
                    return 1 + temp;
            }
        }
        return 0;
    }

    public void RemoveBullet()
    {
        currentBullets -= 1;
    }

    public void RemoveMine()
    {
        currentMines -= 1;
    }

    public void DestroyTank(float explosionSize = 1.0f)
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
}
