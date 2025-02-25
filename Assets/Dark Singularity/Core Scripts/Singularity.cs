using UnityEngine;

[RequireComponent(typeof(SphereCollider))]

public class Singularity : MonoBehaviour
{
    // This is the main script which pulls the objects nearby using a gravitational force.
    
    // The strength of the gravitational pull.
    [SerializeField] private float GRAVITY_PULL = 100f;

    // The radius within which the gravitational pull is effective.
    public static float m_GravityRadius = 1f;

    // Awake is called when the script instance is being loaded.
    void Awake() {
        // Set the gravity radius to the radius of the SphereCollider attached to this GameObject.
        m_GravityRadius = GetComponent<SphereCollider>().radius;

        // Ensure the SphereCollider is set as a trigger.
        if(GetComponent<SphereCollider>()){
            GetComponent<SphereCollider>().isTrigger = true;
        }
    }
    
    // OnTriggerStay is called once per frame for every Collider other that is touching the trigger.
    void OnTriggerStay (Collider other) {
        // Check if the other object has a Rigidbody and is affected by the Singularity.
        if(other.attachedRigidbody && other.GetComponent<SingularityPullable>()) {
            // Calculate the intensity of the gravitational pull based on the distance from the Singularity.
            float gravityIntensity = Vector3.Distance(transform.position, other.transform.position) / m_GravityRadius;
            
            // Apply a force to the other object to pull it towards the Singularity.
            other.attachedRigidbody.AddForce((transform.position - other.transform.position) * gravityIntensity * other.attachedRigidbody.mass * GRAVITY_PULL * Time.smoothDeltaTime);
        }
    }
}
