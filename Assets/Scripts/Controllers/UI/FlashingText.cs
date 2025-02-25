using UnityEngine;
using System.Collections;
using TMPro;

public class FlashingText : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI pressAnyButtonText; // Riferimento al componente Text
    [SerializeField] private float flashDuration = 0.5f; // Durata del flash

    private void Start()
    {
        StartCoroutine(FlashingCoroutine());
    }

    private IEnumerator FlashingCoroutine()
    {
        while (true)
        {
            pressAnyButtonText.gameObject.SetActive(true); // Mostra il testo
            yield return new WaitForSeconds(flashDuration); // Attendi per un intervallo di tempo
            pressAnyButtonText.gameObject.SetActive(false); // Nascondi il testo
            yield return new WaitForSeconds(flashDuration); // Attendi di nuovo
        }
    }
}