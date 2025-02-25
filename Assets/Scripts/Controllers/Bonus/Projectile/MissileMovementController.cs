using UnityEngine;
using Helper;

public class MissileMovementController : MonoBehaviour
{
    [Range(0, 1000)][SerializeField] protected float missileSpeed; // Speed of the projectile.
    [Range(5, 10)][SerializeField] protected float missileDistance; // Distance from the spaceship where the projectile will be spawned.
    private Transform spaceshipTransform; // Transform of the player's spaceship.
    private Rigidbody r; // Rigidbody of the projectile.

    void Awake()
    {
        r = GetComponent<Rigidbody>(); // Get the Rigidbody component of the projectile.

        if (r == null)
        {
            Debug.LogError("Missile rigid body not found.");
            return;
        }

        // Find the player in the scene using the player tag.
        GameObject player = GameObject.FindGameObjectWithTag(TagsHelper.Player);
        // Get the Transform component of the player.
        spaceshipTransform = player.transform;
    }

    private void OnEnable()
    {
        // Set the position and rotation of the projectile.
        r.transform.SetPositionAndRotation(spaceshipTransform.position + spaceshipTransform.forward * missileDistance, spaceshipTransform.rotation * Quaternion.Euler(90, 0, 0));

        // Set the velocity of the projectile's Rigidbody.
        r.linearVelocity = spaceshipTransform.forward * missileSpeed;
    }
}