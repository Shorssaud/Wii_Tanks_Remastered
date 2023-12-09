using UnityEngine;

public class DestroyParticleAfterComplete : MonoBehaviour
{
    private ParticleSystem particleSystem;

    void Start()
    {
        // Find the ParticleSystem component in the children of this GameObject
        particleSystem = GetComponentInChildren<ParticleSystem>();
    }

    void Update()
    {
        // Check if the particle system has finished playing
        if (particleSystem && !particleSystem.IsAlive())
        {
            Destroy(gameObject); // Destroy the parent GameObject
        }
    }
}
