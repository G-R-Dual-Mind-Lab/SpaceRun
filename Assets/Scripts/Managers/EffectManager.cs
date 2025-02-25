using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Helper;
using UnityEditor;
using UnityEngine;
using UnityEngine.InputSystem;

public class EffectManager : Singleton<EffectManager>, IManager
{
    // Paths
    private const string BasePath = "Effects";
    private const string SpaceShipEffectsPath = "SpaceShip";

    // Dictionary category names
    private const string SpawnerCategory = "Spawner";
    private const string PlayerCategory = "Player";
    private const string MissileCategory = "Missile";

    // Sub-dictionary key names
    private const string MissileExplosionKey = "MissileExplosion";
    private const string DeathExplosionKey = "DeathExplosion";
    private const string HyperdriveKey = "Hyperdrive";
    private const string EnginePlumeKey = "EnginePlume";

    // Particle Systems Hierarchy Dictionary to store all the Particle Systems and manage them
    private Dictionary<string, Dictionary<string, ParticleSystem>> particleSystemHierarchy = new Dictionary<string, Dictionary<string, ParticleSystem>>();

    // Event Channels
    private GameObjectEventChannelSO missileHitEventChannel;
    private GameStateEventChannelSO gameStateEventChannel; // Channel for game state change communication (source: GameManager)
    private Vector3EventChannelSO playerDeathPositionEventChannel; // Channel for player death position communication (source: player)
    private FloatEventChannelSO playerAccelerationValueEventChannel;
    private BooleanEventChannelSO playerIsAcceleratingEventChannel;
    private BooleanEventChannelSO playerIsShakingEventChannel;
    private BooleanEventChannelSO playerIsBoostingEventChannel;

    // Player Reference
    private GameObject player;

    // Dirty Flags
    private bool channelsConfigured = false;
    private bool paused;

    // Variable used to scale the effects to be played
    private Vector3 scaleFactor;


    //////////////////////////////// INTERFACE METHODS ////////////////////////////////

    public void ConfigureChannels(Dictionary<Type, ScriptableObject> channels)
    {
        var channelMappings = new (Type type, Action<ScriptableObject> assign, string name)[]
        {
            (typeof(GameStateEventChannelSO), obj => gameStateEventChannel = (GameStateEventChannelSO)obj, "GameStateEventChannelSO"),
            (typeof(Vector3EventChannelSO), obj => playerDeathPositionEventChannel = (Vector3EventChannelSO)obj, "Vector3EventChannelSO"),
            (typeof(GameObjectEventChannelSO), obj => missileHitEventChannel = (GameObjectEventChannelSO)obj, "GameObjectEventChannelSO"),
            (typeof(FloatEventChannelSO), obj => playerAccelerationValueEventChannel = (FloatEventChannelSO)obj, "FloatEventChannelSO"),
            (typeof(BooleanIsAcceleratingEventChannelSO), obj => playerIsAcceleratingEventChannel = (BooleanEventChannelSO)obj, "BooleanIsAcceleratingEventChannelSO"),
            (typeof(BooleanIsShakingEventChannelSO), obj => playerIsShakingEventChannel = (BooleanEventChannelSO)obj, "BooleanIsShakingEventChannelSO"),
            (typeof(BooleanIsBoostingEventChannelSO), obj => playerIsBoostingEventChannel = (BooleanEventChannelSO)obj, "BooleanIsBoostingEventChannelSO")
        };

        foreach (var (type, assign, name) in channelMappings)
        {
            if (channels.TryGetValue(type, out var channel))
            {
                assign(channel);
            }
            else
            {
                Debug.LogError($"[GameManager] {name} not found in provided channels dictionary!");
            }
        }

        channelsConfigured = true;
        RegisterDelegates();
    }

    public void RegisterDelegates()
    {
        gameStateEventChannel.OnEventRaised += HandleGameStateChanged;
        playerAccelerationValueEventChannel.OnEventRaised += HandlePlayerAccelerationChange;
        playerIsBoostingEventChannel.OnEventRaised += HandlePlayerIsBoosting;
        playerIsShakingEventChannel.OnEventRaised += HandlePlayerIsShaking;
        playerDeathPositionEventChannel.OnEventRaised += HandlePlayerDeath;
        missileHitEventChannel.OnEventRaised += HandleMissileHit;
        playerIsAcceleratingEventChannel.OnEventRaised += HandlePlayerIsAccelerating;
    }

    public void Initialize() {}


    //////////////////////////////// LIFECYCLE METHODS ////////////////////////////////

    private void Awake()
    {
        // Retrieve the player from the scene
        player = GameObject.FindGameObjectsWithTag(TagsHelper.Player).FirstOrDefault(obj => obj.transform.parent == null);

        if (player == null)
        {
            Debug.LogError("Player is null in EffectManager Awake");
        }

        // Create the effects hierarchy (populate the dictionary)
        CreateParticleSystem(PlayerCategory, DeathExplosionKey, Path.Combine(BasePath, SpaceShipEffectsPath, "DeathExplosion"));
        CreateParticleSystem(MissileCategory, MissileExplosionKey, Path.Combine(BasePath, SpaceShipEffectsPath, "MissileExplosion"));
        CreateParticleSystem(PlayerCategory, HyperdriveKey, Path.Combine(BasePath, SpaceShipEffectsPath, "Hyperdrive"));
        GetEnvironmentParticleSystems();
    }

    private void OnEnable()
    {
        if (channelsConfigured)
        {
            RegisterDelegates();
        }
    }

    private void OnDisable()
    {
        playerIsAcceleratingEventChannel.OnEventRaised -= HandlePlayerIsAccelerating;
        playerAccelerationValueEventChannel.OnEventRaised -= HandlePlayerAccelerationChange;
        gameStateEventChannel.OnEventRaised -= HandleGameStateChanged;
        playerDeathPositionEventChannel.OnEventRaised -= HandlePlayerDeath;
        playerIsBoostingEventChannel.OnEventRaised -= HandlePlayerIsBoosting;
    }


    //////////////////////////////// CLASS METHODS ////////////////////////////////

    // Controller vibration management
    public void StartVibration(float lowFrequency, float highFrequency, float duration)
    {
        if (Gamepad.current != null)
        {
            Gamepad.current.SetMotorSpeeds(lowFrequency, highFrequency);
            if (duration != 0)
            {
                Invoke(nameof(StopVibration), duration);
            }
        }
    }

    public void StopVibration()
    {
        if (Gamepad.current != null)
        {
            Gamepad.current.SetMotorSpeeds(0, 0);
        }
    }

    // Particle system dictionary management
    private void CreateParticleSystem(string category, string key, string path)
    {
        if (!particleSystemHierarchy.ContainsKey(category))
        {
            particleSystemHierarchy[category] = new Dictionary<string, ParticleSystem>();
        }

        var particleSystem = Resources.Load<ParticleSystem>(path);
        particleSystemHierarchy[category][key] = particleSystem;
    }

    private ParticleSystem GetParticleSystem(string category, string key)
    {
        if (particleSystemHierarchy.TryGetValue(category, out var categoryDict) && categoryDict.TryGetValue(key, out var particleSystem))
        {
            return particleSystem;
        }
        Debug.LogError($"ParticleSystem '{key}' not found in category '{category}'.");
        return null;
    }

    private Dictionary<string, ParticleSystem> GetAllParticleSystemByCategory(string category)
    {
        if (particleSystemHierarchy.TryGetValue(category, out var categoryDict))
        {
            return categoryDict;
        }
        Debug.LogError($"Category '{category}' not found in particleSystemHierarchy.");
        return null;
    }

    private List<ParticleSystem> GetAllFilteredParticleSystemsByCategory(string category, string filter)
    {
        if (particleSystemHierarchy.TryGetValue(category, out var categoryDict))
        {
            return categoryDict
                .Where(kv => kv.Key.Contains(filter))
                .Select(kv => kv.Value)
                .ToList();
        }
        return null;
    }

    private void RemoveEntriesContainingKeySubstring(Dictionary<string, ParticleSystem> dictionary, string substring)
    {
        if (dictionary == null || string.IsNullOrEmpty(substring))
            return;

        var keysToRemove = dictionary.Keys.Where(key => key.ToString().Contains(substring)).ToList();

        foreach (var key in keysToRemove)
        {
            dictionary.Remove(key);
        }
    }

    private void ToggleParticleSystems(Dictionary<string, ParticleSystem> particleSystems, bool shouldPlay)
    {
        if (particleSystems == null) return;

        foreach (var ps in particleSystems.Values)
        {
            if (ps == null) continue;

            if (shouldPlay)
            {
                ps.Play();
            }
            else
            {
                ps.Stop();
                ps.Clear();
            }
        }
    }

    // Player Death Management
    private void HandlePlayerDeath(Vector3 shipDeathPosition)
    {
        Instantiate(GetParticleSystem(PlayerCategory, DeathExplosionKey), shipDeathPosition, Quaternion.identity);
        StartVibration(0.9f, 1.0f, 0.6f);
    }

    private void HandlePlayerIsAccelerating(bool isAccelerating)
    {
        if (isAccelerating)
        {
            foreach (var ps in GetAllFilteredParticleSystemsByCategory(PlayerCategory, EnginePlumeKey))
            {
                ps.Play();
            }
        }
        else
        {
            foreach (var ps in GetAllFilteredParticleSystemsByCategory(PlayerCategory, EnginePlumeKey))
            {
                ps.Stop();
            }
        }
    }

    private void HandlePlayerIsBoosting(bool IsBoosting)
    {
        if (IsBoosting)
        {
            foreach (var ps in GetAllFilteredParticleSystemsByCategory(PlayerCategory, EnginePlumeKey))
            {
                var main = ps.main; // Get the main module of the ParticleSystem
                main.startColor = Color.red; // Set the start color to red
            }
            foreach (var ps in GetAllFilteredParticleSystemsByCategory(PlayerCategory, HyperdriveKey))
            {
                ps.Play();
            }
        }
        else
        {
            foreach (var ps in GetAllFilteredParticleSystemsByCategory(PlayerCategory, EnginePlumeKey))
            {
                var main = ps.main;
                main.startColor = Color.cyan;
            }
            foreach (var ps in GetAllFilteredParticleSystemsByCategory(PlayerCategory, HyperdriveKey))
            {
                ps.Stop();
            }
        }
    }

    private void HandlePlayerAccelerationChange(float acceleration)
    {
        foreach (var ps in GetAllFilteredParticleSystemsByCategory(PlayerCategory, EnginePlumeKey))
        {
            var main = ps.main; // Access the Main Module
            main.startSize = Mathf.Lerp(20f, 30f, acceleration); // Modify the particle size
        }
        if (acceleration == 0) GetParticleSystem(PlayerCategory, HyperdriveKey).Stop();
    }

    private void HandlePlayerIsShaking(bool isShaking)
    {
        if (isShaking)
        {
            StartVibration(0.5f, 0.7f, 1f);
        }
        else
        {
            StopVibration();
        }
    }

    private void GetPlayerParticleSystems()
    {
        // Retrieve particle systems from the player
        ParticleSystem[] playerParticleSystems = player.GetComponentsInChildren<ParticleSystem>();

        Dictionary<string, ParticleSystem> playerPS = new Dictionary<string, ParticleSystem>();

        foreach (ParticleSystem pS in playerParticleSystems)
        {
            pS.Stop();
            pS.Clear();
            playerPS.Add(pS.name, pS);
        }

        // Remove any keys that contain the string "EnginePlume" to avoid conflicts with adding new EnginePlume ParticleSystems
        RemoveEntriesContainingKeySubstring(GetAllParticleSystemByCategory(PlayerCategory), EnginePlumeKey);

        // Insert the new EnginePlume ParticleSystems into the dictionary under the Player key
        particleSystemHierarchy[PlayerCategory] = GetAllParticleSystemByCategory(PlayerCategory).Concat(playerPS).ToDictionary(kvp => kvp.Key, kvp => kvp.Value);
    }

    private void InjectHyperdrive()
    {
        if (particleSystemHierarchy.TryGetValue(PlayerCategory, out Dictionary<string, ParticleSystem> categoryDict) && categoryDict.TryGetValue(HyperdriveKey, out ParticleSystem pS))
        {
            ParticleSystem newPS = Instantiate(pS, player.transform);
            categoryDict[HyperdriveKey] = newPS; // Update the reference in the dictionary
        }
    }

    private void EjectHyperdrive()
    {
        if (particleSystemHierarchy.TryGetValue(PlayerCategory, out Dictionary<string, ParticleSystem> categoryDict) && categoryDict.TryGetValue(HyperdriveKey, out ParticleSystem pS))
        {
            foreach (ParticleSystem ps in player.GetComponentsInChildren<ParticleSystem>())
            {
                if (ps.name.Contains(HyperdriveKey))
                {
                    Destroy(ps.gameObject);
                    categoryDict.Remove(HyperdriveKey);
                    CreateParticleSystem(PlayerCategory, HyperdriveKey, Path.Combine(BasePath, SpaceShipEffectsPath, HyperdriveKey));
                }
            }
        }
    }

    private void GetEnvironmentParticleSystems()
    {
        // Retrieve the Spawner from the scene
        GameObject spawner = GameObject.FindWithTag(TagsHelper.Spawner);

        if (spawner == null)
        {
            Debug.LogError("Spawner is null in EffectManager GetEnvironmentParticleSystems");
        }

        // Retrieve particle systems from the environment
        ParticleSystem[] environmentParticleSystems = spawner.GetComponentsInChildren<ParticleSystem>();

        Dictionary<string, ParticleSystem> environmentPS = new Dictionary<string, ParticleSystem>();

        foreach (ParticleSystem pS in environmentParticleSystems)
        {
            environmentPS.Add(pS.name, pS);
        }

        particleSystemHierarchy[SpawnerCategory] = environmentPS;
    }

    // Reproduce the missile explosion effect on the base of the size of the object hit
    private void HandleMissileHit(GameObject hitObject)
    {
        scaleFactor = hitObject.transform.localScale;

        foreach (Transform child in GetParticleSystem(MissileCategory, MissileExplosionKey).transform)
        {
            child.localScale = scaleFactor;
        }
        GetParticleSystem(MissileCategory, MissileExplosionKey).transform.localScale = scaleFactor;

        Instantiate(GetParticleSystem(MissileCategory, MissileExplosionKey), hitObject.transform.position, Quaternion.identity).Play();
    }

    /*
     * Game state management
     */
    private void HandleGameStateChanged(GameState state)
    {
        switch (state)
        {
            case GameState.Initializing:
                break;

            case GameState.MainMenu:
                paused = false;
                EjectHyperdrive();
                ToggleParticleSystems(GetAllParticleSystemByCategory(SpawnerCategory), false);
                break;

            case GameState.Playing:
                if (!paused)
                {
                    GetPlayerParticleSystems();
                    InjectHyperdrive();
                }
                ToggleParticleSystems(GetAllParticleSystemByCategory(SpawnerCategory), true);
                break;

            case GameState.Paused:
                paused = true;
                StopVibration();
                break;

            case GameState.GameOver:
                EjectHyperdrive();
                break;

            case GameState.Tutorial:
                if (!paused)
                {
                    GetPlayerParticleSystems();
                    InjectHyperdrive();
                }
                ToggleParticleSystems(GetAllParticleSystemByCategory(SpawnerCategory), true);
                break;
        }
    }
}