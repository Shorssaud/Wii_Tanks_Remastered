using UnityEngine;
using UnityEngine.AI;


// This class can be used as a reference for other AI tanks
public class TankAIYellow : Tank
{
    private GameObject player;
    private NavMeshAgent agent;
    private float movementDecisionInterval = 2f;
    private Quaternion currentCannonRot;

    private Transform cannon;
    private Transform bulletSpawn;

    private float avoidanceDistance = 40f;
    private LayerMask mineLayer;
    private LayerMask aiLayer;
    private float edgeBiasDistance = 15f;

    // Start is called before the first frame update
    void Start()
    {
        player = GameObject.FindGameObjectWithTag("Player");
        agent = GetComponent<NavMeshAgent>();
        cannon = transform.Find("cannon");
        bulletSpawn = cannon.Find("bulletSpawn");

        currentCannonRot = cannon.rotation;

        agent.speed = maxSpeed;
        agent.acceleration = 10;
        InvokeRepeating("MovementDecision", 0f, movementDecisionInterval);
        mineLayer = LayerMask.GetMask("Mine");
        aiLayer = LayerMask.GetMask("AI");

    }

    // Update is called once per frame
    void Update()
    {
        AimAndShoot();
    }

    private void MovementDecision()
    {
        NavMeshHit hit;
        Vector3 randomDirection;

        if (NavMesh.SamplePosition(transform.position, out hit, avoidanceDistance, NavMesh.AllAreas))
        {
            float distanceToEdge = Vector3.Distance(transform.position, hit.position);

            if (distanceToEdge > edgeBiasDistance)
            {
                // Apply bias towards the center when near the edge
                randomDirection = (hit.position - transform.position).normalized * (avoidanceDistance - distanceToEdge);
            }
            else
            {
                // Normal random direction calculation
                randomDirection = Random.insideUnitSphere * avoidanceDistance;
            }

            // Sample position within the NavMesh
            randomDirection += transform.position;

            NavMesh.SamplePosition(randomDirection, out hit, avoidanceDistance, NavMesh.AllAreas);
        }
    }

    // This function can be changed to make the AI tank aim and shoot differently
    private void AimAndShoot()
    {
        // check if there are any ai tanks nearby by using a collision sphere
        Collider[] colliders = Physics.OverlapSphere(transform.position, 10f);
        foreach (Collider collider in colliders)
        {
            if (collider.tag == "AI" && collider.gameObject != gameObject)
            {
                return;
            }
        }
        PlaceMine();
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
