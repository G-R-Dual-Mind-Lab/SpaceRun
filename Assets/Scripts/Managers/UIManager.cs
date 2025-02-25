using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UIManager : Singleton<UIManager>, IManager
{
    // Dictionary Hierarchy
    private Dictionary<string, Dictionary<string, GameObject>> userInterfaceHierarchy = new();

    // Event Channels
    private GameStateEventChannelSO gameStateEventChannel; // Channel for game state change communication (source: GameManager)
    private IntEventChannelSO highScoreEventChannel;
    private SettingsDataEventChannelSO settingsDataEventChannel;
    private IntEventChannelSO playerScoreEventChannel;
    private FloatEventChannelSO distanceTravelledEventChannel;

    // Dirty Flags
    private bool channelsConfigured = false;
    private bool isPreviousStatePlaying = false;
    private bool isPreviousStatePaused = false;

    // Player Variables
    private int currentPlayerScore;
    private float currentPlayerDistance;

    // Dictionary categories
    private const string CanvasCategory = "Canvas";

    // Sub-dictionary keys
    private const string SplashCanvasKey = "Splash";
    private const string MainCanvasKey = "MainMenu";
    private const string GameOverPlayingCanvasKey = "GameOverPlaying";
    private const string GameOverTutorialCanvasKey = "GameOverTutorial";
    private const string PauseCanvasPlayingKey = "PausePlaying";
    private const string PauseCanvasTutorialKey = "PauseTutorial";
    private const string HUDPlayingCanvasKey = "HUDPlaying";
    private const string HUDTutorialCanvasKey = "HUDTutorial";
    private const string OcclusionCanvasKey = "Occlusion";

    // UI element names
    private const string DistanceValueGameOverUIElement = "DistanceValue";
    private const string ScoreValuePauseUIElement = "ScoreValue";
    private const string DistanceValuePauseUIElement = "DistanceValue";
    private const string HighScoreValueMainMenuUIElement = "HighScoreValue";
    private const string ScoreValueGameOverUIElement = "ScoreValue";
    private const string MusicSliderNameUIElement = "MusicSlider";
    private const string EffectsSliderNameUIElement = "EffectsSlider";
    private const string RadioSliderNameUIElement = "RadioSlider";
    private const string SensitivitySliderNameUIElement = "SensitivitySlider";

    // Prefab paths
    private const string HUDTutorialCanvasPath = "Prefabs/UI/HUD/HUDTutorialCanvas";

    //////////////////////////////// INTERFACE METHODS ////////////////////////////////

    public void Initialize() { }

    public void ConfigureChannels(Dictionary<Type, ScriptableObject> channels) // Method used to configure channels (IManager interface)
    {
        var channelMappings = new (Type type, Action<ScriptableObject> assign, string name)[]
        {
            (typeof(GameStateEventChannelSO), obj => gameStateEventChannel = (GameStateEventChannelSO)obj, "GameStateEventChannelSO"),
            (typeof(IntHighScoreEventChannelSO), obj => highScoreEventChannel = (IntEventChannelSO)obj, "IntHighScoreEventChannelSO"),
            (typeof(SettingsDataEventChannelSO), obj => settingsDataEventChannel = (SettingsDataEventChannelSO)obj, "SettingsDataEventChannelSO"),
            (typeof(IntEventChannelSO), obj => playerScoreEventChannel = (IntEventChannelSO)obj, "IntEventChannelSO"),
            (typeof(FloatDistanceTravelledEventChannelSO), obj => distanceTravelledEventChannel = (FloatEventChannelSO)obj, "FloatEventChannelSO"),
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
        highScoreEventChannel.OnEventRaised += HandleHighScore;
        settingsDataEventChannel.OnEventRaised += HandleSettingDataChanged;
        playerScoreEventChannel.OnEventRaised += HandlePlayerScoreChange;
        distanceTravelledEventChannel.OnEventRaised += HandleDistanceTravelledChange;
    }

    //////////////////////////////// LIFECYCLE METHODS ////////////////////////////////

    private void Awake()
    {
        CreateUIGameObject(CanvasCategory, SplashCanvasKey, "Prefabs/UI/Canvas/SplashCanvas");
        CreateUIGameObject(CanvasCategory, MainCanvasKey, "Prefabs/UI/Canvas/MainCanvas");
        CreateUIGameObject(CanvasCategory, GameOverPlayingCanvasKey, "Prefabs/UI/Canvas/GameOverCanvasPlaying");
        CreateUIGameObject(CanvasCategory, GameOverTutorialCanvasKey, "Prefabs/UI/Canvas/GameOverCanvasTutorial");
        CreateUIGameObject(CanvasCategory, PauseCanvasPlayingKey, "Prefabs/UI/Canvas/PauseCanvasPlaying");
        CreateUIGameObject(CanvasCategory, PauseCanvasTutorialKey, "Prefabs/UI/Canvas/PauseCanvasTutorial");
        CreateUIGameObject(CanvasCategory, HUDPlayingCanvasKey, "Prefabs/UI/HUD/HUDPlayingCanvas");
        CreateUIGameObject(CanvasCategory, OcclusionCanvasKey, "Prefabs/UI/HUD/OcclusionCanvas");
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
        highScoreEventChannel.OnEventRaised -= HandleHighScore;
        settingsDataEventChannel.OnEventRaised -= HandleSettingDataChanged;
        playerScoreEventChannel.OnEventRaised -= HandlePlayerScoreChange;
        distanceTravelledEventChannel.OnEventRaised -= HandleDistanceTravelledChange;
    }

    //////////////////////////////// CLASS METHODS ////////////////////////////////

    private void CreateUIGameObject(string category, string key, string path)
    {
        if (!userInterfaceHierarchy.ContainsKey(category))
        {
            userInterfaceHierarchy[category] = new Dictionary<string, GameObject>();
        }

        var uiGameObject = Instantiate(Resources.Load<GameObject>(path));
        uiGameObject.SetActive(false);
        userInterfaceHierarchy[category][key] = uiGameObject;
    }

    private GameObject GetUIElement(string category, string key)
    {
        if (userInterfaceHierarchy.TryGetValue(category, out var categoryDict) && categoryDict.TryGetValue(key, out var uiGameObject))
        {
            return uiGameObject;
        }

        Debug.LogWarning($"GameObject '{key}' not found in category '{category}'.");
        return null;
    }

    private void SetRecapInfoInGameOverCanvas()
    {
        TextMeshProUGUI[] texts = GetUIElement(CanvasCategory, GameOverPlayingCanvasKey).GetComponentsInChildren<TextMeshProUGUI>(true);

        foreach (TextMeshProUGUI tmp in texts)
        {
            if (tmp.name == ScoreValueGameOverUIElement)
            {
                tmp.text = currentPlayerScore.ToString();
            }
            else if (tmp.name == DistanceValueGameOverUIElement)
            {
                tmp.text = currentPlayerDistance.ToString("F5") + " l.y.";
            }
        }
    }

    private void SetRecapInfoInPauseCanvas()
    {
        TextMeshProUGUI[] texts = GetUIElement(CanvasCategory, PauseCanvasPlayingKey).GetComponentsInChildren<TextMeshProUGUI>(true);

        foreach (TextMeshProUGUI tmp in texts)
        {
            if (tmp.name == ScoreValuePauseUIElement)
            {
                tmp.text = currentPlayerScore.ToString();
            }
            else if (tmp.name == DistanceValuePauseUIElement)
            {
                tmp.text = currentPlayerDistance.ToString("F5") + " l.y.";
            }
        }
    }

    /*
     * Game settings management (volume)
     */
    private void HandleSettingDataChanged(SettingsData settingsData)
    {
        Slider[] sliders = GetUIElement(CanvasCategory, MainCanvasKey).GetComponentsInChildren<Slider>();

        foreach (Slider sld in sliders)
        {
            switch (sld.name)
            {
                case MusicSliderNameUIElement:
                    sld.value = settingsData.musicVolumePercentage;
                    break;

                case EffectsSliderNameUIElement:
                    sld.value = settingsData.effectsVolumePercentage;
                    break;

                case RadioSliderNameUIElement:
                    sld.value = settingsData.radioVolumePercentage;
                    break;
                    
                case SensitivitySliderNameUIElement:
                    sld.value = settingsData.sensitivityValue;
                    break;
            }
        }
    }

    private void ShowCanvas(string canvasName)
    {
        GetUIElement(CanvasCategory, canvasName).SetActive(true);
    }

    private void HideCanvas(string canvasName)
    {
        GetUIElement(CanvasCategory, canvasName).SetActive(false);
    }

    private void HandleHighScore(int highScore)
    {
        TextMeshPro[] texts = GetUIElement(CanvasCategory, MainCanvasKey).GetComponentsInChildren<TextMeshPro>();
        foreach (TextMeshPro tmp in texts)
        {
            if (tmp.name == HighScoreValueMainMenuUIElement)
            {
                tmp.text = highScore.ToString();
                break;
            }
        }
    }

    private void HandlePlayerScoreChange(int score)
    {
        currentPlayerScore = score;
    }

    private void HandleDistanceTravelledChange(float distance)
    {
        currentPlayerDistance = distance;
    }

    private void HandleGameStateChanged(GameState state)
    {
        switch (state)
        {
            case GameState.Initializing:
                ShowCanvas(SplashCanvasKey);
                break;

            case GameState.MainMenu:
                isPreviousStatePaused = false;

                if (GetUIElement(CanvasCategory, SplashCanvasKey).activeInHierarchy) HideCanvas(SplashCanvasKey);

                if (GetUIElement(CanvasCategory, GameOverPlayingCanvasKey).activeInHierarchy) HideCanvas(GameOverPlayingCanvasKey);

                if (GetUIElement(CanvasCategory, GameOverTutorialCanvasKey).activeInHierarchy) HideCanvas(GameOverTutorialCanvasKey);

                if (GetUIElement(CanvasCategory, PauseCanvasPlayingKey).activeInHierarchy) HideCanvas(PauseCanvasPlayingKey);
                
                if (GetUIElement(CanvasCategory, PauseCanvasTutorialKey).activeInHierarchy) HideCanvas(PauseCanvasTutorialKey);

                if (GetUIElement(CanvasCategory, HUDTutorialCanvasKey)) Destroy(GetUIElement(CanvasCategory, HUDTutorialCanvasKey).gameObject);

                ShowCanvas(MainCanvasKey);

                HideCanvas(OcclusionCanvasKey);
                
                break;

            case GameState.Playing:
                isPreviousStatePlaying = true;

                if (GetUIElement(CanvasCategory, MainCanvasKey).activeInHierarchy) HideCanvas(MainCanvasKey);

                if (GetUIElement(CanvasCategory, PauseCanvasPlayingKey).activeInHierarchy) HideCanvas(PauseCanvasPlayingKey);

                ShowCanvas(HUDPlayingCanvasKey);

                ShowCanvas(OcclusionCanvasKey);
                
                break;

            case GameState.Paused:
                isPreviousStatePaused = true;

                if (isPreviousStatePlaying)
                {
                    if (GetUIElement(CanvasCategory, HUDPlayingCanvasKey).activeInHierarchy) HideCanvas(HUDPlayingCanvasKey);
                    SetRecapInfoInPauseCanvas();
                    ShowCanvas(PauseCanvasPlayingKey);
                }
                else
                {
                    if (GetUIElement(CanvasCategory, HUDTutorialCanvasKey).activeInHierarchy) HideCanvas(HUDTutorialCanvasKey);
                    ShowCanvas(PauseCanvasTutorialKey);
                }
                break;

            case GameState.GameOver:
                if (isPreviousStatePlaying)
                {
                    if (GetUIElement(CanvasCategory, HUDPlayingCanvasKey).activeInHierarchy) HideCanvas(HUDPlayingCanvasKey);
                    SetRecapInfoInGameOverCanvas();
                    ShowCanvas(GameOverPlayingCanvasKey);
                }
                else
                {
                    if (GetUIElement(CanvasCategory, HUDTutorialCanvasKey).activeInHierarchy) HideCanvas(HUDTutorialCanvasKey);
                    ShowCanvas(GameOverTutorialCanvasKey);
                }
                break;

            case GameState.Tutorial:
                isPreviousStatePlaying = false;

                if (GetUIElement(CanvasCategory, MainCanvasKey).activeInHierarchy) HideCanvas(MainCanvasKey);
                if (GetUIElement(CanvasCategory, PauseCanvasTutorialKey).activeInHierarchy) HideCanvas(PauseCanvasTutorialKey);
                if (!isPreviousStatePaused)
                {
                    CreateUIGameObject(CanvasCategory, HUDTutorialCanvasKey, HUDTutorialCanvasPath);
                }
                else
                {
                    isPreviousStatePaused = false;
                }
                ShowCanvas(HUDTutorialCanvasKey);
                ShowCanvas(OcclusionCanvasKey);
                break;
        }
    }
}