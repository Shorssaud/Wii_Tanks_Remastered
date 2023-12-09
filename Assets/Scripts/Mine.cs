using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Exploder.Utils;
using UnityEngine.AI;

public class Mine : MonoBehaviour
{
    public float timer = 10.0f;

    public GameObject explosionParticlePrefab;

    // store the parent tank
    public GameObject parentTank;

    public float explosionScale = 1.0f; // Default scale is 1.0

    bool isUnderAi = true;

    NavMeshObstacle obstacle;

    // Start is called before the first frame update
    void Start()
    {
        obstacle = GetComponent<NavMeshObstacle>();
        obstacle.enabled = false;
    }

    // Update is called once per frame
    void Update()
    {
        if (isUnderAi && parentTank != null)
        {
            //check if the parent tank is still colliding with the mine
            if (!parentTank.GetComponent<Collider>().bounds.Contains(transform.position))
            {
                //if it isn't, stop being under AI control
                isUnderAi = false;
            }
        }
        else
        {
            obstacle.enabled = true;
        }
        timer -= Time.deltaTime;
        if (timer <= 0.0f)
        {
            BlowUp(false);
            Destroy(gameObject);
        }
    }


    void OnTriggerEnter(Collider collision)
    {
        // if the creater of the mine collides with it, ignore the collision
        if (parentTank != null && collision.gameObject == parentTank)
        {
            return;
        }
        if (collision.gameObject.tag == "Player" || collision.gameObject.tag == "AI" || collision.gameObject.tag == "Bullet")
        {
            BlowUp(false);
            Destroy(gameObject);
        }
    }

    public void BlowUp(bool isMined)
    {
        // Instantiate explosion at the point of collision
        if (explosionParticlePrefab != null)
        {
            GameObject explosion = Instantiate(explosionParticlePrefab, transform.position, Quaternion.identity);
            explosion.transform.localScale *= explosionScale; // Scale the explosion effect
        }
        // Check in a sphere around the mine for tanks, bullets, and breakable walls
        Collider[] hitColliders = Physics.OverlapSphere(transform.position, 10.0f);
        foreach (Collider c in hitColliders)
        {
            // Handle destruction of tanks, bullets, and breakable walls
            if (c.gameObject.tag == "AI" || c.gameObject.tag == "Player")
            {
                c.gameObject.GetComponent<Tank>().DestroyTank(explosionScale); // Pass explosionScale as parameter
            }
            if (!isMined && c.gameObject.tag == "Mine")
            {
                c.gameObject.GetComponent<Mine>().BlowUp(true);
                Destroy(c.gameObject);
            }
            else if (c.gameObject.tag == "Bullet")
            {
                Destroy(c.gameObject);
            }
            else if (c.gameObject.tag == "WallBreakable")
            {
                Destroy(c.gameObject);
            }
        }
        if (parentTank != null)
        {
            parentTank.GetComponent<Tank>().RemoveMine();
        }
    }
}
