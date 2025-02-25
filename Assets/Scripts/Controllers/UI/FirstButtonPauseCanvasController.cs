using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class FirstButtonPauseCanvasController : MonoBehaviour
{
    [SerializeField] private Button continueButton;

    private void OnEnable()
    {
        EventSystem.current.SetSelectedGameObject(continueButton.gameObject);
    }

    private void Update()
    {
        if (EventSystem.current.currentSelectedGameObject == null)
        {
            EventSystem.current.SetSelectedGameObject(continueButton.gameObject);
        }
    }
}