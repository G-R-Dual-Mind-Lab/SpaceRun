using UnityEngine;

public class SpaceDetritusPSController : MonoBehaviour
{
    [Header("Event Channels")]
    [SerializeField] private GameStateEventChannelSO gsEventChannel; // Event channel for game state changes.

    // Game Object Components
    private ParticleSystem sdParticleSystem; // Reference to the ParticleSystem component.

    private void Awake()
    {
        if (gsEventChannel == null)
        {
            Debug.LogError("gameStateEventChannel is null in SpaceDetritusPSController");
        }
    }
    
    void Start()
    {
        // Get the reference to the ParticleSystem component on the same GameObject.
        sdParticleSystem = GetComponent<ParticleSystem>();
        
        // Check if the reference was found.
        if (sdParticleSystem == null)
        {
            Debug.LogError("sdParticleSystem is null in SpaceDetritusPSController");
        }
    }

    private void OnEnable()
    {
        // Subscribe to the game state change event when the object is enabled.
        gsEventChannel.OnEventRaised += HandleGameStateChanged;
    }

    private void OnDisable()
    {
        // Unsubscribe from the game state change event when the object is disabled.
        gsEventChannel.OnEventRaised -= HandleGameStateChanged;
    }

    // Handle the game state change event.
    private void HandleGameStateChanged(GameState state)
    {
        switch (state)
        {
            case GameState.MainMenu:
                // Pause the particle system when in the main menu.
                sdParticleSystem.Pause();
                break;

            case GameState.Playing:
                // Play the particle system when the game is playing.
                sdParticleSystem.Play();
                break;

            case GameState.Paused:
                // Pause the particle system when the game is paused.
                sdParticleSystem.Pause();
                break;
        }
    }
}