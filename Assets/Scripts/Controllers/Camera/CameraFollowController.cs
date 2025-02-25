using UnityEngine;

public class CameraFollowController : MonoBehaviour
{
    [Header("Follow Settings")]
    [SerializeField] private Transform target; // The spaceship to follow.
    [SerializeField] private Vector3 offset = new Vector3(0, 10, -20); // Offset from the spaceship.
    [SerializeField] private float smoothSpeed = 0.125f; // Speed of the smooth interpolation.

    private Vector3 targetPosition; // The target position for the camera.

    private void Awake()
    {
        if (target == null)
        {
            Debug.LogError("Target is null in CameraFollow");
        }
    }

    private void FixedUpdate()
    {
        // The multiplication (target.rotation * offset) rotates the offset vector based on the current orientation of the target.
        // The result is a vector indicating a certain direction: the direction in which the spaceship is rotated.
        // Adding this result to the position vector of the spaceship gives the final position the camera should assume.
        targetPosition = target.position + target.rotation * offset;

        // Smooth interpolation of the position and rotation.
        transform.SetPositionAndRotation(Vector3.Lerp(transform.position, targetPosition, smoothSpeed), Quaternion.Lerp(transform.rotation, target.rotation, smoothSpeed));
    }
}