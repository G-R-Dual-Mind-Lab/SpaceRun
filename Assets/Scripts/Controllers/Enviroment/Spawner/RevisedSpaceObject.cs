using UnityEngine.Pool;
using UnityEngine;

public class RevisedSpaceObject : MonoBehaviour
{
    // Initial impulse force applied to the object to push it downwards.
    [SerializeField] private float force = 200f;

    // Pool for the space object.
    private IObjectPool<RevisedSpaceObject> objectPool;

    // Game Object Component
    private Rigidbody rbSpaceObject;

    // Other Variables
    private Quaternion randomRotation;
    private float rotationVelocity;

    // Pool Property
    public IObjectPool<RevisedSpaceObject> ObjectPool
    {
        get => objectPool;
        set => objectPool = value;
    }

    private void Awake()
    {
        // Get the Rigidbody component of the space object.
        rbSpaceObject = gameObject.GetComponent<Rigidbody>();

        if (rbSpaceObject == null)
        {
            Debug.LogError("rbSpaceObject is null in RevisedSpaceObject");
        }
    }

    // Apply an initial impulse to the object.
    public void AddImpulse()
    {
        // Apply a downward force (along the y-axis).
        rbSpaceObject.AddForce(Vector3.down * force, ForceMode.Impulse);

        // Apply an angular velocity along a random rotation axis to make the object rotate.
        rbSpaceObject.angularVelocity = Random.onUnitSphere.normalized * rotationVelocity;
    }

    // Initialize the object's rotation with a random rotation and set its rotation speed.
    public void InitializeObjectRotation(float rotationSpeed)
    {
        // Calculate a random rotation.
        randomRotation = Quaternion.Euler(
            Random.Range(0f, 360f),
            Random.Range(0f, 360f),
            Random.Range(0f, 360f)
        );

        // Apply the random rotation to the object.
        gameObject.transform.rotation = randomRotation;

        // Set the rotation velocity.
        rotationVelocity = rotationSpeed;
    }
}