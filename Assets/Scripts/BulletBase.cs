using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BulletBase : MonoBehaviour
{
    public float speed;
    public int ricochetCount = 0;
    public int ricochetMax = 1;
    private Vector3 vel;

    public GameObject explosionParticlePrefab;
    public GameObject ricochetParticlePrefab;

    // store the parent tank
    public GameObject parentTank;

    // Start is called before the first frame update
    void Start()
    {
        vel = transform.forward;
        //ignore collisions with parentTank (Disable Friendly Fire)
        //Physics.IgnoreCollision(GetComponent<Collider>(), parentTank.GetComponent<Collider>());
    }

    // Update is called once per frame
    void Update()
    {
        // Move the bullet
        transform.position += vel * speed * Time.deltaTime;
    }


    void OnCollisionEnter(Collision collision)
    {
        print("Bullet collided with " + collision.gameObject.name);
        // if it collides with a tank, destroy the bullet and the tank unless it is the tank that fired the bullet
        if (collision.gameObject.tag == "AI" || collision.gameObject.tag == "Player")
        {
            Destroy(gameObject);
            float explosionSize = 3.0f;
            collision.gameObject.GetComponent<Tank>().DestroyTank(explosionSize);
            parentTank.GetComponent<Tank>().RemoveBullet();
        }

        // if the bullet collides with a bullet, destroy both bullets
        if (collision.gameObject.tag == "Bullet")
        {
            // Instantiate explosion at the point of collision
            if (explosionParticlePrefab != null)
            {
                Instantiate(explosionParticlePrefab, transform.position, Quaternion.identity);
            }

            Destroy(gameObject);
            Destroy(collision.gameObject);
            parentTank.GetComponent<Tank>().RemoveBullet();
        }
        // If the bullet collides with a wall, destroy the bullet or ricochet
        if (collision.gameObject.tag == "Wall")
        {
            if (ricochetCount < ricochetMax)
            {
                if (ricochetParticlePrefab != null)
                {
                    Instantiate(ricochetParticlePrefab, transform.position, Quaternion.identity);
                }
                // Calculate the reflection direction using Unity's physics engine
                Vector3 reflection = Vector3.Reflect(vel.normalized, collision.contacts[0].normal);

                // Apply the reflection to the bullet's velocity
                vel = reflection;

                // Increment the ricochet count
                ricochetCount++;

                // rotate the bullet to face the direction it is moving
                transform.rotation = Quaternion.LookRotation(vel);
            }
            else
            {
                // Instantiate explosion at the point of collision
                if (explosionParticlePrefab != null)
                {
                    Instantiate(explosionParticlePrefab, transform.position, Quaternion.identity);
                }
                Destroy(gameObject);
                parentTank.GetComponent<Tank>().RemoveBullet();
            }
        }
    }

}
