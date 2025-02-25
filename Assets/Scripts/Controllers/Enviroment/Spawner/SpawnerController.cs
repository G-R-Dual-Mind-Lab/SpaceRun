using UnityEngine;
using Helper;
using System.Collections;

public class SpawnerController : MonoBehaviour
{
    [Header("Channels")]
    [SerializeField] private GameStateEventChannelSO gsEventChannel; // Event channel for game state changes.

    [Header("Particle systems")]
    [SerializeField] private GameObject particleSystemsParent; // Parent object for all particle systems.
    [SerializeField] private ParticleSystem particleSystemDetritus; // Particle system for space detritus.
    [SerializeField] private ParticleSystem particleSystemElectric; // Particle system for electric effects.
    [SerializeField] private ParticleSystem particleSystemFog; // Particle system for fog effects.

    [Header("Bounds")]
    [SerializeField] private GameObject bounds; // Game object representing the bounds.

    [Header("Sensor")]
    [SerializeField] private GameObject sensor; // Game object representing the sensor.

    [Header("SpawnerVolume")]
    [SerializeField] private GameObject spawnerVolume; // Game object representing the spawner volume.

    [Header("ReticleTexture")]
    [SerializeField] private GameObject reticleTexture; // Game object representing the reticle texture.

    [Header("Translation properties")]
    [SerializeField] private float spawnerTranslationSpeed = 2000f; // Speed of the spawner translation.
    [SerializeField] private float scaleFactorTranslation = 1.5f; // Scale factor for the translation.

    // Children's Components
    private SpaceObjectSpawnerController spaceObjectSpawnerController; // Reference to the SpaceObjectSpawnerController component.
    private BonusSpawnerController bonusSpawnerController; // Reference to the BonusSpawnerController component.

    // Variables used for changing the speed of child particle systems
    private ParticleSystem.VelocityOverLifetimeModule velocityDetritusModule;
    private ParticleSystem.VelocityOverLifetimeModule velocityElectricityModule;
    private ParticleSystem.MinMaxCurve originalDetritusYVelocity;
    private ParticleSystem.MinMaxCurve originalElectricityYVelocity;
    private float elapsedTime;
    private float duration;
    private float inverseDuration;
    private float extraVelocityY;

    // Translation delta for the spawner
    private float deltaTranslation = GlobalConstants.spawnLength / 10; // Offset

    // Dirty Flags
    private bool firstTimeInMainMenu = true;
    private bool isPreviousStatePause = false;
    private bool isCoroutineActive = false;
    private bool isCoroutineStopped = false;

    // Position variables for child objects used for translation
    private Vector3 startPosition;
    private Vector3 targetPositionParticleSystemsParent;
    private Vector3 targetPositionSpawnerVolume;
    private Vector3 targetPositionBounds;
    private Vector3 sensorTargetPosition;
    private Vector3 reticleTextureTargetPosition;

    private void Awake()
    {
        spaceObjectSpawnerController = GetComponentInChildren<SpaceObjectSpawnerController>();

        if (spaceObjectSpawnerController == null)
        {
            Debug.LogError("spaceObjectSpawnerController is null in SpawnerController Awake");
        }

        bonusSpawnerController = GetComponentInChildren<BonusSpawnerController>();

        if (bonusSpawnerController == null)
        {
            Debug.LogError("bonusSpawnerController is null in SpawnerController Awake");
        }

        if (gsEventChannel == null)
        {
            Debug.LogError("gameStateEventChannel is null in SpawnerController Awake");
        }

        InitializeParticleModules();

        duration = deltaTranslation / spawnerTranslationSpeed;
        inverseDuration = 1f / duration;
        extraVelocityY = (deltaTranslation / duration) / scaleFactorTranslation;
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

    // Initialize the velocities variables of the particle system modules.
    private void InitializeParticleModules()
    {
        velocityDetritusModule = particleSystemDetritus.velocityOverLifetime;
        velocityElectricityModule = particleSystemElectric.velocityOverLifetime;
        originalDetritusYVelocity = velocityDetritusModule.y;
        originalElectricityYVelocity = velocityElectricityModule.y;
    }

    // Reset the positions of all child objects.
    private void ResetSpawnerPosition()
    {
        bounds.transform.position = Vector3.zero;
        spawnerVolume.transform.position = Vector3.zero;
        reticleTexture.transform.position = Vector3.zero;
        sensor.transform.position = new Vector3(0, -GlobalConstants.spawnLength / 4, 0);
        if (isCoroutineActive)
        {
            isCoroutineStopped = true;
            StopCoroutine(MoveSmoothly());
        }
        particleSystemsParent.transform.position = Vector3.zero;
    }

    // Method to translate all child objects.
    public void TranslateSpawner()
    {
        targetPositionBounds = bounds.transform.position;
        targetPositionBounds.y += deltaTranslation;
        bounds.transform.position = targetPositionBounds;

        targetPositionSpawnerVolume = spawnerVolume.transform.position;
        targetPositionSpawnerVolume.y += deltaTranslation;
        spawnerVolume.transform.position = targetPositionSpawnerVolume;

        reticleTextureTargetPosition = reticleTexture.transform.position;
        reticleTextureTargetPosition.y += deltaTranslation;
        reticleTexture.transform.position = reticleTextureTargetPosition;

        sensorTargetPosition = sensor.transform.position;
        sensorTargetPosition.y += deltaTranslation;
        sensor.transform.position = sensorTargetPosition;

        targetPositionParticleSystemsParent = particleSystemsParent.transform.position;
        targetPositionParticleSystemsParent.y += deltaTranslation;

        StartCoroutine(MoveSmoothly());
    }

    // Handle the game state change event.
    private void HandleGameStateChanged(GameState state)
    {
        switch (state)
        {
            case GameState.Initializing:
                spaceObjectSpawnerController.CurrentState = GameState.Initializing;
                spaceObjectSpawnerController.PoolFiller(); // Fill the space object pool.
                bonusSpawnerController.PoolsFiller(); // Fill the bonus pool.
                break;

            case GameState.MainMenu:
                spaceObjectSpawnerController.CurrentState = GameState.MainMenu;
                spaceObjectSpawnerController.ResetSpawner();
                if (!firstTimeInMainMenu) // If it's not the first time entering the main menu (i.e., the game has already started), reset the spawner position and return objects to the pool.
                {
                    ResetSpawnerPosition();
                    bonusSpawnerController.AllObjectsInPool(); // Return all objects to the pool.
                }
                firstTimeInMainMenu = false;
                isPreviousStatePause = false;
                break;

            case GameState.Playing:
                if (!isPreviousStatePause)
                {
                    spaceObjectSpawnerController.SpawnAllObjectsInPool();
                }
                else
                {
                    isPreviousStatePause = false;
                }
                spaceObjectSpawnerController.CurrentState = GameState.Playing;
                break;

            case GameState.Paused:
                isPreviousStatePause = true;
                spaceObjectSpawnerController.CurrentState = GameState.Paused;
                break;

            case GameState.GameOver:
                bonusSpawnerController.AllObjectsInPool(); // Return all objects to the pool.
                break;

            case GameState.Tutorial:
                isPreviousStatePause = false;
                break;
        }
    }

    // Smoothly translate the particle systems and temporarily increase the particle velocities to hide the movement.
    IEnumerator MoveSmoothly()
    {
        isCoroutineActive = true;
        isCoroutineStopped = false;
        elapsedTime = 0f;

        startPosition = particleSystemsParent.transform.position;

        // Temporarily modify the particle velocities.
        velocityDetritusModule.y = new ParticleSystem.MinMaxCurve(originalDetritusYVelocity.constant + extraVelocityY);
        velocityElectricityModule.y = new ParticleSystem.MinMaxCurve(originalElectricityYVelocity.constant + extraVelocityY);
        try
        {
            while (elapsedTime < duration)
            {
                particleSystemsParent.transform.position = Vector3.Lerp(startPosition, targetPositionParticleSystemsParent, elapsedTime * inverseDuration);
                elapsedTime += Time.deltaTime;
                yield return null; // Wait for the next frame.
            }

            // Restore the original particle velocities.
            velocityDetritusModule.y = originalDetritusYVelocity;
            velocityElectricityModule.y = originalElectricityYVelocity;

            // Ensure the final position is reached.
            if (!isCoroutineStopped)
            {
                particleSystemsParent.transform.position = targetPositionParticleSystemsParent;
            }
            else
            {
                particleSystemsParent.transform.position = Vector3.zero;
            }
        }
        finally
        {
            isCoroutineActive = false;
        }
    }
}