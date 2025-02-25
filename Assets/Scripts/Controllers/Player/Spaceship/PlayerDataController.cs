using Helper;
using UnityEngine;

public class PlayerDataController : MonoBehaviour
{
    [Header("Event Channels")]
    [SerializeField] private FloatEventChannelSO pitchChangedEventChannel;
    [SerializeField] private FloatEventChannelSO rollChangedEventChannel;
    [SerializeField] private FloatEventChannelSO yawChangedEventChannel;
    [SerializeField] private FloatEventChannelSO speedChangedEventChannel;
    [SerializeField] private IntEventChannelSO missilesChangedEventChannel;
    [SerializeField] private FloatEventChannelSO distanceTravelledEventChannel;
    [SerializeField] private GameStateEventChannelSO gameStateEventChannel;
    [SerializeField] private IntEventChannelSO playerScoreEventChannel;

    // Player Data Variables
    private float previousPlayerPositionY;
    private float initialPlayerPositionY;
    private float pitch;
    private float roll;
    private float yaw;
    private float speed;
    private int missilesNumber;
    private float playerPositionYAxis;
    private int score;
    private float distanceTravelled;
    private Vector3 playerRotationEulerAngle;

    // Dirty Flag
    private bool gameWasInPause;

    // Properties
    public float Pitch
    {
        set
        {
            pitch = value;
            pitchChangedEventChannel.RaiseEvent(pitch);
        }
    }

    public float Roll
    {
        set
        {
            roll = value;
            rollChangedEventChannel.RaiseEvent(roll);
        }
    }

    public float Yaw
    {
        set
        {
            yaw = value;
            yawChangedEventChannel.RaiseEvent(yaw);
        }
    }

    public float Speed
    {
        set
        {
            if (value > 0.0001f || value == 0f)
            {
                speed = value;
                speedChangedEventChannel.RaiseEvent(speed);
            }
            return;
        }
    }

    public int MissilesNumber
    {
        set
        {
            missilesNumber = value;
            missilesChangedEventChannel.RaiseEvent(missilesNumber);
        }
    }

    public float PlayerPositionYAxis
    {
        set
        {
            if (transform.forward.y > 0) // If the spaceship is not facing the black hole
            {
                playerPositionYAxis = value;

                Score += (int)(Mathf.Abs(previousPlayerPositionY - playerPositionYAxis));
                previousPlayerPositionY = playerPositionYAxis;

                DistanceTravelled = (Mathf.Abs(initialPlayerPositionY - playerPositionYAxis)) / GlobalConstants.solarSystemScaleFactor;
            }
            else
            {
                previousPlayerPositionY = value;
            }
        }
    }

    public float DistanceTravelled
    {
        get { return distanceTravelled; }

        set
        {
            distanceTravelled = value;
            distanceTravelledEventChannel.RaiseEvent(distanceTravelled);
        }
    }

    public int Score
    {
        get { return score; }

        set
        {
            score = value;
            playerScoreEventChannel.RaiseEvent(score);
        }
    }

    private void Awake()
    {
        if (pitchChangedEventChannel == null ||
            rollChangedEventChannel == null ||
            yawChangedEventChannel == null ||
            speedChangedEventChannel == null ||
            missilesChangedEventChannel == null ||
            distanceTravelledEventChannel == null ||
            gameStateEventChannel == null ||
            playerScoreEventChannel == null)
        {
            Debug.LogError("One or more channels are null in PlayerDataController");
        }

        playerRotationEulerAngle = transform.rotation.eulerAngles;

        gameWasInPause = false;
    }

    private void OnEnable()
    {
        // Subscribe to the game state change event when the object is enabled.
        gameStateEventChannel.OnEventRaised += HandleGameStateChanged;
    }

    private void OnDisable()
    {
        // Unsubscribe from the game state change event when the object is disabled.
        gameStateEventChannel.OnEventRaised -= HandleGameStateChanged;
    }

    // Initialize player data values.
    private void InitializeValues()
    {
        initialPlayerPositionY = gameObject.transform.position.y;
        previousPlayerPositionY = initialPlayerPositionY;

        PlayerPositionYAxis = initialPlayerPositionY;
        Pitch = playerRotationEulerAngle.y;
        Roll = playerRotationEulerAngle.z;
        Yaw = playerRotationEulerAngle.x;
        Speed = 0f;
        MissilesNumber = 0;
        Score = 0;
        DistanceTravelled = 0f;
    }

    // Handle the game state change event.
    private void HandleGameStateChanged(GameState gameState)
    {
        switch (gameState)
        {
            case GameState.Playing:
                if (!gameWasInPause)
                {
                    InitializeValues();
                }
                else
                {
                    gameWasInPause = false;
                }
                break;
            
            case GameState.Paused:
                gameWasInPause = true;
                break;

            case GameState.MainMenu:
                gameWasInPause = false;
                break;

            case GameState.Tutorial:
                if (!gameWasInPause)
                {
                    InitializeValues();
                }
                else
                {
                    gameWasInPause = false;
                }
                break;
        }
    }
}