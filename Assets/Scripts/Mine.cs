using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Exploder.Utils;

public class Mine : MonoBehaviour
{
    public float speed;
    public float timer = 10.0f;

    public GameObject explosionParticlePrefab;

    // store the parent tank
    public GameObject parentTank;

    public float explosionScale = 1.0f; // Default scale is 1.0

    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        timer -= Time.deltaTime;
        if (timer <= 0.0f)
        {
            Destroy(gameObject);
        }
    }


    void OnCollisionEnter(Collision collision)
    {
        // if the creater of the mine collides with it, ignore the collision
        if (collision.gameObject == parentTank)
        {
            return;
        }
        Destroy(gameObject);
    }

private void OnDestroy()
{
    // Check in a sphere around the mine for tanks, bullets, and breakable walls
    Collider[] hitColliders = Physics.OverlapSphere(transform.position, 10.0f);
    foreach (Collider c in hitColliders)
    {
        // Instantiate explosion at the point of collision
        if (explosionParticlePrefab != null)
        {
            GameObject explosion = Instantiate(explosionParticlePrefab, transform.position, Quaternion.identity);
            explosion.transform.localScale *= explosionScale; // Scale the explosion effect
        }

        // Handle destruction of tanks, bullets, and breakable walls
        if (c.gameObject.tag == "AI" || c.gameObject.tag == "Player")
        {
            c.gameObject.GetComponent<Tank>().DestroyTank(explosionScale); // Pass explosionScale as parameter
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
}


}
