using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class TankPlayer : Tank
{
    private Transform cannon;
    private Transform bulletSpawn;
    private Camera mainCamera;

    public GameObject cursor;

    private Gamepad gamepad;

    // For gizmo drawing
    private Ray lastRay;
    private bool didHit;
    private RaycastHit lastHit;

    // Start is called before the first frame update
    void Start()
    {
        gamepad = Gamepad.current;
        for (int i = 0; i < Gamepad.all.Count; i++)
        {
            Debug.Log(Gamepad.all[i].name);
        }
        mainCamera = Camera.main;
        cannon = transform.Find("cannon");
        bulletSpawn = cannon.Find("bulletSpawn");
    }

    // Update is called once per frame
    void Update()
    {
        float horizontal = Input.GetAxis("Horizontal");
        float vertical = Input.GetAxis("Vertical");
        Move(horizontal, vertical);
        CannonTracer();

        // Handle shooting and placing mines with the controller
        if (gamepad != null)
        {
            if (gamepad.rightShoulder.wasPressedThisFrame)
            {
                Shoot(bulletSpawn);
            }

            if (gamepad.leftShoulder.wasPressedThisFrame)
            {
                PlaceMine();
            }
        }

        // Handle shooting and placing mines with the mouse and keyboard
        if (Input.GetMouseButton(0))
        {
            Shoot(bulletSpawn);
        }

        if (Input.GetKeyDown(KeyCode.Space))
        {
            PlaceMine();
        }


        // Make the cannon always point at the mouse
        CannonTracer();
    }

private void CannonTracer()
{
    int floorLayerMask = 1 << LayerMask.NameToLayer("Floor");

    if (cursor != null)
    {
        Ray camRay = mainCamera.ScreenPointToRay(mainCamera.WorldToScreenPoint(cursor.transform.position));
        lastRay = camRay;

        if (Physics.Raycast(camRay, out RaycastHit floorHit, 1000f, floorLayerMask))
        {
            didHit = true;
            lastHit = floorHit;

            Vector3 playerToMouse = floorHit.point - cannon.position;
            playerToMouse.y = 10f; // Ensure the vector is entirely along the floor plane

            Quaternion newRotation = Quaternion.LookRotation(playerToMouse);
            float yRotation = newRotation.eulerAngles.y;

            // Set the cannon's rotation
            // Preserve the current X and Z rotations, only change the Y rotation
            cannon.rotation = Quaternion.Euler(cannon.rotation.eulerAngles.x, yRotation, cannon.rotation.eulerAngles.z);
        }
        else
        {
            didHit = false;
        }
    }
}


    void OnDrawGizmos()
    {
        if (didHit)
        {
            // Draw a green line from the cannon to the hit point
            Gizmos.color = Color.green;
            Gizmos.DrawLine(lastRay.origin, lastHit.point);
            // Draw a sphere at the hit point
            Gizmos.DrawSphere(lastHit.point, 0.2f);
        }
        else
        {
            // Draw a red line along the ray
            Gizmos.color = Color.red;
            Gizmos.DrawLine(lastRay.origin, lastRay.origin + lastRay.direction * 1000f);
        }
    }
    public void AddScore()
    {
        int currentScore = GetScore();
        PlayerPrefs.SetInt("TotalScore", currentScore + 1);
        PlayerPrefs.Save();
    }

    public void AddLife()
    {
        int currentLives = GetLives();
        PlayerPrefs.SetInt("Lives", currentLives + 1);
        PlayerPrefs.Save();
    }

    public void RemoveLife()
    {
        int currentLives = GetLives();
        PlayerPrefs.SetInt("Lives", currentLives - 1);
        PlayerPrefs.Save();
    }

    public int GetScore()
    {
        return PlayerPrefs.GetInt("TotalScore");
    }

    public int GetLives()
    {
        return PlayerPrefs.GetInt("Lives");
    }

    public void EndGame()
    {
        PlayerPrefs.SetInt("TotalScore", 0);
        PlayerPrefs.SetInt("Lives", 3);
    }
}
