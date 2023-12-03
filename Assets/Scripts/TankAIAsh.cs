using UnityEngine;
using UnityEngine.AI;

public class TankAIAsh : Tank
{
    private GameObject player;
    private NavMeshAgent agent;
    private Vector3 lastPlayerPosition;
    private float stationaryTime = 0f;
    private float movementDecisionInterval = 2f;

    // Start is called before the first frame update
    void Start()
    {
        player = GameObject.FindGameObjectWithTag("Player");
        agent = GetComponent<NavMeshAgent>();

        maxSpeed = 5f; // Slow movement
        rotSpeed = 30f; // Slow turret rotation
        bulletSpeed = 10f;
        bulletRicochetMax = 1;
        maxBullets = 1;
        fireRate = 3f; // Slow fire rate
        nextFire = 0f; // Initialize nextFire

        agent.speed = maxSpeed;
        lastPlayerPosition = player.transform.position;
    }

    // Update is called once per frame
    void Update()
    {
        AimAndShoot();
        MovementDecision();
        nextFire -= Time.deltaTime; // Decrement the nextFire timer
    }

    private void MovementDecision()
    {
        // Decide whether to move or stay stationary
        if (Time.time > stationaryTime + movementDecisionInterval)
        {
            Vector3 randomDirection = Random.insideUnitSphere * 10f;
            randomDirection += transform.position;
            NavMeshHit hit;
            NavMesh.SamplePosition(randomDirection, out hit, 10f, NavMesh.AllAreas);

            agent.SetDestination(hit.position);

            stationaryTime = Time.time;
        }
    }

    private void AimAndShoot()
    {
        if (nextFire > 0) return; // Check if the tank is ready to fire

        Vector3 directionToPlayer = player.transform.position - transform.position;
        directionToPlayer.y = 0; // Ignore vertical difference
        Quaternion lookRotation = Quaternion.LookRotation(directionToPlayer);

        // Extract current rotation angles
        Vector3 currentRotation = cannon.rotation.eulerAngles;

        // Rotate turret slowly towards player only on the Y axis
        cannon.rotation = Quaternion.Euler(currentRotation.x,
                                           Quaternion.RotateTowards(cannon.rotation, lookRotation, rotSpeed * Time.deltaTime).eulerAngles.y,
                                           currentRotation.z);

        // Check if the turret is roughly facing the player before shooting
        if (Quaternion.Angle(cannon.rotation, lookRotation) < 15f)
        {
            // Check for line of sight
            if (HasLineOfSightToPlayer())
            {
                Shoot();
            }
        }
    }


    private bool HasLineOfSightToPlayer()
    {
        RaycastHit hit;
        Vector3 direction = player.transform.position - cannon.position;

        if (Physics.Raycast(cannon.position, direction, out hit))
        {
            // Check if the raycast hits the player
            if (hit.collider.gameObject == player)
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
