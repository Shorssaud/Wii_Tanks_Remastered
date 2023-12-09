using UnityEngine;
using UnityEngine.AI;
using static UnityEditor.Searcher.SearcherWindow.Alignment;


// This class can be used as a reference for other AI tanks
public class TankAIYellow : Tank
{
    private GameObject player;
    private NavMeshAgent agent;
    private float movementDecisionInterval = 2f;
    private Quaternion currentCannonRot;

    private Transform cannon;
    private Transform bulletSpawn;
    private Vector3 currentDest;
    float horizontal = 0;
    float vertical = 0;


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
        horizontal = agent.desiredVelocity.x / maxSpeed;
        vertical = agent.desiredVelocity.z / maxSpeed;
        Move(horizontal, vertical);
        AimAndShoot();
    }

    private void MovementDecision()
    {
        int i = 0;
        float minDistance = 30f;

        Vector3 randomDirection = Random.insideUnitSphere * 100;
        randomDirection += player.transform.position; // Add player's position to random direction
        NavMeshHit hit;

        // Continuously try sampling positions until a valid position with minimum distance is found
        while (i < 5)
        {
            i++;
            NavMesh.SamplePosition(randomDirection, out hit, 100, NavMesh.AllAreas);
            Vector3 sampledPosition = new Vector3(hit.position.x, 0, hit.position.z);

            // Check the distance between sampled position and player
            if (Vector3.Distance(sampledPosition, player.transform.position) >= minDistance)
            {
                agent.SetDestination(sampledPosition);
                break; // Exit the loop when a valid position is found
            }
            else
            {
                // Recalculate a new random direction for sampling
                randomDirection = Random.insideUnitSphere * 100;
                randomDirection += player.transform.position;
            }
        }
    }

    // This function can be changed to make the AI tank aim and shoot differently
    private void AimAndShoot()
    {
        // check if there are any ai tanks nearby by using a collision sphere
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

}
