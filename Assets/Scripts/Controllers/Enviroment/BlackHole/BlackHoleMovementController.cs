using UnityEngine;
using Helper;

public class BlackHoleMovementController : MonoBehaviour
{
    private const int ThresholdSpeed = 85;

    [Header("Player publishing channels")]
    [SerializeField] private TransformEventChannelSO playerTransformEventChannel;
    [SerializeField] private FloatEventChannelSO speedChangedEventChannel;

    [Header("Movement properties")]
    [SerializeField] private float baseSpeed = 100f; // Base speed of the black hole.

    // Player Transform
    private Transform playerTransform;

    // Game Object Components
    private BlackHoleStatusController blackHoleStatusController;
    private Rigidbody rigidBodyBlackHole;
    private Vector3 blackHoleVelocity;

    // Other Variables
    private float currentDistanceFromPlayer;
    private float playerForwardY;
    private float maxDistanceFromPlayer;
    private float intialPlayerPositionY;

    private void Awake()
    {
        if (speedChangedEventChannel == null || playerTransformEventChannel == null)
        {
            Debug.LogError("One or more channels are null in BlackHoleMovementController");
        }

        rigidBodyBlackHole = GetComponent<Rigidbody>();
        if (rigidBodyBlackHole == null)
        {
            Debug.LogError("rigidBodyBlackHole is null in BlackHoleMovementController");
        }

        blackHoleStatusController = GetComponent<BlackHoleStatusController>();
        if (blackHoleStatusController == null)
        {
            Debug.LogError("blackHoleStatusController is null in BlackHoleMovementController");
        }

        ResetPosition();
        maxDistanceFromPlayer = GlobalConstants.spawnLength / 5; // 2000
    }

    private void OnEnable()
    {
        // Subscribe to the events when the object is enabled.
        speedChangedEventChannel.OnEventRaised += HandlePlayerSpeedChanged;
        playerTransformEventChannel.OnEventRaised += HandlePlayerTransform;
    }

    private void OnDisable()
    {
        // Unsubscribe from the events when the object is disabled.
        speedChangedEventChannel.OnEventRaised -= HandlePlayerSpeedChanged;
        playerTransformEventChannel.OnEventRaised -= HandlePlayerTransform;
    }

    // Reset the position of the black hole.
    public void ResetPosition()
    {
        transform.position = new Vector3(0, intialPlayerPositionY - maxDistanceFromPlayer, 0);
    }

    // Handle the event to update the player's transform.
    private void HandlePlayerTransform(Transform transform)
    {
        intialPlayerPositionY = transform.position.y;
        if(transform == null)
        {
            Debug.LogError("transform is null in BlackHoleMovementController");
        }
        playerTransform = transform;
    }

    // Handle the event to update the moving state of the black hole.
    public void HandleIsMovingChange(bool isMoving)
    {
        if (isMoving)
        {
            rigidBodyBlackHole.linearVelocity = new Vector3(0, baseSpeed, 0);
        }
        else
        {
            rigidBodyBlackHole.linearVelocity = Vector3.zero;
        }
    }

    // Handle the event when the player's speed changes.
    private void HandlePlayerSpeedChanged(float playerSpeed)
    {   
        // If the black hole is in the "isMoving" state.
        if (blackHoleStatusController.IsMoving)
        {
            // Reset the velocity of the black hole.
            blackHoleVelocity = Vector3.zero;

            // Get the forward direction of the player along the Y axis.
            playerForwardY = playerTransform.forward.y;

            // Calculate the distance from the player along the Y axis.
            currentDistanceFromPlayer = Mathf.Abs(playerTransform.position.y - transform.position.y);


            // If the game state is "Playing".
            if (blackHoleStatusController.currentState == GameState.Playing)
            {
                // If the player's speed is above the threshold and the player is moving forward.
                if (playerSpeed > ThresholdSpeed && playerForwardY > 0)
                {
                    // If the distance between the black hole and the player is below the threshold maxDistanceFromPlayer.
                    if (currentDistanceFromPlayer < maxDistanceFromPlayer)
                    {
                        blackHoleVelocity.y = -playerSpeed; // The black hole moves away from the player.
                    }
                    else
                    {
                        blackHoleVelocity.y = playerSpeed; // The black hole follows the player at the same speed.
                    }
                }
                // If the player's speed is below the threshold or the player is moving backward.
                else
                {
                    blackHoleVelocity.y = baseSpeed; // The black hole moves towards the player at a base speed.
                }
            }
            // Otherwise, if the game state is "Tutorial".
            else
            {
                blackHoleVelocity.y = playerSpeed; // The black hole moves towards the player at the player's speed and doesn't catch the player.
            }

            // Set the velocity of the black hole.
            rigidBodyBlackHole.linearVelocity = blackHoleVelocity;
        }
    }
}
