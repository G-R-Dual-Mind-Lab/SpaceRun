using UnityEngine;

public class CameraShakingController : MonoBehaviour
{
    [Header("Event Channels")]
    [SerializeField] private BooleanEventChannelSO isShakingEventChannelSO;

    [Header("Shaking Properties")]
    [SerializeField] private float magnitude = 0.5f; // Intensity of the shake.

    // Dirty Flag
    public bool shake = false;

    void Awake()
    {
        if (isShakingEventChannelSO == null)
        {
            Debug.LogError("isShakingEventChannelSO is null in CameraShakingController Awake");
        }
    }

    private void OnEnable()
    {
        // Subscribe to the event when the object is enabled.
        isShakingEventChannelSO.OnEventRaised += HandleShipStatusChanged;
    }

    private void OnDisable()
    {
        // Unsubscribe from the event when the object is disabled.
        isShakingEventChannelSO.OnEventRaised -= HandleShipStatusChanged;
    }

    void Update()
    {
        if (shake)
        {
            // Generate random shake offsets and apply them to the camera's position.
            transform.position = transform.position + new Vector3((Random.Range(-1f, 1f) * magnitude), (Random.Range(-1f, 1f) * magnitude), 0); // The camera shakes.
        }
    }

    // Handle the event to start or stop shaking.
    private void HandleShipStatusChanged(bool isShaking)
    {
        shake = isShaking;
    }
}