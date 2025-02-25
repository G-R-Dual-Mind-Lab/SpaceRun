using UnityEngine;
using Helper;

public class SpawnerSensorController : MonoBehaviour
{  
    [SerializeField] private BonusSpawnerController bonusSpawnerController; // Reference to the BonusSpawnerController component.
    
    private SpawnerController spawnerController; // Reference to the SpawnerController component.

    private void Awake()
    {
        // Get the SpawnerController component from the root of the hierarchy.
        spawnerController = transform.root.GetComponent<SpawnerController>();

        if (spawnerController == null)
        {
            Debug.LogError("spawnerController is null in SpawnerSensorController Awake");
        }

        if (bonusSpawnerController == null)
        {
            Debug.LogError("bonusSpawnerController is null in SpawnerSensorController Awake");
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        // Check if the object that entered the trigger is the player.
        if (other.gameObject.CompareTag(TagsHelper.Player))
        {
            // Translate the spawner to a new position.
            spawnerController.TranslateSpawner();

            // Spawn a random bonus at the player's position.
            bonusSpawnerController.SpawnRandomBonus(other.gameObject.transform.position);
        }
    }
}