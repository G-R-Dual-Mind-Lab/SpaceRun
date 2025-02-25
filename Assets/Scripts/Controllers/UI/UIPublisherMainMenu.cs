using UnityEngine;

public class UIPublisherMainMenu : MonoBehaviour
{
    [Header("Event Channels")]
    [SerializeField] private UIMessageEventChannelSO uiEventChannel; // canale per comunicare "bottoni" premuti su UI (sorgente UI)
    [SerializeField] private IntEventChannelSO musicVolumeEventChannel;
    [SerializeField] private IntEventChannelSO effectsVolumeEventChannel;
    [SerializeField] private IntEventChannelSO radioVolumeEventChannel;
    [SerializeField] private IntEventChannelSO sensitivityEventChannel;

    [Header("UI Elements")]
    [SerializeField] private GameObject musicSlider;
    [SerializeField] private GameObject effectsSlider;
    [SerializeField] private GameObject radioSlider;
    [SerializeField] private GameObject sensitivitySlider;
 
    void Start()
    {
        if (uiEventChannel == null ||
            musicVolumeEventChannel == null ||
            effectsVolumeEventChannel == null ||
            radioVolumeEventChannel == null ||
            sensitivityEventChannel == null)
        {
            Debug.LogError("One or more channels are null in UIPublisher Start");
        }
    }

    public void OnMusicSliderChange(float newVolumePercentage)
    {
        musicVolumeEventChannel.RaiseEvent((int)newVolumePercentage);
    }

    public void OnEffectsSliderChange(float newVolumePercentage)
    {
        effectsVolumeEventChannel.RaiseEvent((int)newVolumePercentage);
    }

    public void OnRadioSliderChange(float newVolumePercentage)
    {
        radioVolumeEventChannel.RaiseEvent((int)newVolumePercentage);
    }

    public void OnSensitivitySliderChange(float newSensitivity)
    {
        sensitivityEventChannel.RaiseEvent((int)newSensitivity);
    }

    public void ManageUIEvent(string message)
    {
        switch (message)
        {
            case "StartGame":
                uiEventChannel.RaiseEvent(UIMessage.StartGame);
                break;

            case "GoToMainMenu":
                Debug.Log("Messaggio inviato.");
                uiEventChannel.RaiseEvent(UIMessage.GoToMainMenu);
                break;

            case "GoToSettings":
                uiEventChannel.RaiseEvent(UIMessage.GoToSettings);
                break;

            case "GoToHangar":
                uiEventChannel.RaiseEvent(UIMessage.GoToHangar);
                break;

            case "GoToTrainingZone":
                uiEventChannel.RaiseEvent(UIMessage.GoToTrainingZone);
                break;

            case "ExitGame":
                uiEventChannel.RaiseEvent(UIMessage.ExitGame);
                break;

            case "Return":
                uiEventChannel.RaiseEvent(UIMessage.Return);
                break;

            case "Choose":
                uiEventChannel.RaiseEvent(UIMessage.Choose);
                break;
        }
    }
}

public enum UIMessage
{
    StartGame,
    GoToMainMenu,
    GoToSettings,
    GoToHangar,
    GoToTrainingZone,
    Shop,
    Continue,
    Return,
    Choose,
    Quit,
    ExitGame,
    Pause
}