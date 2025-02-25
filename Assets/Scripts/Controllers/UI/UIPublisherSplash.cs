using UnityEngine;

public class UIPublisherSplash : MonoBehaviour
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
        }
    }
}
