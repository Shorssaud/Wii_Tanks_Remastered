using UnityEngine;
using UnityEngine.AI;


// This class can be used as a reference for other AI tanks
public class TankAIAsh : Tank
{
    private GameObject player;
    private NavMeshAgent agent;
    private Vector3 lastPlayerPosition;
    private float stationaryTime = 0f;
    private float movementDecisionInterval = 2f;
    private Quaternion currentCannonRot;
    private Vector3 currentMoveTarget;

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
        lastPlayerPosition = player.transform.position;
    }

    // Update is called once per frame
    void Update()
    {
        AimAndShoot();
        MovementDecision();
        stationaryTime += Time.deltaTime;
        nextFire -= Time.deltaTime; // Decrement the nextFire timer 
    }

    private void MovementDecision()
    {
        // Decide whether to move or stay stationary
        if (stationaryTime > movementDecisionInterval)
        {
            // This is the code for the navmesh to give a movement command
            Vector3 randomDirection = Random.insideUnitSphere * 10f;
            randomDirection += transform.position;
            NavMeshHit hit;
            NavMesh.SamplePosition(randomDirection, out hit, 10f, NavMesh.AllAreas);
            currentMoveTarget = new Vector3(hit.position.x, 0, hit.position.z);
            stationaryTime = 0f;
        }
        //IMPORTANT: DO NOT MODIFY THE CODE BELOW THIS LINE, IT IS USED FOR THE TANK TO MOVE AND WILL MESS U UP IF YOU CHANGE IT
        // get the direction in vertical and horizontal input value
        float horizontal = (currentMoveTarget.x - transform.position.x) / 10f;
        float vertical = (currentMoveTarget.z - transform.position.z) / 10f;
        // round this to 1, 0, or -1
        horizontal = Mathf.Round(horizontal);
        vertical = Mathf.Round(vertical);

        Move(horizontal, vertical);
    }

    // This function can be changed to make the AI tank aim and shoot differently
    private void AimAndShoot()
    {
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
            if (HasLineOfSightToPlayer() && nextFire < 0)
            {
                Shoot(bulletSpawn);
            }
        }
    }


    private bool HasLineOfSightToPlayer()
    {
        RaycastHit hit;
        Vector3 direction = player.transform.position - cannon.position;
        // show the raycast in the editor
        Debug.DrawRay(cannon.position, direction, Color.red, 1f);

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
