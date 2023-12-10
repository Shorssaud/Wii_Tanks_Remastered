using UnityEngine;
using UnityEngine.AI;


// This class can be used as a reference for other AI tanks
public class TankAIGreen : Tank
{
    private GameObject player;
    private NavMeshAgent agent;
    private float stationaryTime = 0f;
    private float movementDecisionInterval = 2f;
    private Quaternion currentCannonRot;

    private Transform cannon;
    private Transform bulletSpawn;

    private float aimAngle;
    private float currentFanAngle;

    public GameObject Player { get => player; set => player = value; }
    public NavMeshAgent Agent { get => agent; set => agent = value; }
    public float StationaryTime { get => stationaryTime; set => stationaryTime = value; }
    public float MovementDecisionInterval { get => movementDecisionInterval; set => movementDecisionInterval = value; }
    public Quaternion CurrentCannonRot { get => currentCannonRot; set => currentCannonRot = value; }
    public Transform Cannon { get => cannon; set => cannon = value; }
    public Transform BulletSpawn { get => bulletSpawn; set => bulletSpawn = value; }
    public float AimAngle { get => aimAngle; set => aimAngle = value; }
    public float CurrentFanAngle { get => currentFanAngle; set => currentFanAngle = value; }


    // Start is called before the first frame update
    void Start()
    {
        Player = GameObject.FindGameObjectWithTag("Player");
        Agent = GetComponent<NavMeshAgent>();
        Cannon = transform.Find("cannon");
        BulletSpawn = Cannon.Find("bulletSpawn");

        CurrentCannonRot = Cannon.rotation;

        Agent.speed = maxSpeed;
        CurrentFanAngle = 45;
    }

    // Update is called once per frame
    void Update()
    {
        AimAndShoot();
        StationaryTime += Time.deltaTime;
    }

    // This function can be changed to make the AI tank aim and shoot differently
    private void AimAndShoot()
    {
        // Before doing anything, check if the player is still active
        if (Player == null)
        {
            return; // If not, don't aim or shoot
        }

        if (!ThinAngleSearch())
        {
            FanSearch();
        }
        if (HasLineOfSightToPlayer())
        {
            // If the player is in line of sight, aim at the player
            AimAngle = Quaternion.LookRotation(Player.transform.position - transform.position).eulerAngles.y;
        }

        Cannon.rotation = CurrentCannonRot; //Always keep the cannon facing the desired direction
                                            // cast a ray in 16 directions which bounces off 2 walls

        // Rotate turret slowly towards player only on the Y axis
        Quaternion aimQuat = Quaternion.Euler(0, AimAngle, -90);
        Cannon.rotation = Quaternion.Euler(CurrentCannonRot.x,
                                           Quaternion.RotateTowards(Cannon.rotation, aimQuat, rotSpeed * Time.deltaTime).eulerAngles.y,
                                           -90);
        CurrentCannonRot = Cannon.rotation;

        // if the cannon is aiming close enough to the aimQuat, shoot
        if (SimulateBouncingRay(transform.position, Cannon.forward, bulletRicochetMax) != 0)
        {
            Shoot(BulletSpawn);
        }
    }

    bool ThinAngleSearch()
    {
        // First check near last rotation angle with 2 casts
        Vector3 slightLeft = Quaternion.Euler(0, Cannon.rotation.eulerAngles.y + 70, 0) * transform.position;
        Vector3 slightRight = Quaternion.Euler(0, Cannon.rotation.eulerAngles.y + 50, 0) * transform.position;
        int temp = SimulateBouncingRay(transform.position, Cannon.forward, bulletRicochetMax);
        int slres = SimulateBouncingRay(transform.position, slightLeft, bulletRicochetMax);
        int srres = SimulateBouncingRay(transform.position, slightRight, bulletRicochetMax);

        if (temp == 1)
        {
            AimAngle = Cannon.rotation.eulerAngles.y;
            return true;
        }
        if (slres == 1)
        {
            AimAngle = Cannon.rotation.eulerAngles.y - 10;
            return true;
        }
        if (srres == 1)
        {
            AimAngle = Cannon.rotation.eulerAngles.y + 10;
            return true;
        }
        if (temp == 2)
        {
            AimAngle = Cannon.rotation.eulerAngles.y;
            return true;
        }
        if (slres == 2)
        {
            AimAngle = Cannon.rotation.eulerAngles.y - 10;
            return true;
        }
        if (srres == 2)
        {
            AimAngle = Cannon.rotation.eulerAngles.y + 10;
            return true;
        }
        return false;
    }




    // If the ai doesnt have an angle to the player, fan left and right until it finds one
    void FanSearch()
    {
        if (CurrentFanAngle == 45)
        {
            // The aim angle is set to be 45 degrees to the left of the rotation towards the player
            AimAngle = Quaternion.LookRotation(Player.transform.position - transform.position).eulerAngles.y - 45;
            // If the aim angle is further than 45 degrees from the player, set the currentFanAngle to -45
            if (Vector3.Angle(Cannon.forward, Quaternion.Euler(0, AimAngle, 0) * transform.forward) < 10f)
            {
                CurrentFanAngle = -45;
            }
        }
        if (CurrentFanAngle == -45)
        {
            // The aim angle is set to be 45 degrees to the left of the rotation towards the player
            AimAngle = Quaternion.LookRotation(Player.transform.position - transform.position).eulerAngles.y + 45;
            //if the cannon is within 10 degrees of the angle, shoot
            if (Vector3.Angle(Cannon.forward, Quaternion.Euler(0, AimAngle, 0) * transform.forward) < 10f)
            {
                CurrentFanAngle = 45;
            }
        }
    }
    // Function to simulate the bouncing ray
    int SimulateBouncingRay(Vector3 origin, Vector3 direction, int remainingBounces)
    {
        // Perform a raycast
        RaycastHit hit;
        if (Physics.Raycast(origin, direction, out hit))
        {
            // Check if the ray hit the player
            if (hit.collider.CompareTag("AI"))
                return 0;
            if (hit.collider.CompareTag("Player"))
            {
                // Perform actions when the player is hit by the bouncing ray
                Debug.DrawRay(origin, direction * 50, Color.red);
                return 1;
            }
            Debug.DrawRay(origin, direction * 50, Color.blue);
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

    private bool HasLineOfSightToPlayer()
    {
        RaycastHit hit;
        Vector3 direction = Player.transform.position - Cannon.position;

        if (Physics.Raycast(Cannon.position, direction, out hit))
        {
            // Check if the raycast hits the player
            if (hit.collider.tag == "Player")
            {
                return true; // Line of sight is clear, player is hit directly
            }

            // Check if the raycast hits a wall
            if (hit.collider.tag == "Wall")
            {
                return false; // Line of sight is blocked by a wall
            }
        }

        return false; // Line of sight is not clear
    }
}
