using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class FirstButtonGameOverCanvasController : MonoBehaviour
{
    [SerializeField] private Button mainnMenuButton;

    private void OnEnable()
    {
        EventSystem.current.SetSelectedGameObject(mainnMenuButton.gameObject);
    }

    private void Update()
    {
        if (EventSystem.current.currentSelectedGameObject == null)
        {
            EventSystem.current.SetSelectedGameObject(mainnMenuButton.gameObject);
        }
    }
}