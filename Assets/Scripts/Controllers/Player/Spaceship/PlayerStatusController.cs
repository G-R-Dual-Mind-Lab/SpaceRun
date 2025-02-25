using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections;

public class PlayerStatusController : MonoBehaviour
{
    [Header("Player publishing channels | State")]
    [SerializeField] private BooleanEventChannelSO playerIsFiringEventChannel;
    [SerializeField] private BooleanEventChannelSO playerIsShakingEventChannel;
    [SerializeField] private BooleanEventChannelSO playerIsAcceleratingEventChannel;
    [SerializeField] private BooleanEventChannelSO playerIsInDangerEventChannel;
    [SerializeField] private BooleanEventChannelSO playerIsBoostingEventChannel;
    [SerializeField] private BooleanEventChannelSO playerIsDeathEventChannel;
    [SerializeField] private BooleanEventChannelSO playerIsPitchingEventChannel;
    [SerializeField] private BooleanEventChannelSO playerIsYawingEventChannel;
    [SerializeField] private BooleanEventChannelSO playerIsRollingEventChannel;

    [Header("Player publishing channels | General")]
    [SerializeField] private FloatEventChannelSO playerAccelerationValueEventChannel;
    [SerializeField] private Vector3EventChannelSO playerDeathPositionEventChannel;

    [Header("Other Event Channels")]
    [SerializeField] private SettingsDataEventChannelSO settingsDataEventChannel;
    [SerializeField] private IntEventChannelSO setSensitivityEventChannel;
    [SerializeField] private BooleanEventChannelSO pauseEventChannel;

    [Header("Player controllers")]
    [SerializeField] private CameraShakingController cameraShakingController;

    // Sensitivity values for pitch, yaw, and roll
    private float pitchSensitivity;
    private float yawSensitivity;
    private float rollSensitivity;

    // Input Variable
    private Vector2 inputs;

    // Coroutine Variable, necessary to start and potentially suspend it (before completion)
    private Coroutine disableCoroutine;

    // Game Object Components
    private RevisedGun playerFiringController;
    private PlayerMovementController playerMovementController;

    // Boolean Status Variables
    private bool isPitching = false; // Whether the spaceship is pitching
    private bool isYawing = false; // Whether the spaceship is yawing
    private bool isRolling = false; // Whether the spaceship is rolling
    private bool isAccelerating = false; // Whether the spaceship is accelerating
    private bool isFiring = false; // Whether the spaceship is firing
    private bool isDeath = false; // Whether the spaceship is dead
    private bool isInDanger = false; // Whether the spaceship is in danger
    private bool isShaking = false; // Whether the spaceship is shaking
    private bool isBoosting = false; // Whether the spaceship is boosting

    // Rotation and Acceleration Variables
    private Vector3 rotation;
    private float acceleration;

    // Properties
    public bool IsPitching
    {
        get => isPitching;

        set
        {
            if (isPitching == value)
                return;

            isPitching = value;
            playerIsPitchingEventChannel.RaiseEvent(isPitching);
        }
    }

    public bool IsYawing
    {
        get => isYawing;

        set
        {
            if (isYawing == value)
                return;

            isYawing = value;
            playerIsYawingEventChannel.RaiseEvent(isYawing);
        }
    }

    public bool IsRolling
    {
        get => isRolling;

        set
        {
            if (isRolling == value)
                return;

            isRolling = value;
            playerIsRollingEventChannel.RaiseEvent(isRolling);
        }
    }

    public bool IsAccelerating
    {
        get => isAccelerating;

        set
        {
            if (isAccelerating == value)
                return;

            isAccelerating = value;
            playerIsAcceleratingEventChannel.RaiseEvent(isAccelerating);
        }
    }

    public bool IsFiring
    {
        get => isFiring;

        set
        {
            if (isFiring == value)
                return;

            isFiring = value;
            playerIsFiringEventChannel.RaiseEvent(isFiring);
        }
    }

    public bool IsDeath
    {
        get => isDeath;

        set
        {
            if (isDeath == value)
                return;

            if (value == true)
            {
                if (IsShaking) // If the player dies while shaking, set it to false
                {
                    IsShaking = false;
                }
                if (IsBoosting) // If the player dies while boosting, set it to false
                {
                    IsBoosting = false;
                }
                playerDeathPositionEventChannel.RaiseEvent(transform.position);
            }

            isDeath = value;
            playerIsDeathEventChannel.RaiseEvent(isDeath);
        }
    }

    public bool IsShaking
    {
        get => isShaking;

        set
        {
            if (isShaking == value)
                return;

            isShaking = value;
            playerIsShakingEventChannel.RaiseEvent(isShaking);
        }
    }

    public bool IsBoosting
    {
        get => isBoosting;

        set
        {
            if (isBoosting == value)
                return;

            if (value == true)
                StartCoroutine(StopBoostAfterDelay(5f));

            isBoosting = value;
            playerMovementController.OnChangeBoostingState();
            playerIsBoostingEventChannel.RaiseEvent(isBoosting);
        }
    }

    public bool IsInDanger
    {
        get => isInDanger;

        set
        {
            if (isInDanger == value)
                return;

            if (value == true)
            {
                disableCoroutine = StartCoroutine(PlayerIsDeathAfterDelay(5f));
            }
            else
            {
                if (disableCoroutine != null)
                {
                    StopCoroutine(disableCoroutine);
                }
            }

            isInDanger = value;
            playerIsInDangerEventChannel.RaiseEvent(isInDanger);
        }
    }

    public Vector3 Rotation
    {
        get => rotation;

        set
        {
            if (rotation == value)
                return;

            rotation = value;
        }
    }

    public float Acceleration
    {
        get => acceleration;

        set
        {
            if (acceleration == value)
                return;

            acceleration = value;
            playerAccelerationValueEventChannel.OnEventRaised(acceleration);
        }
    }

    //////////////////////////////// LIFECYCLE METHODS ////////////////////////////////

    private void Awake()
    {
        playerFiringController = GetComponent<RevisedGun>();
        playerMovementController = GetComponent<PlayerMovementController>();

        if (playerMovementController == null)
        {
            Debug.LogError("playerMovementController is null in PlayerStatusController Awake");
        }

        if (playerFiringController == null)
        {
            Debug.LogError("playerFiringController is null in PlayerStatusController Awake");
        }

        if (playerIsShakingEventChannel == null ||
            playerIsDeathEventChannel == null ||
            playerIsFiringEventChannel == null ||
            playerIsPitchingEventChannel == null ||
            playerIsYawingEventChannel == null ||
            playerIsRollingEventChannel == null ||
            setSensitivityEventChannel == null ||
            settingsDataEventChannel == null)
        {
            Debug.LogError("Communication channels error in PlayerStatusController Awake");
        }
    }

    private void OnEnable()
    {
        isPitching = false;
        isYawing = false;
        isRolling = false;
        isAccelerating = false;
        isFiring = false;
        isDeath = false;
        isInDanger = false;
        isShaking = false;
        IsBoosting = false;

        setSensitivityEventChannel.OnEventRaised += HandleSensitivityChange;
        settingsDataEventChannel.OnEventRaised += HandleSettingsDataChange;
    }

    private void OnDisable()
    {
        setSensitivityEventChannel.OnEventRaised -= HandleSensitivityChange;
        settingsDataEventChannel.OnEventRaised -= HandleSettingsDataChange;
    }

    //////////////////////////////// CLASS METHODS ////////////////////////////////

    // Handle acceleration input
    public void OnAcceleration(InputAction.CallbackContext context)
    {
        if (context.performed)
        {
            Acceleration = context.ReadValue<float>();
            IsAccelerating = true;
        }
        else if (context.canceled)
        {
            Acceleration = 0f;
            IsAccelerating = false;
        }
    }

    // Handle yaw input
    public void OnYaw(InputAction.CallbackContext context)
    {
        if (context.performed || context.started)
        {
            inputs = context.ReadValue<Vector2>();

            // Small input movements are made even smaller, allowing for greater control precision
            // Math.Sign(f) returns the original sign of the input value
            rotation.x = Mathf.Sign(inputs.x) * Mathf.Pow(Mathf.Abs(inputs.x), 1.5f) * yawSensitivity;
            IsYawing = Mathf.Abs(inputs.x) > 0.1f; // Activate yaw only if there is significant movement
        }
        else if (context.canceled)
        {
            IsYawing = false;
            rotation.x = 0f;
        }
    }

    // Handle pitch input
    public void OnPitch(InputAction.CallbackContext context)
    {
        if (context.performed || context.started)
        {
            inputs = context.ReadValue<Vector2>();

            // Small input movements are made even smaller, allowing for greater control precision
            rotation.y = Mathf.Sign(inputs.y) * Mathf.Pow(Mathf.Abs(inputs.y), 1.5f) * pitchSensitivity;
            IsPitching = Mathf.Abs(inputs.y) > 0.1f; // Activate pitch only if there is significant movement
        }
        else if (context.canceled)
        {
            rotation.y = 0f;
            IsPitching = false;
        }
    }

    // Handle roll input
    public void OnRoll(InputAction.CallbackContext context)
    {
        if (context.performed || context.started)
        {
            inputs = context.ReadValue<Vector2>();

            // Small input movements are made even smaller, allowing for greater control precision
            rotation.z = Mathf.Sign(inputs.x) * Mathf.Pow(Mathf.Abs(inputs.x), 1.5f) * rollSensitivity;
            IsRolling = Mathf.Abs(inputs.x) > 0.1f; // Activate roll only if there is significant movement
        }
        else if (context.canceled)
        {
            rotation.z = 0f;
            IsRolling = false;
        }
    }

    // Handle fire input
    public void OnFire(InputAction.CallbackContext context)
    {
        if (context.performed)
        {
            if (playerFiringController.GetAvailableMissilesNumber != 0)
            {
                isFiring = true;
                playerFiringController.Fire();
                playerIsFiringEventChannel.OnEventRaised(isFiring);
            }
        }
        else if (context.canceled)
        {
            isFiring = false;
        }
    }

    // Handle pause input
    public void OnPauseEnter()
    {
        pauseEventChannel.RaiseEvent(true);   
    }

    // Handle sensitivity change event
    public void HandleSensitivityChange(int newSensitivity)
    {
        pitchSensitivity = newSensitivity;
        yawSensitivity = newSensitivity;
        rollSensitivity = newSensitivity * 2;
    }

    // Handle settings data change event
    public void HandleSettingsDataChange(SettingsData settingsData)
    {
        pitchSensitivity = settingsData.sensitivityValue;
        yawSensitivity = settingsData.sensitivityValue;
        rollSensitivity = settingsData.sensitivityValue * 2;
    }

    //////////////////////////////// COROUTINES ////////////////////////////////

    // Coroutine to set the player as dead after a delay
    private IEnumerator PlayerIsDeathAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        disableCoroutine = null;
        IsDeath = true;
    }

    // Coroutine to stop boosting after a delay
    private IEnumerator StopBoostAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        IsBoosting = false;
    }
}