using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class AudioManager : Singleton<AudioManager>, IManager
{
    // Base paths for audio files
    private const string SoundTracksPath = "AudioClips/SoundTracks";
    private const string SoundEffectsPath = "AudioClips/SoundEffects";
    private const string SpaceShipPath = "SpaceShip";
    private const string MenuSoundtrackName = "MenuSoundTrack";
    private const string AccelerationKey = "Acceleration";
    private const string ExplosionKey = "Explosion";
    private const string DebrisKey = "Debris";
    private const string FiringKey = "Firing";
    private const string AmbienceKey = "Ambience1";
    private const string RadioKey = "Radio1";
    private const string ChangeEffectKey = "ChangeEffect";
    private const string SpaceAmbiencePath = "SpaceAmbience";
    private const string RadioCommunicationAmbiencePath = "RadioCommunication";
    private const string RadioMusicPath = "RadioMusic";
    private const string MusicCategory = "Music";
    private const string RadioMusicKey = "RadioMusic";
    private const string MenuMusicKey = "MenuMusic";
    private const string EffectsCategory = "Effects";
    private const string RadioCategory = "Radio";

    // Dictionary to manage the hierarchy of sounds (music, sound effects, or spaceship radio)
    private Dictionary<string, Dictionary<string, AudioSource>> audioHierarchy = new Dictionary<string, Dictionary<string, AudioSource>>();

    // Event Channels
    private GameStateEventChannelSO gameStateEventChannel; // Channel for game state changes
    private BooleanEventChannelSO playerIsShakingEventChannel;
    private BooleanEventChannelSO playerIsFiringEventChannel;
    private BooleanEventChannelSO playerIsDeathEventChannel;
    private FloatEventChannelSO shipAccelerationEventChannel;
    private BooleanEventChannelSO radioStatusEventChannel;
    private AudioClipEventChannelSO reproduceRadioClipEventChannel;
    private IntEventChannelSO musicVolumeEventChannel;
    private IntEventChannelSO effectsVolumeEventChannel;
    private IntEventChannelSO radioVolumeEventChannel;
    private SettingsDataEventChannelSO settingsDataEventChannel;

    // Other Variables
    private bool channelsConfigured = false;
    private int currentMusicVolumePercentage;
    private int currentEffectsVolumePercentage;
    private int currentRadioVolumePercentage;
    private bool isPreviousStatePaused = false;


    //////////////////////////////// INTERFACE METHODS ////////////////////////////////

    public void Initialize() { }

    public void ConfigureChannels(Dictionary<Type, ScriptableObject> channels)
    {
        var channelMappings = new (Type type, Action<ScriptableObject> assign, string name)[]
        {
            (typeof(GameStateEventChannelSO), obj => gameStateEventChannel = (GameStateEventChannelSO)obj, "GameStateEventChannelSO"),
            (typeof(BooleanIsDeathEventChannelSO), obj => playerIsDeathEventChannel = (BooleanEventChannelSO)obj, "BooleanIsDeathEventChannelSO"),
            (typeof(BooleanIsFiringEventChannelSO), obj => playerIsFiringEventChannel = (BooleanEventChannelSO)obj, "BooleanIsFiringEventChannelSO"),
            (typeof(BooleanIsShakingEventChannelSO), obj => playerIsShakingEventChannel = (BooleanEventChannelSO)obj, "BooleanIsShakingEventChannelSO"),
            (typeof(BooleanRadioStatusEventChannelSO), obj => radioStatusEventChannel = (BooleanEventChannelSO)obj, "BooleanRadioStatusEventChannelSO"),
            (typeof(FloatEventChannelSO), obj => shipAccelerationEventChannel = (FloatEventChannelSO)obj, "FloatEventChannelSO"),
            (typeof(AudioClipEventChannelSO), obj => reproduceRadioClipEventChannel = (AudioClipEventChannelSO)obj, "AudioClipEventChannelSO"),
            (typeof(IntMusicVolumeEventChannelSO), obj => musicVolumeEventChannel = (IntEventChannelSO)obj, "IntMusicVolumeEventChannelSO"),
            (typeof(IntEffectsVolumeEventChannelSO), obj => effectsVolumeEventChannel = (IntEventChannelSO)obj, "IntEffectsVolumeEventChannelSO"),
            (typeof(IntRadioVolumeEventChannelSO), obj => radioVolumeEventChannel = (IntEventChannelSO)obj, "IntRadioVolumeEventChannelSO"),
            (typeof(SettingsDataEventChannelSO), obj => settingsDataEventChannel = (SettingsDataEventChannelSO)obj, "SettingsDataEventChannelSO")
        };

        foreach (var (type, assign, name) in channelMappings)
        {
            if (channels.TryGetValue(type, out var channel))
            {
                assign(channel);
            }
            else
            {
                Debug.LogError($"[AudioManager] {name} not found in provided channels dictionary!");
            }
        }

        channelsConfigured = true;
        RegisterDelegates();
    }

    public void RegisterDelegates()
    {
        gameStateEventChannel.OnEventRaised += HandleGameStateChanged;
        playerIsDeathEventChannel.OnEventRaised += HandlePlayerDeath;
        playerIsFiringEventChannel.OnEventRaised += HandlePlayerIsFiring;
        shipAccelerationEventChannel.OnEventRaised += HandlePlayerAccelerationChange;
        playerIsShakingEventChannel.OnEventRaised += HandlePlayerIsShaking;
        radioStatusEventChannel.OnEventRaised += HandleRadioStatus;
        reproduceRadioClipEventChannel.OnEventRaised += HandleRadioChannelChange;
        musicVolumeEventChannel.OnEventRaised += HandleMusicVolumeChanged;
        effectsVolumeEventChannel.OnEventRaised += HandleEffectsVolumeChanged;
        radioVolumeEventChannel.OnEventRaised += HandleRadioVolumeChanged;
        settingsDataEventChannel.OnEventRaised += HandleSettingDataChanged;
    }


    //////////////////////////////// LIFECYCLE METHODS ////////////////////////////////

    private void Awake()
    {
        CreateAudioSource(MusicCategory, MenuMusicKey, Path.Combine(SoundTracksPath, MenuSoundtrackName), true);

        CreateAudioSource(EffectsCategory, AccelerationKey, Path.Combine(SoundEffectsPath, SpaceShipPath, "AccelerationSoundEffect"), true);
        CreateAudioSource(EffectsCategory, ExplosionKey, Path.Combine(SoundEffectsPath, SpaceShipPath, "DeathExplosionSoundEffects"));
        CreateAudioSource(EffectsCategory, DebrisKey, Path.Combine(SoundEffectsPath, SpaceShipPath, "DebrisSoundEffect"));
        CreateAudioSource(EffectsCategory, FiringKey, Path.Combine(SoundEffectsPath, SpaceShipPath, "MissileShootSoundEffect"));
        CreateAudioSource(EffectsCategory, RadioKey, Path.Combine(SoundEffectsPath, RadioCommunicationAmbiencePath, "Radio4"), false);

        CreateAudioSource(MusicCategory, AmbienceKey, Path.Combine(SoundEffectsPath, SpaceAmbiencePath, "Ambience1"), true);

        CreateAudioSource(RadioCategory, RadioMusicKey, null, true);
        CreateAudioSource(RadioCategory, ChangeEffectKey, Path.Combine(SoundEffectsPath, RadioMusicPath, "RadioEffect1"), false);

        // Necessary to set volumes to those read from the JSON file at game start
        currentMusicVolumePercentage = -1;
        currentEffectsVolumePercentage = -1;
        currentRadioVolumePercentage = -1;
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
        gameStateEventChannel.OnEventRaised -= HandleGameStateChanged;
        playerIsDeathEventChannel.OnEventRaised -= HandlePlayerDeath;
        playerIsFiringEventChannel.OnEventRaised -= HandlePlayerIsFiring;
        shipAccelerationEventChannel.OnEventRaised -= HandlePlayerAccelerationChange;
        playerIsShakingEventChannel.OnEventRaised -= HandlePlayerIsShaking;
        radioStatusEventChannel.OnEventRaised -= HandleRadioStatus;
        reproduceRadioClipEventChannel.OnEventRaised -= HandleRadioChannelChange;
        musicVolumeEventChannel.OnEventRaised -= HandleMusicVolumeChanged;
        effectsVolumeEventChannel.OnEventRaised -= HandleEffectsVolumeChanged;
        radioVolumeEventChannel.OnEventRaised -= HandleRadioVolumeChanged;
        settingsDataEventChannel.OnEventRaised -= HandleSettingDataChanged;
    }


    //////////////////////////////// CLASS METHODS ////////////////////////////////

    // Create an audio source and add it to the hierarchy
    private void CreateAudioSource(string category, string key, string path, bool loop = false, float volume = 1f)
    {
        if (!audioHierarchy.ContainsKey(category))
        {
            audioHierarchy[category] = new Dictionary<string, AudioSource>();
        }

        var source = gameObject.AddComponent<AudioSource>();

        if (!(path == null))
        {
            source.clip = Resources.Load<AudioClip>(path);
            source.loop = loop;
            source.volume = volume;
        }
        audioHierarchy[category][key] = source;
    }

    // Get an audio source from the hierarchy
    private AudioSource GetAudioSource(string category, string key)
    {
        if (audioHierarchy.TryGetValue(category, out var categoryDict) && categoryDict.TryGetValue(key, out var source))
        {
            return source;
        }
        return null;
    }

    // Set the volume for all audio sources in a category
    private void SetAudioSourceVolume(string category, int volumePercentage)
    {
        foreach (var categ in audioHierarchy)
        {
            if (categ.Key == category)
            {
                foreach (var audioPair in categ.Value)
                {
                    AudioSource audioSource = audioPair.Value;
                    if (audioSource != null)
                    {
                        audioSource.volume = volumePercentage / 100f; // Set the desired volume
                    }
                }
            }
        }
    }


    //////////////////////////////// RADIO MANAGEMENT ////////////////////////////////

    // Handle radio status change
    private void HandleRadioStatus(bool radioStatus)
    {
        switch (radioStatus)
        {
            case true:
                GetAudioSource(RadioCategory, RadioMusicKey).Play();
                break;

            case false:
                GetAudioSource(RadioCategory, RadioMusicKey).Stop();
                break;
        }
    }

    // Handle radio channel change
    private void HandleRadioChannelChange(AudioClip clip)
    {
        GetAudioSource(RadioCategory, ChangeEffectKey).Play();
        StartCoroutine(CrossfadeClips(GetAudioSource(RadioCategory, ChangeEffectKey), GetAudioSource(RadioCategory, RadioMusicKey), clip, GetAudioSource(RadioCategory, ChangeEffectKey).clip.length));
    }


    //////////////////////////////// PLAYER MANAGEMENT ////////////////////////////////

    // Handle player death event
    private void HandlePlayerDeath(bool isDeath)
    {
        if (isDeath)
        {
            GetAudioSource(EffectsCategory, ExplosionKey).Play();
        }
    }

    // Handle player shaking event
    private void HandlePlayerIsShaking(bool isShaking)
    {
        if (isShaking)
        {
            GetAudioSource(EffectsCategory, DebrisKey).Play();
        }
        else
        {
            StartCoroutine(FadeOut(GetAudioSource(EffectsCategory, DebrisKey), 1));
        }
    }

    // Handle player acceleration change event
    private void HandlePlayerAccelerationChange(float acceleration)
    {
        if(acceleration != 0 && GetAudioSource(EffectsCategory, AccelerationKey).isPlaying)
        {
            GetAudioSource(EffectsCategory, AccelerationKey).pitch = acceleration;
        }
        else if (acceleration != 0)
        {
            GetAudioSource(EffectsCategory, AccelerationKey).Play();
        }
        else
        {
            GetAudioSource(EffectsCategory, AccelerationKey).Stop();
            GetAudioSource(EffectsCategory, AccelerationKey).pitch = 1;
        }
    }

    // Handle player firing event
    private void HandlePlayerIsFiring(bool isFiring)
    {
        if (isFiring)
        {
            GetAudioSource(EffectsCategory, FiringKey).Play();
        }
    }


    //////////////////////////////// SETTINGS MANAGEMENT ////////////////////////////////

    // Handle music volume change event
    private void HandleMusicVolumeChanged(int musicVolumePercentage)
    {
        if ((currentMusicVolumePercentage == -1) || (currentMusicVolumePercentage != musicVolumePercentage))
        {
            currentMusicVolumePercentage = musicVolumePercentage;
            SetAudioSourceVolume(MusicCategory, currentMusicVolumePercentage);
        } 
    }

    // Handle effects volume change event
    private void HandleEffectsVolumeChanged(int effectsVolumePercentage)
    {
        if ((currentEffectsVolumePercentage == -1) || (currentEffectsVolumePercentage != effectsVolumePercentage))
        {
            currentEffectsVolumePercentage = effectsVolumePercentage;
            SetAudioSourceVolume(EffectsCategory, currentEffectsVolumePercentage);
        } 
    }

    // Handle radio volume change event
    private void HandleRadioVolumeChanged(int radioVolumePercentage)
    {
        if ((currentRadioVolumePercentage == -1) || (currentRadioVolumePercentage != radioVolumePercentage))
        {
            currentRadioVolumePercentage = radioVolumePercentage;
            SetAudioSourceVolume(RadioCategory, currentRadioVolumePercentage);
        } 
    }

    // Handle settings data change event
    private void HandleSettingDataChanged(SettingsData data)
    {
        currentMusicVolumePercentage = data.musicVolumePercentage;
        SetAudioSourceVolume(MusicCategory, currentMusicVolumePercentage);
        currentEffectsVolumePercentage = data.effectsVolumePercentage;
        SetAudioSourceVolume(EffectsCategory, currentEffectsVolumePercentage);
        currentRadioVolumePercentage = data.radioVolumePercentage;
        SetAudioSourceVolume(RadioCategory, currentRadioVolumePercentage);
    }

    //////////////////////////////// GAME STATE MANAGEMENT ////////////////////////////////

    // Handle game state change event
    private void HandleGameStateChanged(GameState state)
    {
        var menuMusic = GetAudioSource(MusicCategory, MenuMusicKey);
        var ambience = GetAudioSource(MusicCategory, AmbienceKey);
        var radioCommunicationsPlaying = GetAudioSource(EffectsCategory, RadioKey);
        var radioMusicPlaying = GetAudioSource(RadioCategory, RadioMusicKey);

        switch (state)
        {
            case GameState.Initializing:
                menuMusic.Play();
                break;

            case GameState.MainMenu:
                if (menuMusic != null && !menuMusic.isPlaying)
                {
                    menuMusic.Play();
                }
                radioCommunicationsPlaying.Stop();
                ambience.Stop();
                isPreviousStatePaused = false;
                break;

            case GameState.Playing:
                if (!isPreviousStatePaused)
                {
                    menuMusic.Stop();
                    ambience.Play();
                    radioCommunicationsPlaying.Play();
                    isPreviousStatePaused = false;
                }
                break;

            case GameState.Paused:
                isPreviousStatePaused = true;
                radioMusicPlaying.Stop();
                break;

            case GameState.GameOver:
                radioCommunicationsPlaying.Stop();
                break;
            
            case GameState.Tutorial:
                menuMusic.Stop();
                ambience.Play();
                break;
        }
    }
    

    //////////////////////////////// COROUTINES ////////////////////////////////

    // Coroutine to fade out an audio source
    private IEnumerator FadeOut(AudioSource audioSource, float duration)
    {
        float startVolume = audioSource.volume;
        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            audioSource.volume = Mathf.Lerp(startVolume, 0f, elapsed / duration);
            yield return null;
        }

        audioSource.Stop();
        audioSource.volume = 1; // Reset for the next fade-in
    }

    // Coroutine to crossfade between two audio sources
    private IEnumerator CrossfadeClips(AudioSource source1, AudioSource source2, AudioClip nextClip, float fadeDuration)
    {
        source2.clip = nextClip;
        source2.volume = 0;
        source2.Play();

        float elapsedTime = 0f;

        while (elapsedTime < fadeDuration)
        {
            elapsedTime += Time.deltaTime;
            source1.volume = Mathf.Lerp(currentRadioVolumePercentage/100f, 0, elapsedTime / fadeDuration);
            source2.volume = Mathf.Lerp(0, currentRadioVolumePercentage/100f, elapsedTime / fadeDuration);
            yield return null;
        }

        source1.Stop();
    }
}