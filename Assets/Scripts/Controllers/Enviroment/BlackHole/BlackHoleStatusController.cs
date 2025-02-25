using UnityEngine;

public class BlackHoleStatusController : MonoBehaviour
{
    [Header("GameManager publishing channels")]
    [SerializeField] private GameStateEventChannelSO gameStateEventChannel;

    private BlackHoleMovementController blackHoleMovementController;

    // Dirty Flag
    public GameState currentState;

    // Status variable
    private bool isMoving;

    // Status property
    public bool IsMoving
    {
        get { return isMoving; }

        set
        {
            if (isMoving == value) return;

            isMoving = value;
            blackHoleMovementController.HandleIsMovingChange(isMoving);
        }
    }

    private void Awake()
    {
        if (gameStateEventChannel == null)
        {
            Debug.LogError("One or more channels are null in BlackHoleStatusController");
        }

        blackHoleMovementController = GetComponent<BlackHoleMovementController>();

        if (blackHoleMovementController == null)
        {
            Debug.LogError("blackHoleMovementController is null in BlackHoleStatusController");
        }
    }

    private void OnEnable()
    {
        // Subscribe to the game state change event when the object is enabled.
        gameStateEventChannel.OnEventRaised += HandleGameStateChange;
    }

    private void OnDisable()
    {
        // Unsubscribe from the game state change event when the object is disabled.
        gameStateEventChannel.OnEventRaised -= HandleGameStateChange;
    }

    // Handle the game state change event.
    private void HandleGameStateChange(GameState newState)
    {
        switch (newState)
        {
            case GameState.MainMenu:
                currentState = GameState.MainMenu;
                IsMoving = false;
                blackHoleMovementController.ResetPosition();
                break;

            case GameState.Playing:
                currentState = GameState.Playing;
                IsMoving = true;
                break;
            
            case GameState.GameOver:
                currentState = GameState.GameOver;
                IsMoving = false;
                break;

            case GameState.Tutorial:
                currentState = GameState.Tutorial;
                IsMoving = true;
                break;
        }
    }
}