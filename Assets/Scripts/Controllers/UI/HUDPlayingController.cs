using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class HUDPlayingController : MonoBehaviour
{
    [Header("TextMeshPro elements")]
    [SerializeField] private TextMeshProUGUI textMeshProRoll;
    [SerializeField] private TextMeshProUGUI textMeshProPitch;
    [SerializeField] private TextMeshProUGUI textMeshProYaw;
    [SerializeField] private TextMeshProUGUI textMeshProScore;
    [SerializeField] private TextMeshProUGUI textMeshProDistanceTravelled;
    [SerializeField] private TextMeshProUGUI textMeshProSpeed;
    [SerializeField] private TextMeshProUGUI textMeshProMissilesNumber;
    [SerializeField] private TextMeshProUGUI textMeshProCuriosity;
    [SerializeField] private Image imagePanelCuriosity;

    [Header("Channels")]
    [SerializeField] private FloatEventChannelSO pitchChangedEventChannel;
    [SerializeField] private FloatEventChannelSO rollChangedEventChannel;
    [SerializeField] private FloatEventChannelSO yawChangedEventChannel;
    [SerializeField] private FloatEventChannelSO speedChangedEventChannel;
    [SerializeField] private IntEventChannelSO missilesChangedEventChannel;
    [SerializeField] private FloatEventChannelSO distanceTravelledEventChannel;
    [SerializeField] private IntEventChannelSO playerScoreEventChannel;

    private void Awake()
    {
        if (pitchChangedEventChannel == null || 
            rollChangedEventChannel == null || 
            yawChangedEventChannel == null ||
            speedChangedEventChannel == null || 
            missilesChangedEventChannel == null || 
            distanceTravelledEventChannel == null)
        {
            Debug.LogError("One or more channels are null in HUDController Awake");
        }
    }

    private void OnEnable()
    {
        // Subscribe to event channels
        pitchChangedEventChannel.OnEventRaised += HandlePitchChanged;
        rollChangedEventChannel.OnEventRaised += HandleRollChanged;
        yawChangedEventChannel.OnEventRaised += HandleYawChanged;
        speedChangedEventChannel.OnEventRaised += HandleSpeedChanged;
        missilesChangedEventChannel.OnEventRaised += HandleMissileNumbersChanged;
        distanceTravelledEventChannel.OnEventRaised += HandleDistanceTravelledChanged;
        playerScoreEventChannel.OnEventRaised += HandlePlayerScoreChange;
    }

    private void OnDisable()
    {
        // Unsubscribe from event channels
        pitchChangedEventChannel.OnEventRaised -= HandlePitchChanged;
        rollChangedEventChannel.OnEventRaised -= HandleRollChanged;
        yawChangedEventChannel.OnEventRaised -= HandleYawChanged;
        speedChangedEventChannel.OnEventRaised -= HandleSpeedChanged;
        missilesChangedEventChannel.OnEventRaised -= HandleMissileNumbersChanged;
        distanceTravelledEventChannel.OnEventRaised -= HandleDistanceTravelledChanged;
        playerScoreEventChannel.OnEventRaised -= HandlePlayerScoreChange;
    }

    // Handle pitch change event
    private void HandlePitchChanged(float pitch)
    {
        textMeshProPitch.text = $"{pitch}";
    }

    // Handle roll change event
    private void HandleRollChanged(float roll)
    {
        textMeshProRoll.text = $"{roll}";
    }

    // Handle yaw change event
    private void HandleYawChanged(float yaw)
    {
        textMeshProYaw.text = $"{yaw}";
    }

    // Handle speed change event
    private void HandleSpeedChanged(float speed)
    {
        // Convert speed from m/s to km/h
        textMeshProSpeed.text = $"{((speed * 0.001f) * 3600).ToString("F2")} km/h";
    }

    // Handle missile numbers change event
    private void HandleMissileNumbersChanged(int missilesNumber)
    {
        textMeshProMissilesNumber.text = $"× {missilesNumber}";
    }

    // Handle player score change event
    private void HandlePlayerScoreChange(int score)
    {
        textMeshProScore.text = $"{score.ToString()}";
    }

    // Handle distance travelled change event
    private void HandleDistanceTravelledChanged(float distanceTravelled)
    {
        textMeshProDistanceTravelled.text = $"{distanceTravelled.ToString("F5")} l.y.";
    
        switch (distanceTravelled)
        {
            case float d when (d >= 0 && d <= 0.00010):
                imagePanelCuriosity.gameObject.SetActive(true);
                textMeshProCuriosity.text = $"You are near Neptune!\nCuriosity: Neptune has supersonic winds and it rains diamonds!";
                break;
            
            case float d when (d >= 0.00020 && d <= 0.00035):
                imagePanelCuriosity.gameObject.SetActive(true);
                textMeshProCuriosity.text = $"You are near Uranus!\nCuriosity: Uranus also has diamond rain!";
                break;
    
            case float d when (d >= 0.00045 && d <= 0.00065):
                imagePanelCuriosity.gameObject.SetActive(true);
                textMeshProCuriosity.text = $"You are near Saturn!\nCuriosity: Saturn has a stunning ring system and could float in water!";
                break;
    
            case float d when (d >= 0.00070 && d <= 0.00100):
                imagePanelCuriosity.gameObject.SetActive(true);
                textMeshProCuriosity.text = $"You are near Jupiter!\nCuriosity: Jupiter is the largest planet and has a Great Red Spot storm!";
                break;
    
            case float d when (d >= 0.00138 && d <= 0.00140):
                imagePanelCuriosity.gameObject.SetActive(true);
                textMeshProCuriosity.text = $"You are near Mars!\nCuriosity: Mars has the tallest volcano and the deepest canyon in the solar system!";
                break;
    
            case float d when (d >= 0.00141 && d <= 0.00142):
                imagePanelCuriosity.gameObject.SetActive(true);
                textMeshProCuriosity.text = $"You are near Earth!\nCuriosity: Earth is the only planet known to support life and has liquid water!";
                break;
    
            case float d when (d >= 0.00143 && d <= 0.00144):
                imagePanelCuriosity.gameObject.SetActive(true);
                textMeshProCuriosity.text = $"You are near Venus!\nCuriosity: Venus has a runaway greenhouse effect and is hotter than Mercury!";
                break;
    
            case float d when (d >= 0.00145 && d <= 0.00147):
                imagePanelCuriosity.gameObject.SetActive(true);
                textMeshProCuriosity.text = $"You are near Mercury!\nCuriosity: Mercury has extreme temperature fluctuations and no atmosphere!";
                break;
    
            case float d when (d >= 0.00155 && d <= 0.00210):
                imagePanelCuriosity.gameObject.SetActive(true);
                textMeshProCuriosity.text = $"You are near the Sun!\nCuriosity: The Sun is a massive ball of burning gas and is 109 times the diameter of Earth!";
                break;
            
            default:
                imagePanelCuriosity.gameObject.SetActive(false);
                textMeshProCuriosity.text = $""; // Clear the text
                break;
        }
    }
}