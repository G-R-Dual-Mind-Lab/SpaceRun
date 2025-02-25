using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections;
using System.Collections.Generic;

public class HUDTutorialController : MonoBehaviour
{
    [Header("TextMeshPro elements")]
    [SerializeField] private TextMeshProUGUI textMeshProRoll;
    [SerializeField] private TextMeshProUGUI textMeshProPitch;
    [SerializeField] private TextMeshProUGUI textMeshProYaw;
    [SerializeField] private TextMeshProUGUI textMeshProSpeed;
    [SerializeField] private TextMeshProUGUI textMeshProMissilesNumber;
    [SerializeField] private TextMeshProUGUI textMeshProTips;

    [Header("Channels")]
    [SerializeField] private FloatEventChannelSO pitchChangedEventChannel;
    [SerializeField] private FloatEventChannelSO rollChangedEventChannel;
    [SerializeField] private FloatEventChannelSO yawChangedEventChannel;
    [SerializeField] private FloatEventChannelSO speedChangedEventChannel;
    [SerializeField] private IntEventChannelSO missilesChangedEventChannel;
    [SerializeField] private BooleanEventChannelSO isAcceleratingEventChannel;
    [SerializeField] private BooleanEventChannelSO isBoostingEventChannel;
    [SerializeField] private BooleanEventChannelSO isFiringEventChannel;
    [SerializeField] private BooleanEventChannelSO isPitchingEventChannel;
    [SerializeField] private BooleanEventChannelSO isYawingEventChannel;
    [SerializeField] private BooleanEventChannelSO isRollingEventChannel;
    [SerializeField] private StringEventChannelSO spawnBonusEventChannel;

    // Message constants
    private const string MessagePitchAndYawKeyboard = "Use W A S D to manoeuvre your ship.\nPrecision and control are crucial in space!";
    private const string MessagePitchAndYawGamepad = "Use the left analogue stick to manoeuvre your ship.\nPrecision and control are crucial in space!";

    private const string MessageAccelerationKeyboard = "Press the Space Bar to accelerate.\nPush those engines to the max!";
    private const string MessageAccelerationGamepad = "Press the right trigger to accelerate.\nPush those engines to the max!";

    private const string MessageRollKeyboard = "Use the Left and Right Arrow to roll.\nYou'll need it to dodge obstacles!";
    private const string MessageRollGamepad = "Move the right stick to roll.\nYou'll need it to dodge obstacles!";

    private const string MessageFireKeyboard = "As you go on in the mission, you may find power-ups, try taking the missile and firing it pressing F!";
    private const string MessageFireGamepad = "As you go on in the mission, you may find power-ups, try taking the missile and firing it using the left trigger!";

    private const string MessageBoost = "You can also find a red boost, use it to increase your speed for a short period of time!";

    private const string MessageDetritus = "Watch out for space debris! If you get too close, it only takes a few seconds to turn you into stardust!";

    private const string MessageBlackHole = "This is just training, but in the real mission, the black hole will chase you. Keep moving, or you won't make it";

    private const string MessageRadioOnKeyboard = "Turn on the radio by pressing R.\nA bit of music makes the journey even more epic!";
    private const string MessageRadioOnGamepad = "Turn on the radio by pressing X.\nA bit of music makes the journey even more epic!";

    private const string MessageChangeChannelKeyboard = "Switch radio stations using Q and E.\nFind the perfect soundtrack for your flight!";
    private const string MessageChangeChannelGamepad = "Switch radio stations using LB and RB.\nFind the perfect soundtrack for your flight!";

    private const string MessageRadioOffKeyboard = "Turn off the radio by pressing R again.\nSometimes, the silence of space is the best soundtrack.";
    private const string MessageRadioOffGamepad = "Turn off the radio by pressing X again.\nSometimes, the silence of space is the best soundtrack.";

    private const string MessagePauseKeyboard = "Press Esc to pause your journey through the stars.\nTake a break, Pilot.";
    private const string MessagePauseGamepad = "Press Start to pause your mission.\nTake a break, Pilot.";

    private const string MessageTutorialCompleted = "You are now equipped for the mission ahead! Head back to the main menu and prepare for the ultimate challenge, Pilot.";

    private const string Yaw = "yaw";
    private const string Pitch = "pitch";
    private const string Acceleration = "acceleration";
    private const string Roll = "roll";
    private const string Fire = "fire";
    private const string Boost = "boost";
    private const string GamepadStr = "Gamepad";
    private const string KeyboardStr = "Keyboard";

    // List of tutorial Messages
    private List<string> coroutineKeyboardTutorialMessages;
    private List<string> coroutineGamepadTutorialMessages;

    // Dirty Variables
    private bool tutorialCompleted = true;
    private bool hasYawed = false;
    private bool hasPitched = false;
    private bool hasAccelerated = false;
    private bool hasRolled = false;
    private bool hasFired = false;
    private bool hasBoosted = false;
    private string activeCoroutineMessage = "";
    private string lastInputMethod = "Keyboard"; // Default to keyboard

    private void Awake()
    {
        if (pitchChangedEventChannel == null || 
            rollChangedEventChannel == null || 
            yawChangedEventChannel == null ||
            speedChangedEventChannel == null || 
            missilesChangedEventChannel == null ||
            isAcceleratingEventChannel == null ||
            isBoostingEventChannel == null ||
            isFiringEventChannel == null ||
            isPitchingEventChannel == null ||
            isYawingEventChannel == null ||
            isRollingEventChannel == null)
        {
            Debug.LogError("One or more channels are null in HUDController Awake");
        }
    }

    private void OnEnable()
    {
        // Initialize tutorial messages for keyboard and gamepad
        coroutineKeyboardTutorialMessages = new List<string> { MessageDetritus, MessageBlackHole, MessageRadioOnKeyboard, MessageChangeChannelKeyboard, MessageRadioOffKeyboard, MessagePauseKeyboard, MessageTutorialCompleted };
        coroutineGamepadTutorialMessages = new List<string> { MessageDetritus, MessageBlackHole, MessageRadioOnGamepad, MessageChangeChannelGamepad, MessageRadioOffGamepad, MessagePauseGamepad, MessageTutorialCompleted };

        // Subscribe to event channels
        pitchChangedEventChannel.OnEventRaised += HandlePitchChanged;
        rollChangedEventChannel.OnEventRaised += HandleRollChanged;
        yawChangedEventChannel.OnEventRaised += HandleYawChanged;
        speedChangedEventChannel.OnEventRaised += HandleSpeedChanged;
        missilesChangedEventChannel.OnEventRaised += HandleMissileNumbersChanged;
        isAcceleratingEventChannel.OnEventRaised += HandleIsAccelerating;
        isBoostingEventChannel.OnEventRaised += HandleIsBoosting;
        isPitchingEventChannel.OnEventRaised += HandleIsPitching;
        isYawingEventChannel.OnEventRaised += HandleIsYawing;
        isRollingEventChannel.OnEventRaised += HandleIsRolling;
        isFiringEventChannel.OnEventRaised += HandleIsFiring;

        DetectInputMethod();

        if (tutorialCompleted)
        {
            textMeshProTips.text = GetMessage(MessagePitchAndYawKeyboard, MessagePitchAndYawGamepad);
            activeCoroutineMessage = "";
            tutorialCompleted = false;
        }

        switch (activeCoroutineMessage)
        {
            case MessageDetritus:
                StartCoroutine(ShowMessageCoroutine(8f, MessageDetritus));
                break;

            case MessageBlackHole:
                StartCoroutine(ShowMessageCoroutine(8f, MessageBlackHole));
                break;

            case MessageRadioOnKeyboard:
            case MessageRadioOnGamepad:
                StartCoroutine(ShowMessageCoroutine(8f, GetMessage(MessageRadioOnKeyboard, MessageRadioOnGamepad)));
                break;
            
            case MessageChangeChannelKeyboard:
            case MessageChangeChannelGamepad:
                StartCoroutine(ShowMessageCoroutine(8f, GetMessage(MessageChangeChannelKeyboard, MessageChangeChannelGamepad)));
                break;

            case MessageRadioOffKeyboard:
            case MessageRadioOffGamepad:
                StartCoroutine(ShowMessageCoroutine(8f, GetMessage(MessageRadioOffKeyboard, MessageRadioOffGamepad)));
                break;
            
            case MessagePauseKeyboard:
            case MessagePauseGamepad:
                StartCoroutine(ShowMessageCoroutine(8f, GetMessage(MessagePauseKeyboard, MessagePauseGamepad)));
                break;

            case MessageTutorialCompleted:
                StartCoroutine(ShowMessageCoroutine(8f, MessageTutorialCompleted));
                break;
        }
    }

    private void OnDisable()
    {
        // Clear tutorial messages
        coroutineGamepadTutorialMessages.Clear();
        coroutineKeyboardTutorialMessages.Clear();

        // Unsubscribe from event channels
        pitchChangedEventChannel.OnEventRaised -= HandlePitchChanged;
        rollChangedEventChannel.OnEventRaised -= HandleRollChanged;
        yawChangedEventChannel.OnEventRaised -= HandleYawChanged;
        speedChangedEventChannel.OnEventRaised -= HandleSpeedChanged;
        missilesChangedEventChannel.OnEventRaised -= HandleMissileNumbersChanged;
        isAcceleratingEventChannel.OnEventRaised -= HandleIsAccelerating;
        isBoostingEventChannel.OnEventRaised -= HandleIsBoosting;
        isPitchingEventChannel.OnEventRaised -= HandleIsPitching;
        isYawingEventChannel.OnEventRaised -= HandleIsYawing;
        isRollingEventChannel.OnEventRaised -= HandleIsRolling;
        isFiringEventChannel.OnEventRaised -= HandleIsFiring;

        tutorialCompleted = false;
    }

    // Detect the input method (keyboard or gamepad)
    private void DetectInputMethod()
    {
        if (Gamepad.current != null && Gamepad.current.wasUpdatedThisFrame)
        {
            lastInputMethod = GamepadStr;
        }
        else if (Keyboard.current.wasUpdatedThisFrame)
        {
            lastInputMethod = KeyboardStr;
        }
    }

    // Get the appropriate message based on the input method
    private string GetMessage(string keyboardMessage, string gamepadMessage)
    {
        return lastInputMethod == GamepadStr ? gamepadMessage : keyboardMessage;
    }

    // Show the appropriate message based on the executed command
    private void ShowMessage(string executedCommand)
    {
        switch (executedCommand)
        {
            case Yaw:
            case Pitch:
                if (!hasYawed || !hasPitched)
                {
                    textMeshProTips.text = GetMessage(MessageAccelerationKeyboard, MessageAccelerationGamepad);
                    hasYawed = true;
                    hasPitched = true;
                }
                break;

            case Acceleration:
                if (!hasAccelerated && (hasYawed || hasPitched))
                {
                    textMeshProTips.text = GetMessage(MessageRollKeyboard, MessageRollGamepad);
                    hasAccelerated = true;
                }
                break;

            case Roll:
                if (!hasRolled && hasAccelerated)
                {
                    spawnBonusEventChannel.RaiseEvent(BonusTypes.Missile);
                    textMeshProTips.text = GetMessage(MessageFireKeyboard, MessageFireGamepad);
                    hasRolled = true;
                }
                break;

            case Fire:
                if (!hasFired && hasRolled)
                {
                    spawnBonusEventChannel.RaiseEvent(BonusTypes.Boost);
                    textMeshProTips.text = MessageBoost;
                    hasFired = true;
                }
                break;

            case Boost:
                if (!hasBoosted && hasFired)
                {
                    hasBoosted = true;
                    StartCoroutine(ShowMessageCoroutine(8f, MessageDetritus));
                }
                break;
        }
    }

    // Coroutine to show a message for a specified duration
    private IEnumerator ShowMessageCoroutine(float delay, string message)
    {
        activeCoroutineMessage = message;
        textMeshProTips.text = message;
        yield return new WaitForSeconds(delay);

        if (lastInputMethod == GamepadStr)
        {
            int index = coroutineGamepadTutorialMessages.IndexOf(message);
            if (index < coroutineGamepadTutorialMessages.Count - 1)
            {
                StartCoroutine(ShowMessageCoroutine(8f, coroutineGamepadTutorialMessages[index + 1]));
            }
            else
            {
                activeCoroutineMessage = "";
                tutorialCompleted = true;
            }
        }
        else
        {
            int index = coroutineKeyboardTutorialMessages.IndexOf(message);
            if (index < coroutineKeyboardTutorialMessages.Count - 1)
            {
                StartCoroutine(ShowMessageCoroutine(8f, coroutineKeyboardTutorialMessages[index + 1]));
            }
            else
            {
                activeCoroutineMessage = "";
                tutorialCompleted = true;
            }
        }
    }

    // Handle pitch change event
    private void HandlePitchChanged(float pitchValue)
    {
        textMeshProPitch.text = $"{pitchValue}";
    }

    // Handle roll change event
    private void HandleRollChanged(float rollValue)
    {
        textMeshProRoll.text = $"{rollValue}";
    }

    // Handle yaw change event
    private void HandleYawChanged(float yawValue)
    {
        textMeshProYaw.text = $"{yawValue}";
    }

    // Handle speed change event
    private void HandleSpeedChanged(float speedValue)
    {
        // Convert speed from m/s to km/h
        textMeshProSpeed.text = $"{((speedValue * 0.001f) * 3600).ToString("F2")} km/h";
    }

    // Handle missile numbers change event
    private void HandleMissileNumbersChanged(int missilesNumberValue)
    {
        textMeshProMissilesNumber.text = $"× {missilesNumberValue}";
    }

    // Handle acceleration event
    private void HandleIsAccelerating(bool isAccelerating)
    {
        ShowMessage(Acceleration);
    }

    // Handle boosting event
    private void HandleIsBoosting(bool isBoosting)
    {
        ShowMessage(Boost);
    }

    // Handle pitching event
    private void HandleIsPitching(bool isPitching)
    {
        ShowMessage(Pitch);
    }

    // Handle yawing event
    private void HandleIsYawing(bool isYawing)
    {
        ShowMessage(Yaw);
    }

    // Handle rolling event
    private void HandleIsRolling(bool isRolling)
    {
        ShowMessage(Roll);
    }

    // Handle firing event
    private void HandleIsFiring(bool isFiring)
    {
        ShowMessage(Fire);
    }
}