using UnityEngine;
using UnityEngine.AI;

public class TankAIBrown : Tank
{
    private GameObject player;
    private Quaternion currentCannonRot;

    private Transform cannon;
    private Transform bulletSpawn;

    private Quaternion aimAngle;

    // Start is called before the first frame update
    void Start()
    {
        player = GameObject.FindGameObjectWithTag("Player");
        cannon = transform.Find("cannon");
        bulletSpawn = cannon.Find("bulletSpawn");

        currentCannonRot = cannon.rotation;
        aimAngle = currentCannonRot;
    }

    // Update is called once per frame
    void Update()
    {
        AimAndShoot();
    }

    private void AimAndShoot()
    {
        // rotate randomly when reach
        if (Quaternion.Angle(aimAngle, cannon.rotation) < 10f)
        {
            aimAngle = Quaternion.Euler(0f, Random.Range(0f, 360f), -90f);
        }

        // slowly rotate the turret towards the player
        cannon.rotation = Quaternion.RotateTowards(cannon.rotation, aimAngle, rotSpeed * Time.deltaTime);

        // Check for line of sight
        if (Quaternion.Angle(aimAngle, currentCannonRot) < 10f)
        {
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
