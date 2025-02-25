using UnityEngine;

[RequireComponent(typeof(SphereCollider))]

public class SingularityCore : MonoBehaviour
{
    // Tag used to identify satellite objects.
    private const string Satellite = "Satellite";

    // Reference to the RevisedSpaceObject component.
    private RevisedSpaceObject spaceObject;

    // Awake is called when the script instance is being loaded.
    void Awake()
    {
        // Ensure the SphereCollider is set as a trigger.
        if (GetComponent<SphereCollider>())
        {
            GetComponent<SphereCollider>().isTrigger = true;
        }
    }

    // This script is responsible for what happens when the pullable objects reach the core.
    // The object is then released back into the pool as this is much more performant than destroying the objects.
    void OnTriggerEnter(Collider other) {

        // Check if the other object is pullable by the Singularity.
        if(other.GetComponent<SingularityPullable>())
        {
            // Get the RevisedSpaceObject component from the other object.
            spaceObject = other.gameObject.GetComponent<RevisedSpaceObject>();

            // Release the object back into the pool.
            spaceObject.ObjectPool.Release(spaceObject);
        }
        // Check if the other object is a satellite. These objects are modular and have child objects representing smaller parts.
        // These child objects also have the "Satellite" tag because they should still cause the player's destruction upon contact.
        // However, these children do not have the SingularityPullable script, only the parent does, so we need to retrieve it from the parent.
        else if (other.gameObject.CompareTag(Satellite))
        {
            // Get the RevisedSpaceObject component from the parent of the satellite.
            spaceObject = other.gameObject.transform.parent.GetComponent<RevisedSpaceObject>();

            if (spaceObject != null)
            {
                // Ensure the object is active in the hierarchy before releasing it.
                if (spaceObject.gameObject.activeInHierarchy)
                {
                    // Release the object back into the pool.
                    spaceObject.ObjectPool.Release(spaceObject);
                }
            }
        }
    }
}
