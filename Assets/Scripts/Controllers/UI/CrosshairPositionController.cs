using UnityEngine;

public class CrosshairPositionController : MonoBehaviour
{
    [Header("Player Publishing Channel")]
    [SerializeField] private TransformEventChannelSO playerTransformEventChannel; // Event channel for player transform updates
    
    private Transform spaceship; // Reference to the player's spaceship transform
    private Camera mainCamera; // Reference to the main camera
    public float distance = 100f; // Distance to point the crosshair
    private Vector3 targetPoint; // Target point in world space
    private Vector3 screenPoint; // Target point in screen space

    private void Awake()
    {
        if (playerTransformEventChannel == null)
        {
            Debug.LogError("playerTransformEventChannel is null in CrosshairPositionController Awake");
        }
    }
    
    private void Start()
    {
        // Find the main camera in the scene and get its Camera component
        GameObject mainCameraObject = GameObject.FindWithTag("MainCamera");
        mainCamera = mainCameraObject.GetComponent<Camera>();
    }

    private void OnEnable()
    {
        // Subscribe to the player transform event
        playerTransformEventChannel.OnEventRaised += HandleTransformPlayer;
    }

    private void OnDisable()
    {
        // Unsubscribe from the player transform event
        playerTransformEventChannel.OnEventRaised -= HandleTransformPlayer;
    }

    private void Update()
    {
        // Calculate the target point in world space
        targetPoint = spaceship.position + spaceship.forward * distance;

        // Convert the target point from world space to screen space
        screenPoint = mainCamera.WorldToScreenPoint(targetPoint);

        // Set the crosshair position to the screen point
        transform.position = screenPoint;
    }

    // Handle the player transform event
    private void HandleTransformPlayer(Transform playerTransform)
    {
        spaceship = playerTransform; // Update the reference to the player's spaceship transform
    }
}