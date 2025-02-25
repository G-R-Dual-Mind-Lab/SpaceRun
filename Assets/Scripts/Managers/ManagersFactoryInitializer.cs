using System.Collections.Generic;
using UnityEngine;
using System;

public class ManagersFactoryInitializer : MonoBehaviour
{
    Dictionary<Type, ScriptableObject> eventChannels = new();

    [Header("Manager publishing channels")]
    [SerializeField] private GameStateEventChannelSO gameSateEventChannel;
    [SerializeField] private UIMessageEventChannelSO userInterfaceEventChannel;
    [SerializeField] private IntEventChannelSO highScoreEventChannel;
    [SerializeField] private SettingsDataEventChannelSO settingsDataEventChannel;

    [Header("Player publishing channels | State")]
    [SerializeField] private BooleanEventChannelSO playerIsFiringEventChannel;
    [SerializeField] private BooleanEventChannelSO playerIsShakingEventChannel;
    [SerializeField] private BooleanEventChannelSO playerIsAcceleratingEventChannel;
    [SerializeField] private BooleanEventChannelSO playerIsInDangerEventChannel;
    [SerializeField] private BooleanEventChannelSO playerIsBoostingEventChannel;
    [SerializeField] private BooleanEventChannelSO playerIsDeathEventChannel;
    [SerializeField] private IntEventChannelSO playerScoreEventChannel;
    [SerializeField] private FloatEventChannelSO distanceTravelledEventChannel;
    

    [Header("Player publishing channels | General")]
    [SerializeField] private FloatEventChannelSO playerAccelerationValueEventChannel;
    [SerializeField] private Vector3EventChannelSO playerDeathPositionEventChannel;
    [SerializeField] private BooleanEventChannelSO pauseEventChannel;

    [Header("Player publishing channels | Radio")]
    [SerializeField] private BooleanEventChannelSO radioStatusEventChannel;
    [SerializeField] private AudioClipEventChannelSO reproduceRadioClipEventChannel;

    [Header("Missile publishing channels")]
    [SerializeField] private GameObjectEventChannelSO missileHitEventChannel;

    [Header("Canvas publishing channels")]
    [SerializeField] private GameObjectEventChannelSO choosedSpaceShipEventChannel;
    [SerializeField] private IntEventChannelSO musicVolumeEventChannel;
    [SerializeField] private IntEventChannelSO effectsVolumeEventChannel;
    [SerializeField] private IntEventChannelSO radioVolumeEventChannel;
    [SerializeField] private IntEventChannelSO sensitivityEventChannel;


    //////////////////////////////// LIFECYCLE METHODS ////////////////////////////////

    void Awake()
    {
        // Publisher GameManager
        eventChannels.Add(typeof(GameStateEventChannelSO), gameSateEventChannel);
        eventChannels.Add(typeof(IntHighScoreEventChannelSO), highScoreEventChannel);
        eventChannels.Add(typeof(SettingsDataEventChannelSO), settingsDataEventChannel);
        
        // Publisher UIManager
        eventChannels.Add(typeof(UIMessageEventChannelSO), userInterfaceEventChannel);

        // Publisher Player | General
        eventChannels.Add(typeof(BooleanEventChannelSO), pauseEventChannel);
        eventChannels.Add(typeof(Vector3EventChannelSO), playerDeathPositionEventChannel);
        eventChannels.Add(typeof(FloatEventChannelSO), playerAccelerationValueEventChannel);

        // Publisher Player | Status
        eventChannels.Add(typeof(BooleanIsShakingEventChannelSO), playerIsShakingEventChannel);
        eventChannels.Add(typeof(BooleanIsFiringEventChannelSO), playerIsFiringEventChannel);
        eventChannels.Add(typeof(BooleanIsBoostingEventChannelSO), playerIsBoostingEventChannel);
        eventChannels.Add(typeof(BooleanIsAcceleratingEventChannelSO), playerIsAcceleratingEventChannel);
        eventChannels.Add(typeof(BooleanIsDeathEventChannelSO), playerIsDeathEventChannel);
        eventChannels.Add(typeof(BooleanIsInDangerEventChannelSO), playerIsInDangerEventChannel);
        eventChannels.Add(typeof(IntEventChannelSO), playerScoreEventChannel);
        eventChannels.Add(typeof(FloatDistanceTravelledEventChannelSO), distanceTravelledEventChannel);

        // Publisher Player | Radio
        eventChannels.Add(typeof(BooleanRadioStatusEventChannelSO), radioStatusEventChannel);
        eventChannels.Add(typeof(AudioClipEventChannelSO), reproduceRadioClipEventChannel);

        eventChannels.Add(typeof(GameObjectEventChannelSO), missileHitEventChannel);

        eventChannels.Add(typeof(GameObjectSpaceShipChoosedEventChannelSO), choosedSpaceShipEventChannel);

        eventChannels.Add(typeof(IntMusicVolumeEventChannelSO), musicVolumeEventChannel);
        eventChannels.Add(typeof(IntEffectsVolumeEventChannelSO), effectsVolumeEventChannel);
        eventChannels.Add(typeof(IntRadioVolumeEventChannelSO), radioVolumeEventChannel);
        eventChannels.Add(typeof(IntSensitivityEventChannelSO), sensitivityEventChannel);

        // Initialize managers using the SingletonFactory
        GameManager gameManager = SingletonFactory.GetManager<GameManager>(eventChannels);
        UIManager uiManager = SingletonFactory.GetManager<UIManager>(eventChannels);
        AudioManager audioManager = SingletonFactory.GetManager<AudioManager>(eventChannels);
        CameraManager cameraManager = SingletonFactory.GetManager<CameraManager>(eventChannels);
        EffectManager effectManager = SingletonFactory.GetManager<EffectManager>(eventChannels);
    }
}

// Dummy classes used to populate the dictionary
public class BooleanIsFiringEventChannelSO : BooleanEventChannelSO { }
public class BooleanIsShakingEventChannelSO : BooleanEventChannelSO { }
public class BooleanRadioStatusEventChannelSO : BooleanEventChannelSO { }
public class BooleanIsAcceleratingEventChannelSO : BooleanEventChannelSO { }
public class BooleanIsBoostingEventChannelSO : BooleanEventChannelSO { }
public class BooleanIsDeathEventChannelSO : BooleanEventChannelSO { }
public class BooleanIsInDangerEventChannelSO : BooleanEventChannelSO { }
public class GameObjectSpaceShipChoosedEventChannelSO : GameObjectEventChannelSO { }
public class IntHighScoreEventChannelSO : IntEventChannelSO { }
public class FloatDistanceTravelledEventChannelSO : FloatEventChannelSO { }
public class IntMusicVolumeEventChannelSO : IntEventChannelSO { }
public class IntEffectsVolumeEventChannelSO : IntEventChannelSO { }
public class IntRadioVolumeEventChannelSO : IntEventChannelSO { }
public class IntSensitivityEventChannelSO : IntEventChannelSO { }