using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class FirstButtonMainCanvasController : MonoBehaviour
{
    [SerializeField] private Button mainMenuFirstButton;
    [SerializeField] private Button settingsFirstButton;
    [SerializeField] private Button hangarFirstButton;

    private Button firstButton;

    private void Start()
    {
        firstButton = mainMenuFirstButton;
        EventSystem.current.SetSelectedGameObject(mainMenuFirstButton.gameObject);
    }

    private void Update()
    {
        if (EventSystem.current.currentSelectedGameObject == null)
        {
            EventSystem.current.SetSelectedGameObject(firstButton.gameObject);
        }
    }

    public void SetSettingsFirstSelectedButton()
    {
        firstButton = settingsFirstButton;
        EventSystem.current.SetSelectedGameObject(settingsFirstButton.gameObject);
    }

    public void SetMainMenuFirstSelectedButton()
    {
        firstButton = mainMenuFirstButton;
        EventSystem.current.SetSelectedGameObject(mainMenuFirstButton.gameObject);
    }

    public void SetHangarFirstSelectedButton()
    {
        firstButton = hangarFirstButton;
        EventSystem.current.SetSelectedGameObject(hangarFirstButton.gameObject);
    }
}