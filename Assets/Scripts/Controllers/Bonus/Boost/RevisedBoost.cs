using Helper;
using UnityEngine;
using UnityEngine.Pool;

public class RevisedBoost : MonoBehaviour
{
    // Reference to the object pool that manages instances of RevisedBoost.
    private IObjectPool<RevisedBoost> spawnerPool;

    // Property to get or set the spawner pool.
    public IObjectPool<RevisedBoost> SpawnerPool
    {
        get => spawnerPool;
        set => spawnerPool = value;
    }

    // OnTriggerEnter is called when the Collider other enters the trigger.
    private void OnTriggerEnter(Collider other)
    {
        // Check the tag of the other game object.
        switch (other.gameObject.tag)
        {
            // If the other object is the player, release this object back into the pool.
            case TagsHelper.Player:
                spawnerPool.Release(this);
                break;
            
            // If the other object is the black hole core, release this object back into the pool.
            case TagsHelper.BlackHoleCore:
                spawnerPool.Release(this);
                break;
        }
    }
}