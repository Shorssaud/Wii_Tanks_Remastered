using UnityEngine;
using UnityEngine.AI;


// This class can be used as a reference for other AI tanks
public class TankAIMarine: Tank
{
    private GameObject player;
    private NavMeshAgent agent;
    private float stationaryTime = 0f;
    private float movementDecisionInterval = 2f;
    private Quaternion currentCannonRot;

    private Transform cannon;
    private Transform bulletSpawn;

    // Start is called before the first frame update
    void Start()
    {
        player = GameObject.FindGameObjectWithTag("Player");
        agent = GetComponent<NavMeshAgent>();
        cannon = transform.Find("cannon");
        bulletSpawn = cannon.Find("bulletSpawn");

        currentCannonRot = cannon.rotation;

        agent.speed = maxSpeed;
    }

    // Update is called once per frame
    void Update()
    {
        AimAndShoot();
        MovementDecision();
        stationaryTime += Time.deltaTime;
    }

    private void MovementDecision()
    {
        // Decide whether to move or stay stationary
        if (stationaryTime > movementDecisionInterval)
        {
            int minDistance = 40;
            float distanceToPlayer = Vector3.Distance(transform.position, player.transform.position);
            print(distanceToPlayer);
            if (distanceToPlayer < minDistance)
            {
                // Calculate a direction away from the player
                Vector3 dirAwayFromPlayer = transform.position - player.transform.position;
                Vector3 newPosition = transform.position + dirAwayFromPlayer.normalized * minDistance;

                // Move the tank to the new position using NavMeshAgent
                agent.SetDestination(newPosition);
            }
            else
            {
                if (agent.remainingDistance < 1f)
                {
                    // If the tank is already at the destination, set a new random destination
                    Vector3 randomDestination = Random.insideUnitSphere * 100;
                    agent.SetDestination(randomDestination);
                }
            }
        }
    }

    // This function can be changed to make the AI tank aim and shoot differently
    private void AimAndShoot()
    {
        // Before doing anything, check if the player is still active
        if (player == null)
        {
            return; // If not, don't aim or shoot
        }
        cannon.rotation = currentCannonRot; //Always keep the cannon facing the desired direction
        Vector3 directionToPlayer = player.transform.position - transform.position; // Get direction to player
        directionToPlayer.y = 0; // Ignore vertical difference
        Quaternion lookRotation = Quaternion.LookRotation(directionToPlayer); // Create a quaternion (rotation) based on looking down the vector from the ai to the player
        Vector3 currentRotation = cannon.rotation.eulerAngles; // Extract current rotation angles

        // Rotate turret slowly towards player only on the Y axis
        cannon.rotation = Quaternion.Euler(currentRotation.x,
                                           Quaternion.RotateTowards(cannon.rotation, lookRotation, rotSpeed * Time.deltaTime).eulerAngles.y,
                                           currentRotation.z);
        currentCannonRot = cannon.rotation;

        // Check if the tank y rotation is roughly facing the player before shooting
        if (Vector3.Angle(cannon.forward, directionToPlayer) < 5f)
        {
            // Check for line of sight
            if (HasLineOfSightToPlayer())
            {
                Shoot(bulletSpawn);
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
