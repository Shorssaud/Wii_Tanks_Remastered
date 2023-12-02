using UnityEngine;
using UnityEngine.AI;

public class TankAIBrown : Tank
{
    private GameObject player;
    private NavMeshAgent agent;

    // Start is called before the first frame update
    void Start()
    {
        player = GameObject.FindGameObjectWithTag("Player");
        agent = GetComponent<NavMeshAgent>();

        maxSpeed = 0f; // Stationary
        rotSpeed = 20f; // Slow turret rotation
        bulletSpeed = 10f;
        bulletRicochetMax = 1;
        maxBullets = 1;
        fireRate = 2f; // Slow fire rate
        nextFire = 0f; // Initialize nextFire
    }

    // Update is called once per frame
    void Update()
    {
        AimAndShoot();
        nextFire -= Time.deltaTime; // Decrement the nextFire timer
    }

    private void AimAndShoot()
    {
        if (nextFire > 0) return; // Check if the tank is ready to fire

        Vector3 directionToPlayer = player.transform.position - transform.position;
        Quaternion lookRotation = Quaternion.LookRotation(directionToPlayer);

        // Rotate turret slowly towards player
        cannon.rotation = Quaternion.RotateTowards(cannon.rotation, lookRotation, rotSpeed * Time.deltaTime);

        // Check if the turret is roughly facing the player before shooting
        if (Quaternion.Angle(cannon.rotation, lookRotation) < 10f)
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
