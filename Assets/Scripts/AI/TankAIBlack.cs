using UnityEngine;
using UnityEngine.AI;


// This class can be used as a reference for other AI tanks
public class TankAIBlack: Tank
{
    private GameObject player;
    private NavMeshAgent agent;
    private float movementDecisionInterval = 0.7f;
    private Quaternion currentCannonRot;

    private Transform cannon;
    private Transform bulletSpawn;
    private Vector3 currentDest;
    float horizontal = 0;
    float vertical = 0;
    int randRad = 40;
    // Start is called before the first frame update
    void Start()
    {
        player = GameObject.FindGameObjectWithTag("Player");
        agent = GetComponent<NavMeshAgent>();
        cannon = transform.Find("cannon");
        bulletSpawn = cannon.Find("bulletSpawn");

        currentCannonRot = cannon.rotation;

        agent.speed = maxSpeed;
        InvokeRepeating("MovementDecision", 0f, movementDecisionInterval);
        agent.updatePosition = true;
        agent.updateRotation = false;
    }

    // Update is called once per frame
    void Update()
    {
        // change the desired velocity into a horizontal and vertical input
        horizontal = agent.desiredVelocity.x / maxSpeed;
        vertical = agent.desiredVelocity.z / maxSpeed;
        Move(horizontal, vertical);
        AimAndShoot();
    }

    private void MovementDecision()
    {
        // Move towards the player but stay a minimum distance away
        float minDistance = 40f;
        float distanceToPlayer = Vector3.Distance(transform.position, player.transform.position);
        if (distanceToPlayer < minDistance)
        {
            // Calculate a direction away from the player
            Vector3 dirAwayFromPlayer = transform.position - player.transform.position;
            currentDest = transform.position + dirAwayFromPlayer.normalized * minDistance;

            // Move towards the new position while avoiding mines
            agent.SetDestination(currentDest);
        }
        else
        {

            Vector3 playerPosition = player.transform.position;

            // Update the destination towards the player with some randomness
            Vector3 randomOffset = Random.insideUnitSphere * randRad;
            currentDest = playerPosition + randomOffset;
            agent.SetDestination(currentDest);
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
        if (Vector3.Angle(cannon.forward, directionToPlayer) < 10f)
        {
            // Check for line of sight
            if (HasLineOfSightToPlayer())
            {
                Shoot(bulletSpawn);
            }
        }
        Collider[] colliders = Physics.OverlapSphere(transform.position, 10f);
        foreach (Collider collider in colliders)
        {
            if ((collider.tag == "AI" && collider.gameObject != gameObject) || collider.tag == "Mine")
            {
                return;
            }
        }
        PlaceMine();
    }


    private bool HasLineOfSightToPlayer()
    {
        Vector3 direction = player.transform.position - cannon.position;
        RaycastHit hit;

        // Calculate the bullet size/radius (assuming spherical for illustration)
        float bulletRadius = bulletPrefab.GetComponent<Collider>().bounds.extents.magnitude;

        if (Physics.SphereCast(cannon.position, bulletRadius, direction, out hit))
        {
            // Check if the spherecast hits the player
            if (hit.collider.tag == "Player")
            {
                return true; // Line of sight is clear, player is hit directly
            }

            // Check if the spherecast hits a wall
            if (hit.collider.tag == "Wall")
            {
                return false; // Line of sight is blocked by a wall
            }
        }

        return false; // Line of sight is not clear
    }
}
