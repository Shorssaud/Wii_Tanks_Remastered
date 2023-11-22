using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TankAIBrown : Tank
{
    // Start is called before the first frame update
    void Start()
    {
        maxSpeed = 0f;
        rotSpeed = 90f;
        bulletSpeed = 10f;
        bulletRicochetMax = 2;
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
