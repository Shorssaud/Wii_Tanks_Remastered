using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BulletBase : MonoBehaviour
{
    public float speed;
    private int ricochetCount = 0;
    public int ricochetMax = 1;
    private Vector3 vel;

    // store the parent tank
    public GameObject parentTank;

    // Start is called before the first frame update
    void Start()
    {
        vel = transform.forward;
        //ignore collisions with parentTank
        Physics.IgnoreCollision(GetComponent<Collider>(), parentTank.GetComponent<Collider>());
    }

    // Update is called once per frame
    void Update()
    {
        // Move the bullet
        transform.position += vel * speed * Time.deltaTime;
    }


    void OnCollisionEnter(Collision collision)
    {
        // if it collides with a tank, destroy the bullet and the tank unless it is the tank that fired the bullet
        if ((collision.gameObject.tag == "AI" || collision.gameObject.tag == "Player") && collision.gameObject.tag != parentTank.tag)
        {
            Destroy(gameObject);
            Destroy(collision.gameObject);
        }
        // if the bullet collides with a bullet, destroy both bullets
        if (collision.gameObject.tag == "Bullet")
        {
            Destroy(gameObject);
            Destroy(collision.gameObject);
        }
        // If the bullet collides with a wall, destroy the bullet or ricochet
        if (collision.gameObject.tag == "Wall")
        {
            if (ricochetCount < ricochetMax)
            {
                print("Bullet collided with " + collision.gameObject.tag);
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
                Destroy(gameObject);
            }
        }
    }

}
