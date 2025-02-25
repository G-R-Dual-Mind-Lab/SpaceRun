using UnityEngine;

public class UIPublisherPause : MonoBehaviour
{
    [SerializeField] private UIMessageEventChannelSO uiEventChannel; // canale per comunicare "bottoni" premuti su UI (sorgente UI)

    void Start()
    {
        if (uiEventChannel == null)
        {
            Debug.LogError("One or more channels are null in UIPublisher Start");
        }
    }

    public void ManageUIEvent(string message)
    {
        switch (message)
        {
            case "GoToMainMenu":
                uiEventChannel.RaiseEvent(UIMessage.GoToMainMenu);
                break;

            case "StartGame":
                uiEventChannel.RaiseEvent(UIMessage.StartGame);
                break;

            case "GoToTrainingZone":
                uiEventChannel.RaiseEvent(UIMessage.GoToTrainingZone);
                break;
        }
    }
}
