using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BulletBase : MonoBehaviour
{
    public float speed;
    public int ricochetCount = 0;
    public int ricochetMax = 1;
    private float ricochetTimerMax = 0.2f;
    private float ricochetTimer = 0.0f;
    private Vector3 vel;

    public GameObject explosionParticlePrefab;
    public GameObject ricochetParticlePrefab;

    // store the parent tank
    public GameObject parentTank;

    // Start is called before the first frame update
    void Start()
    {
        vel = transform.forward;
        GetComponent<Rigidbody>().velocity = Vector3.zero;
        //ignore collisions with parentTank (Disable Friendly Fire)
        //Physics.IgnoreCollision(GetComponent<Collider>(), parentTank.GetComponent<Collider>());
    }

    // Update is called once per frame
    void Update()
    {
        // Move the bullet
        transform.position += vel * speed * Time.deltaTime;
        ricochetTimer -= Time.deltaTime;
    }

    void OnCollisionEnter(Collision collision)
    {
        // if it collides with a tank, destroy the bullet and the tank unless it is the tank that fired the bullet
        if (collision.gameObject.tag == "AI" || collision.gameObject.tag == "Player")
        {
            Destroy(gameObject);
            float explosionSize = 3.0f;
            if (collision.gameObject.GetComponent<TankAIMachineLearning>() != null)
            {
                collision.gameObject.GetComponent<TankAIMachineLearning>().DestroyTank(explosionSize);
            } else
            {
                collision.gameObject.GetComponent<Tank>().DestroyTank(explosionSize);
            }
            if (collision.gameObject.tag == "AI")
            {
                //FindObjectOfType<AudioManager>().Play("Explosion");
                int currentScore = PlayerPrefs.GetInt("TotalScore");
                PlayerPrefs.SetInt("TotalScore", currentScore + 1);
                PlayerPrefs.Save();
            }
        }

        // if the bullet collides with a bullet, destroy both bullets
        if (collision.gameObject.tag == "Bullet")
        {
            if (explosionParticlePrefab != null) Instantiate(explosionParticlePrefab, transform.position, Quaternion.identity);
            Destroy(gameObject);
            Destroy(collision.gameObject);
        }
        // If the bullet collides with a wall, destroy the bullet or ricochet
        if (collision.gameObject.tag == "Wall" || collision.gameObject.tag == "WallBreakable")
        {
            if (ricochetCount < ricochetMax && ricochetTimer < 0)
            {
                //FindObjectOfType<AudioManager>().Play("Ricochet");
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
                ricochetTimer = ricochetTimerMax;
            }
            else
            {
                if (explosionParticlePrefab != null) Instantiate(explosionParticlePrefab, transform.position, Quaternion.identity);
                Destroy(gameObject);
            }
        }
    }
    private void OnDestroy()
    {
        if (parentTank != null && parentTank.GetComponent<TankAIMachineLearning>() != null)
        {
            parentTank.GetComponent<TankAIMachineLearning>().RemoveBullet(this.transform.localPosition);
            return;
        }
        if (parentTank != null) parentTank.GetComponent<Tank>().RemoveBullet();
    }

}
