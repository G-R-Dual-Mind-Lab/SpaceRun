using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class OcclusionMaskController : MonoBehaviour
{
    [SerializeField] private BooleanEventChannelSO isInDangerEventChannel; // Event channel for danger status updates

    public Image darkOverlayUp; // UI Image for the upper dark overlay
    public Image darkOverlayDown; // UI Image for the lower dark overlay
    public Image darkOverlayLeft; // UI Image for the left dark overlay
    public Image darkOverlayRight; // UI Image for the right dark overlay

    private Coroutine fadeCoroutine; // Coroutine for fading the overlays
    private bool canRestoreVision; // Flag to check if vision can be restored

    private void Awake()
    {
        if (isInDangerEventChannel == null)
        {
            Debug.LogError("isInDangerEventChannel is null in OcclusionMaskController");
        }
    }

    private void OnEnable()
    {
        // Initialize overlay colors to transparent
        darkOverlayUp.color = new Color(0, 0, 0, 0);
        darkOverlayDown.color = new Color(0, 0, 0, 0);
        darkOverlayLeft.color = new Color(0, 0, 0, 0);
        darkOverlayRight.color = new Color(0, 0, 0, 0);

        canRestoreVision = false;

        // Subscribe to the danger status event
        isInDangerEventChannel.OnEventRaised += HandleShipStatusChanged;
    }

    private void OnDisable()
    {
        // Unsubscribe from the danger status event
        isInDangerEventChannel.OnEventRaised -= HandleShipStatusChanged;
    }

    // Handle the ship's danger status change
    private void HandleShipStatusChanged(bool isInDanger)
    {
        if (isInDanger)
        {
            if (fadeCoroutine != null) StopCoroutine(fadeCoroutine);
            fadeCoroutine = StartCoroutine(DarkenVision());
        }
        else
        {
            if (fadeCoroutine != null) StopCoroutine(fadeCoroutine);
            fadeCoroutine = StartCoroutine(RestoreVision());
        }
    }

    // Coroutine to darken the vision
    private IEnumerator DarkenVision()
    {
        float t = 0;
        canRestoreVision = true;
        while (t < 1)
        {
            t += Time.deltaTime;
            darkOverlayUp.color = new Color(0, 0, 0, Mathf.Lerp(0, 1, t)); // Gradually darken
            darkOverlayDown.color = new Color(0, 0, 0, Mathf.Lerp(0, 1, t)); // Gradually darken
            darkOverlayLeft.color = new Color(0, 0, 0, Mathf.Lerp(0, 1, t)); // Gradually darken
            darkOverlayRight.color = new Color(0, 0, 0, Mathf.Lerp(0, 1, t)); // Gradually darken
            yield return null;
        }
    }

    // Coroutine to restore the vision
    private IEnumerator RestoreVision()
    {
        if (canRestoreVision)
        {
            float t = 0;
            while (t < 1)
            {
                t += Time.deltaTime;
                darkOverlayUp.color = new Color(0, 0, 0, Mathf.Lerp(1, 0, t)); // Gradually restore vision
                darkOverlayDown.color = new Color(0, 0, 0, Mathf.Lerp(1, 0, t)); // Gradually restore vision
                darkOverlayLeft.color = new Color(0, 0, 0, Mathf.Lerp(1, 0, t)); // Gradually restore vision
                darkOverlayRight.color = new Color(0, 0, 0, Mathf.Lerp(1, 0, t)); // Gradually restore vision
                yield return null;
            }
        }  
    }
}