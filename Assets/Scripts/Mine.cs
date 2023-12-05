using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Mine : MonoBehaviour
{
    public float speed;
    public float timer = 10.0f;

    public GameObject explosionParticlePrefab;

    // store the parent tank
    public GameObject parentTank;

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
        // check in a sphere around the mine for tanks
        Collider[] hitColliders = Physics.OverlapSphere(transform.position, 10.0f);
        foreach (Collider c in hitColliders)
        {
            if (c.gameObject.tag == "AI" || c.gameObject.tag == "Player")
            {
                // Instantiate explosion at the point of collision
                if (explosionParticlePrefab != null)
                {
                    Instantiate(explosionParticlePrefab, transform.position, Quaternion.identity);
                }
                // destroy the tank
                c.gameObject.GetComponent<Tank>().DestroyTank();
            }
            if (c.gameObject.tag == "Bullet" || c.gameObject.tag == "WallBreakable")
            {
                // Instantiate explosion at the point of collision
                if (explosionParticlePrefab != null)
                {
                    Instantiate(explosionParticlePrefab, transform.position, Quaternion.identity);
                }
                // destroy the bullet
                Destroy(c.gameObject);
            }
        }
    }
}
