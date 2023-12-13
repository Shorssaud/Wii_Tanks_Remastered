using System.Diagnostics;
using Unity.MLAgents;
using Unity.MLAgents.Actuators;
using Unity.MLAgents.Sensors;
using UnityEngine;

public class TankAIMachineLearningClean : Agent
{
    public GameObject otherTankPrefab;
    private GameObject otherTank;
    private Stopwatch stopwatch = new Stopwatch();

    private Quaternion currentCannonRot;
    private Transform cannon;
    private Transform bulletSpawn;

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

    private Vector3 prevBullLand;

    public GameObject[] levels;
    public int currentLevel = 0;

    void Start()
    {
        InitializeTank();
    }

    private void Update()
    {
        UpdateTimers();
    }

    public override void OnEpisodeBegin()
    {
        ResetTank();
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
            sensor.AddObservation(Vector3.zero);
            sensor.AddObservation(Vector3.zero);
        }

        sensor.AddObservation(transform.localPosition);
        sensor.AddObservation(vel);
        sensor.AddObservation(cannon.rotation);
        sensor.AddObservation(SimulateBouncingRay(transform.position, cannon.forward, bulletRicochetMax));
    }

    public override void Heuristic(in ActionBuffers actionsOut)
    {
    }

    public override void OnActionReceived(ActionBuffers actions)
    {
        Move(actions.DiscreteActions[0], actions.DiscreteActions[1]);
        RotateCannon(actions.DiscreteActions[2]);
        if (actions.DiscreteActions[3] == 1) Shoot(bulletSpawn);
        if (actions.DiscreteActions[4] == 1) PlaceMine();
        CheckCollisionsAndRewards();
    }

    private void InitializeTank()
    {
        cannon = transform.Find("cannon");
        bulletSpawn = cannon.Find("bulletSpawn");
        nextFire = 0;
        nextMine = 0;
        currentBullets = 0;
        shotPause = 0;
        currentCannonRot = cannon.rotation;
        arena = transform.parent.gameObject;
        prevBullLand = Vector3.zero;
    }

    private void UpdateTimers()
    {
        nextFire -= Time.deltaTime;
        nextMine -= Time.deltaTime;
    }

    private void ResetTank()
    {
        SetTankParent();
        DestroyExistingOtherTank();
        InstantiateAndPositionOtherTank();
        SetCannonAndBulletSpawn();
        ResetAndStartStopwatch();
        ResetTankRotationAndPosition();
        ClearArena();
    }

    private void SetTankParent()
    {
        transform.SetParent(levels[currentLevel].transform);
    }

    private void DestroyExistingOtherTank()
    {
        if (otherTank)
            Destroy(otherTank);
    }

    private void InstantiateAndPositionOtherTank()
    {
        otherTank = Instantiate(otherTankPrefab, transform.position, Quaternion.identity);
        otherTank.transform.SetParent(transform.parent);
        otherTank.transform.localPosition = levels[currentLevel].transform.Find("AISpawn").localPosition;
    }

    private void SetCannonAndBulletSpawn()
    {
        cannon = transform.Find("cannon");
        bulletSpawn = cannon.Find("bulletSpawn");
    }

    private void ResetAndStartStopwatch()
    {
        stopwatch.Reset();
        stopwatch.Start();
    }

    private void ResetTankRotationAndPosition()
    {
        transform.rotation = Quaternion.identity;
        transform.localPosition = levels[currentLevel].transform.Find("MLSpawn").localPosition;
    }

    private void ClearArena()
    {
        foreach (Transform child in arena.transform)
        {
            if (child.gameObject.CompareTag("Bullet") || child.gameObject.CompareTag("Mine"))
            {
                Destroy(child.gameObject);
            }
        }
    }

    private void CheckCollisionsAndRewards()
    {
        CheckWallCollisions();
        CheckBouncingRayReward();
        CheckOtherTankCollision();
        CheckTimeLimit();
    }

    private void CheckWallCollisions()
    {
        Collider[] colliders = Physics.OverlapSphere(transform.position, tankCollider.radius / 2, LayerMask.GetMask("Default"));

        foreach (Collider collider in colliders)
        {
            if (collider.CompareTag("Wall") || collider.CompareTag("WallBreakable"))
            {
                AddReward(-0.01f);
                break;
            }
        }
    }

    private void CheckBouncingRayReward()
    {
        if (SimulateBouncingRay(transform.position, cannon.forward, bulletRicochetMax) != 0)
            AddReward(0.0001f);
    }

    private void CheckOtherTankCollision()
    {
        if (!otherTank)
        {
            AddReward(100.0f);
            CheckForMoveOn(GetCumulativeReward());
            EndEpisode();
        }
    }

    private void CheckTimeLimit()
    {
        if (stopwatch.ElapsedMilliseconds > 30000)
        {
            CheckForMoveOn(GetCumulativeReward());
            EndEpisode();
        }
    }

    private void Move(int horizontal, int forward)
    {
        RotateTank(horizontal);
        HandleShotPause();

        HandleCollisionAvoidance(CalculateFuturePosition());
        UpdateTrailRenderer();
        MoveTankForward(forward);
    }

    private void RotateTank(int horizontal)
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
    }
    private void HandleShotPause()
    {
        if (shotPause > 0)
            shotPause--;
    }

    private Vector3 CalculateFuturePosition()
    {
        return transform.position + vel * Time.deltaTime;
    }

    private void HandleCollisionAvoidance(Vector3 futurePosition)
    {
        SphereCollider tankCollider = GetComponent<SphereCollider>();
        LayerMask defaultLayerMask = LayerMask.GetMask("Default");

        Physics.IgnoreCollision(tankCollider, tankCollider);

        if (Physics.CheckSphere(futurePosition, tankCollider.radius, defaultLayerMask) && vel != Vector3.zero)
        {
            Collider[] colliders = Physics.OverlapSphere(futurePosition, tankCollider.radius / 4, defaultLayerMask);

            foreach (Collider collider in colliders)
            {
                if (collider.gameObject == gameObject)
                {
                    Physics.IgnoreCollision(tankCollider, collider, true);
                }
                else
                {
                    vel = Vector3.zero;
                }
            }
        }
    }

    private void UpdateTrailRenderer()
    {
        if (vel != Vector3.zero)
        {
            if (leftTrailRenderer != null)
                leftTrailRenderer.emitting = true;
            if (rightTrailRenderer != null)
                rightTrailRenderer.emitting = true;
        }
        else
        {
            if (leftTrailRenderer != null)
                leftTrailRenderer.emitting = false;
            if (rightTrailRenderer != null)
                rightTrailRenderer.emitting = false;
        }
    }

    private void MoveTankForward(int forward)
    {
        if (forward == 1)
        {
            transform.position += vel * Time.deltaTime;
            AddReward(0.0001f);
        }
    }

    private void RotateCannon(int horizontal)
    {
        cannon.rotation = currentCannonRot;
        if (horizontal == 1)
        {
            cannon.Rotate(0f, rotSpeed * Time.deltaTime, 0f, Space.World);
        }
        if (horizontal == 2)
        {
            cannon.Rotate(0f, -rotSpeed * Time.deltaTime, 0f, Space.World);
        }
        currentCannonRot = cannon.rotation;
    }

    private void Shoot(Transform bulletSpawn)
    {
        // Check if the tank can shoot
        if (nextFire > 0 || currentBullets >= maxBullets)
            return;

        if (ObstacleInFiringPath(bulletSpawn))
        {
            AddReward(-0.01f);
            return;
        }
        AddReward(0.001f);
        shotPause = 50;
        currentBullets++;
        InstantiateBullet(bulletSpawn);
        nextFire = fireRate;
    }

    private bool ObstacleInFiringPath(Transform bulletSpawn)
    {
        // Check for obstacles in the firing path using a CheckBox
        if (Physics.CheckBox(bulletSpawn.position, bulletSpawn.localScale / 2, bulletSpawn.rotation, LayerMask.GetMask("Default")))
        {
            // Check for specific colliders in the firing path
            Collider[] colliders = Physics.OverlapBox(bulletSpawn.position, bulletSpawn.localScale / 2, bulletSpawn.rotation, LayerMask.GetMask("Default"));

            foreach (Collider collider in colliders)
            {
                if (collider.gameObject.CompareTag("Wall"))
                {
                    return true;
                }
            }
        }
        return false;
    }

    private void InstantiateBullet(Transform bulletSpawn)
    {
        GameObject bullet = Instantiate(bulletPrefab, bulletSpawn.position, bulletSpawn.rotation);
        bullet.transform.SetParent(transform.parent);
        BulletBase bulletComponent = bullet.GetComponent<BulletBase>();
        bulletComponent.speed = bulletSpeed;
        bulletComponent.ricochetMax = bulletRicochetMax;
        bulletComponent.parentTank = gameObject;
    }

    private void PlaceMine()
    {
        if (nextMine > 0 || currentMines >= maxMines)
            return;
        AddReward(0.05f);
        if (ObstacleInPlacementArea())
            return;

        CreateMine();
        currentMines++;
        nextMine = mineRate;
    }

    private bool ObstacleInPlacementArea()
    {
        // Check for obstacles in the mine placement area using a CheckBox
        Collider[] colliders = Physics.OverlapBox(transform.position, transform.localScale / 2, transform.rotation, LayerMask.GetMask("Default"));

        foreach (Collider collider in colliders)
        {
            if (collider.gameObject.CompareTag("Wall") || collider.gameObject.CompareTag("Mine"))
            {
                return true;
            }
        }

        return false;
    }

    private GameObject CreateMine()
    {
        GameObject mine = Instantiate(minePrefab, transform.position, transform.rotation);

        mine.transform.SetParent(transform.parent);
        mine.GetComponent<Mine>().parentTank = gameObject;
        return mine;

    }

    private int SimulateBouncingRay(Vector3 origin, Vector3 direction, int remainingBounces)
    {
        int reflectionCount = 0;
        RaycastHit hit;

        // Check for ray collision
        if (Physics.Raycast(origin, direction, out hit, 500))
        {
            reflectionCount = HandleSpecialCollisions(origin, direction, hit, remainingBounces);
        }

        return reflectionCount;
    }

    private int HandleSpecialCollisions(Vector3 origin, Vector3 direction, RaycastHit hit, int remainingBounces)
    {
        if (IsPlayer(hit))
        {
            return -1;
        }
        if (IsAI(hit))
        {
            VisualizeRayPath(origin, direction, Color.red);
            return 1;
        }
        VisualizeRayPath(origin, direction, Color.blue);

        // Calculate the reflection direction
        Vector3 reflection = Vector3.Reflect(direction, hit.normal);

        // Recursively simulate bouncing ray if remaining bounces are positive
        return SimulateBouncingRayRecursively(hit.point, reflection, remainingBounces);
    }

    private bool IsPlayer(RaycastHit hit)
    {
        return hit.collider.CompareTag("Player");
    }

    private bool IsAI(RaycastHit hit)
    {
        return hit.collider.CompareTag("AI");
    }

    private int SimulateBouncingRayRecursively(Vector3 hitPoint, Vector3 reflection, int remainingBounces)
    {
        int reflectionCount = 0;

        if (remainingBounces > 0)
        {
            reflectionCount = SimulateBouncingRay(hitPoint, reflection, remainingBounces - 1);
            if (reflectionCount != 0)
            {
                return 1 + reflectionCount;
            }
        }

        return reflectionCount;
    }

    public void RemoveBullet(Vector3 pos)
    {
        if (Vector3.Distance(pos, otherTank.transform.localPosition) > Vector3.Distance(prevBullLand, otherTank.transform.localPosition))
        {
            prevBullLand = pos;
            AddReward(0.1f);
        }
        currentBullets -= 1;
    }

    public void RemoveMine()
    {
        currentMines -= 1;
    }

    public void DestroyTank(float explosionSize = 1.0f)
    {
        if (currentLevel > 0)
        {
            currentLevel--;
            UnityEngine.Debug.Log("Score too low, sending back");
            return;
        }
        AddReward(-100.0f);
        EndEpisode();
    }

    private void CheckForMoveOn(float currentReward)
    {
        if (currentReward > 100)
        {
            if (currentLevel < levels.Length - 1)
            {
                currentLevel++;
                UnityEngine.Debug.Log("Moving on to level " + currentLevel);
                return;
            }
        }
        if (currentLevel > 0 && currentReward < 50)
        {
            currentLevel--;
            UnityEngine.Debug.Log("Score too low, sending back");
        }
    }

    private void VisualizeRayPath(Vector3 origin, Vector3 direction, Color color)
    {
        UnityEngine.Debug.DrawRay(origin, direction * 50, color);
    }
}