using UnityEngine;
using UnityEngine.InputSystem;
using System.IO;
using System.Collections.Generic;

[System.Serializable]
public class ChannelData
{
    public string channelName;
    public List<string> trackNames;
}

[System.Serializable]
public class RadioTracksData
{
    public List<ChannelData> channels;
}

public class PlayerRadioController : MonoBehaviour
{
    // Paths
    private const string radioFolderPath = "AudioClips/Radio";
    private const string radioDataPath = "Data/RadioTracksData";

    [Header("Event Channels")]
    [SerializeField] private BooleanEventChannelSO radioStatusEventChannel;
    [SerializeField] private AudioClipEventChannelSO reproduceRadioClipEventChannel;

    // Dirty Flag: Radio Status
    private bool isRadioON;

    // Other Variables
    private int currentChannel;
    private int channelsNumber;
    private AudioClip currentClip;
    private RadioTracksData data;

    // Necessary for obtaining random values
    private System.Random random = new System.Random();

    // Properties
    private bool IsRadioON
    {
        get => isRadioON;

        set
        {
            if (isRadioON == value)
                return;

            if (value)
            {
                reproduceRadioClipEventChannel.RaiseEvent(GetRandomAudioClipByChannel(currentChannel)); // The first clip to play
            }

            isRadioON = value;
            radioStatusEventChannel.RaiseEvent(isRadioON);
        }
    }

    private int CurrentChannel
    {
        get => currentChannel;

        set
        {
            if (currentChannel == value)
                return;

            currentChannel = value;
            reproduceRadioClipEventChannel.RaiseEvent(GetRandomAudioClipByChannel(currentChannel));
        }
    }

    //////////////////////////////// LIFECYCLE METHODS ////////////////////////////////

    private void Awake()
    {
        if (radioStatusEventChannel == null || reproduceRadioClipEventChannel == null)
        {
            Debug.LogError("One or more channels are null in PlayerRadioController");
        }

        InitializeRadioData();

        isRadioON = false;
    }

    private void OnEnable()
    {
        currentChannel = 0;
    }

    private void OnDisable()
    {
        IsRadioON = false;
    }

    //////////////////////////////// CLASS METHODS ////////////////////////////////

    // Initialize radio data by loading the JSON file from the Resources folder
    private void InitializeRadioData()
    {
        TextAsset jsonFile = Resources.Load<TextAsset>(radioDataPath);
        if (jsonFile == null)
        {
            Debug.LogError($"JSON index file not found in {radioDataPath}");
            return;
        }

        // Convert the JSON to a C# object
        data = JsonUtility.FromJson<RadioTracksData>(jsonFile.text);
        if (data == null || data.channels == null || data.channels.Count == 0)
        {
            Debug.LogError($"Failed to parse JSON or no channels found in {radioDataPath}");
            return;
        }

        channelsNumber = data.channels.Count; // Number of channels
    }

    // Get a random audio clip from the specified channel
    private AudioClip GetRandomAudioClipByChannel(int currentChannel)
    {
        Resources.UnloadAsset(currentClip);
        string channelName = data.channels[currentChannel].channelName;
        int index = random.Next(data.channels[currentChannel].trackNames.Count);
        string trackName = data.channels[currentChannel].trackNames[index];
        currentClip = (AudioClip)Resources.Load(Path.Combine(radioFolderPath, channelName, trackName));
        return currentClip;
    }

    // Toggle the radio on or off
    public void ToggleRadio(InputAction.CallbackContext context)
    {
        if (context.performed)
        {
            IsRadioON = !IsRadioON;
        }  
    }

    // Switch to the next radio channel
    public void NextChannel(InputAction.CallbackContext context)
    {
        if (context.performed)
        {
            if (IsRadioON) // If the radio is on
            {
                CurrentChannel = (CurrentChannel == channelsNumber - 1) ? 0 : CurrentChannel + 1;
            }
        }     
    }

    // Switch to the previous radio channel
    public void PreviousChannel(InputAction.CallbackContext context)
    {
        if (context.performed)
        {
            if (IsRadioON) // If the radio is on
            {
                CurrentChannel = (CurrentChannel == 0) ? channelsNumber - 1 : CurrentChannel - 1;
            }
        }
    }
}