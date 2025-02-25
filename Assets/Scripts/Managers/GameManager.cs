using System;
using System.Collections.Generic;
using UnityEngine;
using Helper;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using System.Linq;

[System.Serializable]
public class GameData
{
    public int highScore;
}

[Serializable]
public class SettingsData
{
    public int musicVolumePercentage;
    public int effectsVolumePercentage;
    public int radioVolumePercentage;
    public int sensitivityValue;
}

public class GameManager : Singleton<AudioManager>, IManager
{
    private const string MenuEnviromentElementsPath = "Prefabs/EnvironmentObject/MenuEnviromentElements";
    private const string SecretKey = "SuperSegreto123!";
    private const string GameDataFileName = "GameData.json";
    private const string SettingsDataFileName = "SettingsData.json";

    // Event Channels
    private GameStateEventChannelSO gameStateEventChannel; // Channel for game state change communication (source: GameManager)
    private UIMessageEventChannelSO userInterfaceEventChannel; // Channel for button press communication in the menu (source: UIManager)
    private BooleanEventChannelSO playerIsDeathEventChannel;
    private BooleanEventChannelSO pauseEventChannel; // Channel for pause communication (source: spaceship)
    private GameObjectEventChannelSO choosedSpaceShipsEventChannel;
    private IntEventChannelSO playerScoreEventChannel;
    private IntEventChannelSO highScoreEventChannel;
    private SettingsDataEventChannelSO settingsDataEventChannel;
    private IntEventChannelSO musicVolumeEventChannel;
    private IntEventChannelSO effectsVolumeEventChannel;
    private IntEventChannelSO radioVolumeEventChannel;
    private IntEventChannelSO sensitivityEventChannel;

    // Player Reference
    private GameObject player;

    // Main Menu Game Object (Planet and Asteroids)
    private GameObject menuEnviromentElement;

    // Current player score
    private int currentPlayerScore;

    // Dirty Flags
    private bool channelsConfigured = false;
    private bool flagFileGameDataExists;

    // Data Variables
    private GameData gameData = null;
    private SettingsData settingsData = null;

    // Other Variables
    private GameState _currentState;

    // Path Properties
    private string GetGameDataFilePath => Path.Combine(Application.persistentDataPath, GameDataFileName);
    private string GetSettingsDataFilePath => Path.Combine(Application.persistentDataPath, SettingsDataFileName);

    //////////////////////////////// INTERFACE METHODS ////////////////////////////////

    public void Initialize() { }

    public void RegisterDelegates()
    {
        userInterfaceEventChannel.OnEventRaised += HandleUIEvent;
        playerIsDeathEventChannel.OnEventRaised += HandlePlayerDeath;
        pauseEventChannel.OnEventRaised += HandlePauseRequest;
        choosedSpaceShipsEventChannel.OnEventRaised += HandleChoosedSpaceShip;
        playerScoreEventChannel.OnEventRaised += HandlePlayerScoreChange;
        settingsDataEventChannel.OnEventRaised += HandleSettingDataChanged;
        musicVolumeEventChannel.OnEventRaised += HandleMusicVolumeChanged;
        effectsVolumeEventChannel.OnEventRaised += HandleEffectsVolumeChanged;
        radioVolumeEventChannel.OnEventRaised += HandleRadioVolumeChanged;
        sensitivityEventChannel.OnEventRaised += HandleSensitivityChanged;
    }

    public void ConfigureChannels(Dictionary<Type, ScriptableObject> channels)
    {
        var channelMappings = new (Type type, Action<ScriptableObject> assign, string name)[]
        {
        (typeof(GameStateEventChannelSO), obj => gameStateEventChannel = (GameStateEventChannelSO)obj, "GameStateEventChannelSO"),
        (typeof(UIMessageEventChannelSO), obj => userInterfaceEventChannel = (UIMessageEventChannelSO)obj, "UIMessageEventChannelSO"),
        (typeof(BooleanIsDeathEventChannelSO), obj => playerIsDeathEventChannel = (BooleanEventChannelSO)obj, "BooleanIsDeathEventChannelSO"),
        (typeof(BooleanEventChannelSO), obj => pauseEventChannel = (BooleanEventChannelSO)obj, "BooleanEventChannelSO"),
        (typeof(GameObjectSpaceShipChoosedEventChannelSO), obj => choosedSpaceShipsEventChannel = (GameObjectEventChannelSO)obj, "GameObjectEventChannelSO"),
        (typeof(IntEventChannelSO), obj => playerScoreEventChannel = (IntEventChannelSO)obj, "IntEventChannelSO"),
        (typeof(IntHighScoreEventChannelSO), obj => highScoreEventChannel = (IntEventChannelSO)obj, "IntHighScoreEventChannelSO"),
        (typeof(SettingsDataEventChannelSO), obj => settingsDataEventChannel = (SettingsDataEventChannelSO)obj, "SettingsDataEventChannelSO"),
        (typeof(IntMusicVolumeEventChannelSO), obj => musicVolumeEventChannel = (IntEventChannelSO)obj, "IntMusicVolumeEventChannelSO"),
        (typeof(IntEffectsVolumeEventChannelSO), obj => effectsVolumeEventChannel = (IntEventChannelSO)obj, "IntEffectsVolumeEventChannelSO"),
        (typeof(IntRadioVolumeEventChannelSO), obj => radioVolumeEventChannel = (IntEventChannelSO)obj, "IntRadioVolumeEventChannelSO"),
        (typeof(IntSensitivityEventChannelSO), obj => sensitivityEventChannel = (IntEventChannelSO)obj, "IntSensitivityEventChannelSO")
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


    //////////////////////////////// LIFECYCLE METHODS ////////////////////////////////

    private void Awake()
    {
        player = FindPlayer();

        Physics.gravity = new Vector3(0, GlobalConstants.gravityAccelerationConstant, 0); // Change gravity along the Y axis

        menuEnviromentElement = (GameObject)Instantiate(Resources.Load(MenuEnviromentElementsPath));
        menuEnviromentElement.SetActive(false);

        // Check if the two data files (game data and game settings) exist, otherwise initialize the values and create the files
        CheckGameData(); 
        CheckSettingsData();
    }

    private void Start()
    {
        SetGameState(GameState.Initializing); // Initial game state

        settingsDataEventChannel.RaiseEvent(settingsData); // Send "settings" information to observers
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
        userInterfaceEventChannel.OnEventRaised -= HandleUIEvent;
        playerIsDeathEventChannel.OnEventRaised -= HandlePlayerDeath;
        choosedSpaceShipsEventChannel.OnEventRaised -= HandleChoosedSpaceShip;
        playerScoreEventChannel.OnEventRaised -= HandlePlayerScoreChange;
        settingsDataEventChannel.OnEventRaised -= HandleSettingDataChanged;
        musicVolumeEventChannel.OnEventRaised -= HandleMusicVolumeChanged;
        effectsVolumeEventChannel.OnEventRaised -= HandleEffectsVolumeChanged;
        radioVolumeEventChannel.OnEventRaised -= HandleRadioVolumeChanged;
        sensitivityEventChannel.OnEventRaised -= HandleSensitivityChanged;
    }

    private GameObject FindPlayer()
    {
        player = GameObject.FindGameObjectsWithTag(TagsHelper.Player).FirstOrDefault(obj => obj.transform.parent == null);
        if (player != null)
        {
            return player;
        }
        else
        {
            Debug.LogError("player is null in GameManager Awake");
            return null;
        }
    }


    //////////////////////////////// CLASS METHODS ////////////////////////////////

    private void SetGameState(GameState newState)
    {
        if (_currentState != newState)
        {
            _currentState = newState;
            OnGameStateChange(_currentState);
            gameStateEventChannel.RaiseEvent(_currentState);
        }
    }

    private void OnGameStateChange(GameState state)
    {
        switch (state)
        {
            case GameState.MainMenu:
                player.SetActive(false);
                player.SetActive(true);
                ReadGameData();
                highScoreEventChannel.RaiseEvent(gameData.highScore);
                menuEnviromentElement.SetActive(true);
                Time.timeScale = 1;
                break;

            case GameState.Playing:
                menuEnviromentElement.SetActive(false);
                Time.timeScale = 1;
                break;

            case GameState.Paused:
                Time.timeScale = 0;
                break;
            
            case GameState.GameOver:
                SaveHighScore(currentPlayerScore);
                break;

            case GameState.Tutorial:
                menuEnviromentElement.SetActive(false);
                Time.timeScale = 1;
                break;
        }
    }

    // Methods for managing game settings
    private void ReadSettingsData()
    {
        if (File.Exists(GetSettingsDataFilePath)) // if the settings file exists, read it
        {
            string json = File.ReadAllText(GetSettingsDataFilePath);
            settingsData = JsonUtility.FromJson<SettingsData>(json);
        }
    }

    private void SaveSettings(SettingsData settings)
    {
        string json = JsonUtility.ToJson(settings, true);
        File.WriteAllText(GetSettingsDataFilePath, json);
    }

    private void CheckSettingsData()
    {
        ReadSettingsData();

        if (settingsData == null) // the settings file does not exist
        {
            // Since the file does not exist, create default settings
            settingsData = new SettingsData { musicVolumePercentage = 70, effectsVolumePercentage = 70, radioVolumePercentage = 20, sensitivityValue = 2 };
            SaveSettings(settingsData);
        }
    }

    private void HandleSettingDataChanged(SettingsData data)
    {
        settingsData = data;
    }

    /*
     * Methods for managing the score
     */
    private void CheckGameData()
    {
        ReadGameData();

        if (gameData == null)
        {
            gameData = new GameData();
            gameData.highScore = 0;
            flagFileGameDataExists = false;
            SaveHighScore(gameData.highScore);
        }
        else
        {
            flagFileGameDataExists = true;
        }
    }

    private void ReadGameData()
    {
        if (File.Exists(GetGameDataFilePath)) 
        {
            string[] lines = File.ReadAllLines(GetGameDataFilePath);

            if (lines.Length < 2) // Invalid file
            {
                return; 
            }
               
            string json = lines[0];
            string savedHash = lines[1];

            if (ComputeHMAC(json, SecretKey) == savedHash.Trim()) // Integrity check
            {
                gameData = JsonUtility.FromJson<GameData>(json);
            }
            else
            {
                Debug.LogWarning("Data has been tampered with! Ignoring the file.");
            }
        }
    }

    private string ComputeHMAC(string data, string key)
    {
        using (HMACSHA256 hmac = new (Encoding.UTF8.GetBytes(key)))
        {
            byte[] hashBytes = hmac.ComputeHash(Encoding.UTF8.GetBytes(data));
            return Convert.ToBase64String(hashBytes);
        }
    }

    public void SaveHighScore(int score)
    {
        if (score > gameData.highScore || !flagFileGameDataExists) // if the incoming score is higher than the high score
        {
            gameData.highScore = score;
            string json = JsonUtility.ToJson(gameData, false);
            string hash = ComputeHMAC(json, SecretKey);
            File.WriteAllText(GetGameDataFilePath, json + Environment.NewLine + hash);
            Debug.Log("High score saved with digital signature!");
            flagFileGameDataExists = true;
        }    
    }

    private void ReplaceChild(GameObject newPrefab)
    {
        if (newPrefab == null)
        {
            Debug.LogError("newPrefab is null");
            return;
        }

        // Get the first child (index 0)
        Transform child = player.transform.GetChild(0);

        if (child != null)
        {
            // Save the position and rotation
            child.GetPositionAndRotation(out Vector3 position, out Quaternion rotation);

            // Destroy the current child
            Destroy(child.gameObject);

            // Instantiate the new prefab
            GameObject newChild = Instantiate(newPrefab, position, rotation);

            // Assign the parent
            newChild.transform.parent = player.transform;
        }
    }

    private void HandlePlayerScoreChange(int score)
    {
        currentPlayerScore = score;
    }

    private void HandleUIEvent(UIMessage message)
    {
        switch (message)
        {
            case UIMessage.GoToMainMenu:
                SetGameState(GameState.MainMenu);
                break;

            case UIMessage.StartGame:
                SetGameState(GameState.Playing);
                break;

            case UIMessage.ExitGame:
                SaveSettings(settingsData);
                Application.Quit();
                break;

            case UIMessage.GoToTrainingZone:
                SetGameState(GameState.Tutorial);
                break;
        }
    }

    private void HandlePlayerDeath(bool isDeath)
    {
        if (isDeath)
        {
            player.SetActive(false);
            SetGameState(GameState.GameOver);
        }
    }

    private void HandlePauseRequest(Boolean pause)
    {
        if (pause)
        {
            SetGameState(GameState.Paused);
        }
        else
        {
            SetGameState(GameState.Playing);
        }
    }

    private void HandleChoosedSpaceShip(GameObject shipChoosed)
    {
        ReplaceChild(shipChoosed);
    }

    /*
     * Game settings management (volume)
     */
    private void HandleMusicVolumeChanged(int musicVolumePercentage)
    {
        settingsData.musicVolumePercentage = musicVolumePercentage;
    }

    private void HandleEffectsVolumeChanged(int effectsVolumePercentage)
    {
        settingsData.effectsVolumePercentage = effectsVolumePercentage;
    }

    private void HandleRadioVolumeChanged(int radioVolumePercentage)
    {
        settingsData.radioVolumePercentage = radioVolumePercentage;
    }

    private void HandleSensitivityChanged(int sensitivityValue)
    {
        settingsData.sensitivityValue = sensitivityValue;
    }
}

public enum GameState
{
    None,
    Initializing,
    MainMenu,
    Playing,
    Paused,
    GameOver,
    Tutorial
}