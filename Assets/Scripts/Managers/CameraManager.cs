using System;
using System.Collections.Generic;
using UnityEngine;

public class CameraManager : Singleton<CameraManager>, IManager
{
    private GameStateEventChannelSO gsEventChannel; // Channel for game state change communication (source: GameManager)
    private UIMessageEventChannelSO uiEventChannel; // Channel for button press communication in the menu (source: UIManager)

    private Camera[] cameras; // Array of cameras present in the scene;
    private bool channelsConfigured = false;


    //////////////////////////////// INTERFACE METHODS ////////////////////////////////

    public void Initialize() {} // IManager interface method

    public void RegisterDelegates()
    {
        gsEventChannel.OnEventRaised += HandleGameStateChanged;
        uiEventChannel.OnEventRaised += HandleUIEvent;
    }

    public void ConfigureChannels(Dictionary<Type, ScriptableObject> channels)
    {
        var channelMappings = new (Type type, Action<ScriptableObject> assign, string name)[]
        {
            (typeof(GameStateEventChannelSO), obj => gsEventChannel = (GameStateEventChannelSO)obj, "GameStateEventChannelSO"),
            (typeof(UIMessageEventChannelSO), obj => uiEventChannel = (UIMessageEventChannelSO)obj, "UIMessageEventChannelSO")
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

    void Awake()
    {
        // Automatically retrieve all cameras in the scene
        cameras = FindObjectsByType<Camera>(FindObjectsInactive.Include, FindObjectsSortMode.None);

        if (cameras.Length == 0)
        {
            Debug.LogError("No cameras found in the scene!");
            return;
        }
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
        gsEventChannel.OnEventRaised -= HandleGameStateChanged;
        uiEventChannel.OnEventRaised -= HandleUIEvent;
    }


    //////////////////////////////// CLASS METHODS ////////////////////////////////

    private GameObject FindCameraByTag(string tag)
    {
        // Disable all cameras except the main one
        for (int x = 0; x < cameras.Length; x++)
        {
            if (cameras[x].CompareTag(tag))
            {
                return cameras[x].gameObject;
            }
        }
        return null;
    }

    private void HandleUIEvent(UIMessage message)
    {
        switch (message)
        {
            case UIMessage.GoToSettings:
                FindCameraByTag("MainCamera").GetComponent<Animator>().SetFloat("Animate", 1);
                break;

            case UIMessage.GoToHangar:
                FindCameraByTag("MainCamera").GetComponent<Animator>().SetFloat("Animate", 2);
                break;

            case UIMessage.Return:
                FindCameraByTag("MainCamera").GetComponent<Animator>().SetFloat("Animate", 0);
                break;

            case UIMessage.Choose:
                FindCameraByTag("MainCamera").GetComponent<Animator>().SetFloat("Animate", 0);
                break;
        }
    }

    private void HandleGameStateChanged(GameState state)
    {
        switch (state)
        {
            case GameState.MainMenu:
                FindCameraByTag("MainCamera").GetComponent<CameraFollowController>().enabled = false;
                FindCameraByTag("MainCamera").GetComponent<Animator>().enabled = true;
                break;

            case GameState.Playing:
                FindCameraByTag("MainCamera").GetComponent<CameraFollowController>().enabled = true;
                FindCameraByTag("MainCamera").GetComponent<Animator>().enabled = false;
                break;

            case GameState.Tutorial:
                FindCameraByTag("MainCamera").GetComponent<CameraFollowController>().enabled = true;
                FindCameraByTag("MainCamera").GetComponent<Animator>().enabled = false;
                break;
        }
    }
}