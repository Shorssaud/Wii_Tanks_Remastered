using System.Diagnostics;
using Unity.MLAgents;
using Unity.MLAgents.Actuators;
using Unity.MLAgents.Sensors;
using UnityEngine;

public class TankAIMachineLearning : Agent
{
    // From this
    private float prevTankToTargetDistance;
    private float tankToTargetDistance;
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

    private int doesHit = 0;
    private Vector3 prevBullLand;
    private int bulletsShot = 0;
    // Advanced mods
    private int timer = 7000;

    // different levels
    public GameObject[] levels;
    public int currentLevel = 0;
    private int consecutiveWins = 0;


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
        prevBullLand = Vector3.zero;
        cannon = transform.Find("cannon");
        bulletSpawn = cannon.Find("bulletSpawn");
    }

    private void Update()
    {
        nextFire -= Time.deltaTime; // Decrement the nextFire timer
        nextMine -= Time.deltaTime; // Decrement the nextMine timer
    }

    public override void OnEpisodeBegin()
    {
        if (currentLevel == 0)
        {
            timer = 7000;
        }
        else
        {
            timer = 30000;
        }
        this.transform.SetParent(levels[currentLevel].transform);
        // Create the player tank again
        if (!otherTank)
        {
            otherTank = Instantiate(otherTankPrefab, transform.position, Quaternion.identity);
        }
        otherTank.transform.SetParent(transform.parent);
        otherTank.transform.localPosition = levels[currentLevel].transform.Find("AISpawn").transform.localPosition;

        stopwatch.Reset();
        stopwatch.Start();
        this.transform.rotation = Quaternion.identity;
        this.transform.localPosition = levels[currentLevel].transform.Find("MLSpawn").transform.localPosition;

        cannon.rotation = Quaternion.Euler(0, -90, -90);
        currentCannonRot = cannon.rotation;
        prevTankToTargetDistance = tankToTargetDistance;
        tankToTargetDistance = Vector3.Distance(this.transform.localPosition, otherTank.transform.localPosition);
    }

    public override void CollectObservations(VectorSensor sensor)
    {
        if (otherTank)
        {
            sensor.AddObservation(otherTank.transform.localPosition);
            sensor.AddObservation(otherTank.GetComponent<Tank>().vel);
        }
        else
        {
            sensor.AddObservation(levels[currentLevel].transform.Find("AISpawn").transform.localPosition);
            sensor.AddObservation(Vector3.zero);
        }
        sensor.AddObservation(this.transform.localPosition);
        sensor.AddObservation(this.vel);
        sensor.AddObservation(cannon.localRotation.y);
    }
    public override void Heuristic(in ActionBuffers actionsOut)
    {
    }

    public override void OnActionReceived(ActionBuffers actions)
    {
        //print(actions.DiscreteActions.Array[0] + " " + actions.DiscreteActions.Array[1] + " " + actions.DiscreteActions.Array[2] + " " + actions.DiscreteActions.Array[3] + " " + actions.DiscreteActions.Array[4]);
        if (actions.DiscreteActions[3] == 1)
        {
            Shoot(bulletSpawn);
            bulletsShot++;
        }
        if (actions.DiscreteActions.Array[4] == 1)
        {
            PlaceMine();
        }
        Move(actions.DiscreteActions[0], actions.DiscreteActions[1]);
        RotateCannon(actions.DiscreteActions[2]);
        // Get all colliders overlapping the CheckSphere
        Collider[] colliders = Physics.OverlapSphere(this.transform.position, tankCollider.radius / 2, LayerMask.GetMask("Default"));
        //show this sphere in debug

        // Iterate through the colliders to check if any have the tank's own tag
        foreach (Collider collider in colliders)
        {
            if (collider.gameObject.tag == "Wall" || collider.gameObject.tag == "WallBreakable")
            {
                AddReward(-0.1f);
                //print("Tank Collided With Wall");
                break;
            }
        }
        if (stopwatch.ElapsedMilliseconds > timer)
        {
            CheckForMoveOn(GetCumulativeReward());
            consecutiveWins = 0;
            EndEpisode();
        }
        if (!otherTank)
        {
            AddReward(15.0f);
            consecutiveWins++;
            CheckForMoveOn(GetCumulativeReward());
            EndEpisode();
        }
    }

    private void printReward()
    {
        UnityEngine.Debug.Log(GetCumulativeReward());
    }


    private void Move(int horizontal, int forward)
    {
        // if horizontal is 1, rotate right
        if (horizontal == 1)
        {
            // rotate the tank right 
            transform.Rotate(0f, rotSpeed * Time.deltaTime, 0f, Space.World);
        }
        // if horizontal is 2, rotate left
        if (horizontal == 2)
        {
            // rotate the tank left
            transform.Rotate(0f, -rotSpeed * Time.deltaTime, 0f, Space.World);
        }

        // Apply movement based on input and speed using the direction vector
        vel = transform.forward * maxSpeed;

        
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
        if (forward == 1)
        {
            transform.position += vel * Time.deltaTime;
            //AddReward(0.001f);
        }
    }

    private void RotateCannon(float horizontal)
    {
        cannon.rotation = currentCannonRot; //Always keep the cannon facing the desired direction
        if (horizontal == 1)
        {
            // rotate the cannon right 
            cannon.Rotate(0f, rotSpeed / 2 * Time.deltaTime, 0f, Space.World);
        }
        if (horizontal == 2)
        {
            // rotate the cannon left
            cannon.Rotate(0f, -rotSpeed / 2 * Time.deltaTime, 0f, Space.World);
        }
        currentCannonRot = cannon.rotation;
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
                    AddReward(-0.01f);
                    return;
                }
            }
        }
        //AddReward(-0.1f);
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

    public void RemoveBullet(Vector3 pos)
    {
        //if (Vector3.Distance(pos, otherTank.transform.localPosition) > Vector3.Distance(prevBullLand, otherTank.transform.localPosition)) ;
        //{
        //    prevBullLand = pos;
        //    //AddReward(0.1f);
        //}
        currentBullets -= 1;
    }

    public void RemoveMine()
    {
        currentMines -= 1;
    }

    public void DestroyTank(float explosionSize = 1.0f)
    {
        consecutiveWins = 0;
        //AddReward(-1.0f);
        CheckForMoveOn(GetCumulativeReward());
        EndEpisode();
        return;
        //if (explosionPrefab != null)
        //{
        //    GameObject explosion = Instantiate(explosionPrefab, transform.position, Quaternion.identity);
        //    explosion.transform.localScale *= explosionSize; // Scale the explosion effect
        //}
        //if (ExploderSingleton.ExploderInstance != null)
        //{
        //    ExploderSingleton.ExploderInstance.ExplodeObject(gameObject);
        //}
        //else
        //{
        //    Destroy(gameObject);
        //}
    }

    private void CheckForMoveOn(float currentReward)
    {
        ResetValues();
        if (currentReward >= 10)
        {
            if (currentLevel < levels.Length - 1 && consecutiveWins >= 10)
            {
                consecutiveWins = 0;
                currentLevel++;
                UnityEngine.Debug.Log("Moving on to level " + currentLevel);
                return;
            }
        }
        if (currentLevel > 0 && currentReward < 5)
        {
            currentLevel--;
            UnityEngine.Debug.Log("Score too low, sending back");
            return;
        }
        return;
    }
    private void ResetValues()
    {
        bulletsShot = 0;
        nextFire = fireRate;
        foreach (Transform child in arena.transform)
        {
            if (child.gameObject.tag == "Bullet" || child.gameObject.tag == "Mine")
            {
                Destroy(child.gameObject);
            }
        }
    }
}
