using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TankPlayer : Tank
{
    
    // Start is called before the first frame update
    void Start()
    {
        maxSpeed = 10f;
        rotSpeed = 270f;
        bulletSpeed = 10f;
        bulletRicochetMax = 2;
    }

    // Update is called once per frame
    void Update()
    {
        // Get horizontal and vertical input values
        float horizontal = Input.GetAxis("Horizontal");
        float vertical = Input.GetAxis("Vertical");
        if (Input.GetKeyDown(KeyCode.Space))
        {
            Shoot();
        }
        
        Move(horizontal, vertical);
        TankUpdate();
    }
}
