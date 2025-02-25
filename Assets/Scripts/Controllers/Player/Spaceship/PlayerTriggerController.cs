using UnityEngine;
using Helper;

public class PlayerTriggerController : MonoBehaviour
{
    private PlayerStatusController playerStatusController;
    private PlayerDataController playerDataController;
    private RevisedGun revisedGun;

    //////////////////////////////// LIFECYCLE METHODS ////////////////////////////////

    private void Awake()
    {
        playerStatusController = GetComponent<PlayerStatusController>();

        if (playerStatusController == null)
        {
            Debug.LogError("playerStatusController is null in PlayerTriggerController Awake");
        }

        playerDataController = GetComponent<PlayerDataController>();

        if (playerDataController == null)
        {
            Debug.LogError("playerDataController is null in PlayerTriggerController Awake");
        }

        revisedGun = GetComponent<RevisedGun>();

        if (revisedGun == null)
        {
            Debug.LogError("revisedGun is null in PlayerTriggerController Awake");
        }
    }

    //////////////////////////////// CLASS METHODS ////////////////////////////////

    private void OnTriggerEnter(Collider other)
    {
        switch (other.gameObject.tag)
        {
            case TagsHelper.SafeVolume:
                // Player is in a safe volume, disable danger and shaking status
                playerStatusController.IsInDanger = false;
                playerStatusController.IsShaking = false;
                break;

            case TagsHelper.Boost:
                // Player has collected a boost, enable boosting status
                playerStatusController.IsBoosting = true;
                break;

            case TagsHelper.Missile:
                // Check if the missile was fired
                if (other.GetComponent<RevisedMissile>().Fired)
                {
                    // If fired, player is dead
                    playerStatusController.IsDeath = true;
                }
                else
                {
                    // If not fired, increase the missile count
                    playerDataController.MissilesNumber = revisedGun.MissileNumber;
                }
                break;

            case TagsHelper.BlackHoleDanger:
                // Player is in the danger zone of the black hole
                playerStatusController.IsInDanger = true;
                break;

            case TagsHelper.BlackHoleCore:
                // Player has reached the core of the black hole, player is dead
                playerStatusController.IsDeath = true;
                break;
        }
    }

    private void OnTriggerExit(Collider other)
    {
        switch (other.gameObject.tag)
        {
            case TagsHelper.SafeVolume:
                // Player has exited the safe volume, enable danger and shaking status
                playerStatusController.IsInDanger = true;
                playerStatusController.IsShaking = true;
                break;

            case TagsHelper.UnsafeVolume:
                // Player has exited the unsafe volume, player is dead and disable shaking status
                playerStatusController.IsDeath = true;
                playerStatusController.IsShaking = false;
                break;
            
            case TagsHelper.BlackHoleDanger:
                // Player has exited the danger zone of the black hole
                playerStatusController.IsInDanger = false;
                break;
        }
    }
}