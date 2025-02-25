using UnityEngine.Pool;
using UnityEngine;
using Helper;

public enum MissilePoolType
{
    None,
    SpawnerPool,
    SpaceShipPool
}

public class RevisedMissile : MonoBehaviour
{
    [Header("Event channels")]
    [SerializeField] private GameObjectEventChannelSO missileHitEventChannelSO;

    // Pools
    private IObjectPool<RevisedMissile> spaceShipPool; // Pool for missiles collected by the player.
    private IObjectPool<RevisedMissile> spawnerPool; // Pool for spawning missiles.
    public MissilePoolType currentPool = MissilePoolType.None;

    // ParticleSystem
    private ParticleSystem particleSystemGlow; // Particle system for the glow effect.

    // Dirty Flags
    private bool fired; // Indicates if the missile has been fired by the player.

    // Other Variables
    private RevisedSpaceObject revisedSpaceObject;

    // Pools Properties
    public IObjectPool<RevisedMissile> SpaceShipPool
    {
        get => spaceShipPool;
        set => spaceShipPool = value;
    }

    public IObjectPool<RevisedMissile> SpawnerPool
    {
        get => spawnerPool;
        set => spawnerPool = value;
    }

    public bool Fired
    {
        get => fired;
        set => fired = value;
    }

    // Awake is called when the script instance is being loaded.
    private void Awake()
    {
        if (missileHitEventChannelSO == null)
        {
            Debug.LogError("missileHitEventChannelSO is null in RevisedMissile");
        }
    }

    // OnEnable is called when the object becomes enabled and active.
    private void OnEnable()
    {
        particleSystemGlow = GetComponentInChildren<ParticleSystem>();
        
        if (particleSystemGlow != null)
        {
            if (fired)
            { 
                // Pause the glow effect when the missile is fired.
                particleSystemGlow.Pause();
            }
            else
            {
                // Play the glow effect when the missile is a collectible bonus.
                particleSystemGlow.Play();
            }
        }
    }

    // OnDisable is called when the behaviour becomes disabled or inactive.
    private void OnDisable()
    {
        if (particleSystemGlow != null)
        {
            particleSystemGlow.Pause();
        }
    }

    // Check if the player's missile pool has already been initialized (i.e., if it has been collected previously).
    private void CheckSpaceShipPool(IObjectPool<RevisedMissile> pool)
    {
        if (spaceShipPool == null)
        {
            spaceShipPool = pool;
        }
    }

    /*
     *  Raise an event on the missileHitEventChannelSO channel to communicate the hit object to the Effect Manager.
     *  Release this object back into the spawner pool.
     *  Release the hit space object back into its respective pool.
     */
    private void HandleHitSpaceObject(Collider other)
    {
        missileHitEventChannelSO.RaiseEvent(other.gameObject);
        spawnerPool.Release(this);
        revisedSpaceObject = other.GetComponent<RevisedSpaceObject>();
        revisedSpaceObject.ObjectPool.Release(revisedSpaceObject);
    }

    // OnTriggerEnter is called when the Collider other enters the trigger.
    private void OnTriggerEnter(Collider other)
    {
        switch (other.gameObject.tag)
        {
            case TagsHelper.Asteroid:
                if (fired)
                {
                    HandleHitSpaceObject(other);
                }
                break;

            case TagsHelper.Planet:
                if(fired)
                {
                    HandleHitSpaceObject(other);
                }
                break;

            case TagsHelper.Mine:
                if (fired)
                {
                    HandleHitSpaceObject(other);
                }
                break;

            case TagsHelper.Satellite:
                if (fired)
                {
                    HandleHitSpaceObject(other);
                }
                break;

            case TagsHelper.Shipwreck:
                if (fired)
                {
                    HandleHitSpaceObject(other);
                }
                break;

            case TagsHelper.Player:
                // Check if the player's missile pool has been initialized.
                CheckSpaceShipPool(other.transform.parent.gameObject.GetComponent<RevisedGun>().MissilePoolSpaceShip);
                if (spaceShipPool.CountInactive == GlobalConstants.PlayerMissileLimit || Fired)
                {
                    // Release the missile back into the spawner pool if the player has reached the missile limit or if it has been fired.
                    spawnerPool.Release(this);
                }
                else
                {
                    // Release the missile back into the player's pool.
                    spaceShipPool.Release(this);
                }
                break;
            
            case TagsHelper.BlackHoleCore:
                // Release the missile back into the spawner pool if it hits the black hole core.
                spawnerPool.Release(this);
                break;

            case TagsHelper.Bound:
                // Release the missile back into the spawner pool if it hits a boundary.
                spawnerPool.Release(this);
                break;
        }
    }
}
