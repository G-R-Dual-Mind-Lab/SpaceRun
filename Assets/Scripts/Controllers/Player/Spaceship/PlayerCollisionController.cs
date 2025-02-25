using System.Collections;
using Helper;
using UnityEngine;

public class PlayerCollisionController : MonoBehaviour
{
    private PlayerStatusController playerStatusController;
    private bool isVulnerable;

    //////////////////////////////// LIFECYCLE METHODS ////////////////////////////////

    private void Awake()
    {
        playerStatusController = GetComponent<PlayerStatusController>();

        if (playerStatusController == null)
        {
            Debug.LogError("playerStatusController is null in PlayerCollisionController Awake");
        }
    }

    private void OnEnable()
    {
        isVulnerable = false;
    }

    //////////////////////////////// CLASS METHODS ////////////////////////////////

    private void OnCollisionEnter(Collision collision)
    {
        switch (collision.gameObject.tag)
        {
            case TagsHelper.Asteroid:
                if (!isVulnerable)
                {
                    isVulnerable = true;
                    StartCoroutine(ResetVulnerableFlagAfterDelay(3f)); // If the player hits another asteroid within 3 seconds, the spaceship is destroyed.
                }
                else
                {
                    playerStatusController.IsDeath = true;
                }
                break;

            case TagsHelper.Planet:
                playerStatusController.IsDeath = true;
                break;

            case TagsHelper.Mine:
                playerStatusController.IsDeath = true;
                break;

            case TagsHelper.Satellite:
                playerStatusController.IsDeath = true;
                break;

            case TagsHelper.Shipwreck:
                // No action needed for shipwreck collision.
                break;
        }
    }

    // Coroutine to reset the vulnerable flag after a delay.
    private IEnumerator ResetVulnerableFlagAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        isVulnerable = false;
    }
}