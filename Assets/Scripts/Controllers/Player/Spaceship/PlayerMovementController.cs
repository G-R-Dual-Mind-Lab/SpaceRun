using System;
using System.Collections;
using System.Linq;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerMovementController : MonoBehaviour
{   
    [Header("GameManager Publishing Channels")]
    [SerializeField] private GameStateEventChannelSO gameStateEventChannel;

    [Header("Player Publishing Channels")]
    [SerializeField] private TransformEventChannelSO playerTransformEventChannel;

    [Header("Spaceship Features")]
    [SerializeField] private float dampingFactor = 3f;
    [SerializeField] private float decelerationFactor = 0.8f;
    [SerializeField] private float rotationRate = 2f;
    [SerializeField] [Range(1, 5)] private float boost;

    // Shared ScriptableObject containing spaceship properties
    [SerializeField] private SpaceShipProperties shipProperties;
    private float throttleResponse;
    private float maxSpeed;

    // Game Object Components
    private Rigidbody rbSpaceship;
    private PlayerStatusController playerStatus;
    private PlayerInput playerInput;
    private PlayerDataController playerDataController;
    private Animator playerAnimator;

    // Dirty Flag
    private bool firstTimePlaying = true;

    // Initial Positions
    private Vector3 initialPlayerPosition = new(4.304367f, -6202.7f, -7.799982f);
    private Quaternion initialPlayerRotation = Quaternion.Euler(-182.979f, 49.3f, 220.396f);
    private Vector3 initialAnimationPosition = new(-8.4448f, -4000f, 24.721f);
    private Quaternion initialAnimationRotation = Quaternion.Euler(-269.13f, 56.905f, 274.6f);

    // Other Variables
    private Vector3 targetLinearVelocity;
    private Vector3 targetAngularVelocity;

    // Variable to nullify the boost effect before and after taking it, in FixedUpdate
    private float boostFactor;

    //////////////////////////////// LIFECYCLE METHODS ///////////////////////////////

    private void Awake()
    {
        rbSpaceship = GetComponent<Rigidbody>(); // Get the Rigidbody component of the spaceship
        playerStatus = GetComponent<PlayerStatusController>(); // Get the PlayerStatusController component
        playerInput = GetComponent<PlayerInput>(); // Get the PlayerInput component
        playerDataController = GetComponent<PlayerDataController>();

        if (playerInput == null)
        {
            Debug.LogError("playerInput is null in PlayerMovementController Awake");
        }

        if (playerDataController == null)
        {
            Debug.LogError("playerDataController is null in PlayerMovementController Awake");
        }

        if (rbSpaceship == null)
        {
            Debug.LogError("rbSpaceship is null in PlayerMovementController Awake");
        }

        if (playerStatus == null)
        {
            Debug.LogError("playerStatus is null in PlayerMovementController Awake");
        }

        if (gameStateEventChannel == null || playerTransformEventChannel == null)
        {
            Debug.LogError("One or more channels are null in PlayerMovementController Awake");
        }

        // Initially, no boost
        boostFactor = 1;

        // Set the initial position of the player
        transform.position = initialPlayerPosition;
        transform.rotation = initialPlayerRotation;
    }

    private void Start()
    {
        // Send the player's transform to the scripts that need it
        playerTransformEventChannel.RaiseEvent(transform);
    }

    private void OnEnable()
    {   
        // Restore the player's position
        transform.position = initialPlayerPosition;
        transform.rotation = initialPlayerRotation;

        gameStateEventChannel.OnEventRaised += HandleGameStateChanged;
    }

    private void OnDisable()
    {
        gameStateEventChannel.OnEventRaised -= HandleGameStateChanged;
        firstTimePlaying = true;
    }

    private void FixedUpdate()
    {
        // === LINEAR ACCELERATION HANDLING ===  
        if (playerStatus.IsAccelerating) // Handle spaceship acceleration
        {
            targetLinearVelocity = boostFactor * maxSpeed * playerStatus.Acceleration * transform.forward; // Calculate the target velocity

            rbSpaceship.linearVelocity = Vector3.Lerp(
                rbSpaceship.linearVelocity,             // Current velocity
                targetLinearVelocity,             // Desired velocity
                Time.fixedDeltaTime * throttleResponse // Interpolation speed
            );

            playerDataController.Speed = rbSpaceship.linearVelocity.magnitude; // Update speed in PlayerDataController
            playerDataController.PlayerPositionYAxis = transform.position.y;
        }
        else
        {
            rbSpaceship.linearVelocity *= decelerationFactor; // Gradually reduce the velocity
            playerDataController.Speed = rbSpaceship.linearVelocity.magnitude;
            playerDataController.PlayerPositionYAxis = transform.position.y;
        }

        // === ANGULAR VELOCITY HANDLING ===  
        targetAngularVelocity = Vector3.zero;

        if (playerStatus.IsPitching)
        {
            targetAngularVelocity += transform.right * -playerStatus.Rotation.y;
            playerDataController.Pitch = transform.localEulerAngles.y;
        }
        if (playerStatus.IsYawing)
        {
            targetAngularVelocity += transform.up * playerStatus.Rotation.x;
            playerDataController.Yaw = transform.localEulerAngles.x;
        }
        if (playerStatus.IsRolling)
        {
            targetAngularVelocity += transform.forward * -playerStatus.Rotation.z;
            playerDataController.Roll = transform.localEulerAngles.z;
        }

        // Gradual interpolation
        rbSpaceship.angularVelocity = Vector3.Lerp(
            rbSpaceship.angularVelocity,
            targetAngularVelocity * rotationRate,
            Time.fixedDeltaTime * dampingFactor
        );
    }

    //////////////////////////////// CLASS METHODS ////////////////////////////////

    private void SetShipModelProperties()
    {
        maxSpeed = shipProperties.maxSpeed;
        throttleResponse = shipProperties.acceleration;
    }

    private void CheckStartGameSession()
    {
        // If entering Playing/Tutorial from the menu, play the animation
        if (firstTimePlaying)
        {
            playerTransformEventChannel.RaiseEvent(transform);
            StartCoroutine(WaitForAllAnimations());
            firstTimePlaying = false;
        }
        // Otherwise, if returning to Playing/Tutorial from pause, do not replay the spaceship animation
        else
        {
            playerInput.enabled = true;
        }
    }

    // Modify the boost value used in FixedUpdate to calculate the target speed of the spaceship
    public void OnChangeBoostingState()
    {
        boostFactor = playerStatus.IsBoosting ? boost : 1;
    }

    private void ResetVelocity()
    {
        rbSpaceship.linearVelocity = Vector3.zero;
        rbSpaceship.angularVelocity = Vector3.zero;
    }

    private void HandleGameStateChanged(GameState state)
    {
        switch (state)
        {
            case GameState.Initializing:
                playerInput.enabled = false;
                GetComponentInChildren<MeshRenderer>().enabled = false;
                break;

            case GameState.MainMenu:
                ResetVelocity();
                playerInput.enabled = false;
                GetComponentInChildren<MeshRenderer>().enabled = true;
                break;

            case GameState.Paused:
                playerInput.enabled = false;
                break;

            case GameState.Playing:
                SetShipModelProperties();
                CheckStartGameSession();
                break;

            case GameState.Tutorial:
                SetShipModelProperties();
                CheckStartGameSession();
                break;
        }
    }

    /*
     * Coroutine to wait for all animations to finish
     */
    IEnumerator WaitForAllAnimations()
    {
        transform.position = initialAnimationPosition;
        transform.rotation = initialAnimationRotation;

        playerAnimator = GetComponent<Animator>();
        playerAnimator.enabled = true;
        // Sum the duration of all clips in the controller
        float totalDuration = playerAnimator.runtimeAnimatorController.animationClips.Sum(clip => clip.length);
        yield return new WaitForSeconds(totalDuration);
        playerAnimator.enabled = false;
        playerInput.enabled = true;
    }
}