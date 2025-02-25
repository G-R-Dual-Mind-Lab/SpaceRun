using UnityEngine;
using UnityEngine.Pool;
using Helper;

public class RevisedGun : MonoBehaviour
{
    [Header("Pool Properties")]
    [SerializeField] private bool collectionCheck = true; // If true, throws an exception if an object that is already in the pool is added to the pool again.

    [Header("Missile Prefab")]
    [SerializeField] private RevisedMissile missilePrefab; // Prefab for the missile.

    [Header("Missile Properties")]
    [Range(0, 20)] [SerializeField] private float offsetDistance; // Distance from the spaceship to spawn the missile.
    [Range(0, 1000)][SerializeField] protected float missileSpeed; // Speed of the missile.

    // Game Object Components
    private PlayerDataController playerDataController;
    private PlayerStatusController playerStatusController;
    private Rigidbody rbSpaceship;

    // Missile Component
    private Rigidbody rbMissile;

    // Pool
    private IObjectPool<RevisedMissile> missilePoolSpaceship;

    // Properties
    public IObjectPool<RevisedMissile> MissilePoolSpaceShip
    {
        get => missilePoolSpaceship;
    }

    public int GetAvailableMissilesNumber
    {
        get => MissilePoolSpaceShip.CountInactive;
    }

    public int MissileNumber
    {
        get => missilePoolSpaceship.CountInactive; // Returns the number of available missiles.
    }


    //////////////////////////////// LIFECYCLE METHODS ////////////////////////////////

    private void Awake()
    {
        missilePoolSpaceship = new ObjectPool<RevisedMissile>(
            CreateMissile, 
            OnGetFromPool, 
            OnReleaseToPool, 
            OnDestroyPooledObject, 
            collectionCheck, 
            0, // defaultCapacity
            GlobalConstants.PlayerMissileLimit // maximum size
        );

        playerDataController = GetComponent<PlayerDataController>();

        if (playerDataController == null)
        {
            Debug.LogError("playerDataController is null in RevisedGun Awake");
        }

        playerStatusController = GetComponent<PlayerStatusController>();

        if (playerStatusController == null)
        {
            Debug.LogError("playerStatusController is null in RevisedGun Awake");
        }

        rbSpaceship = gameObject.GetComponent<Rigidbody>();

        if (rbSpaceship == null)
        {
            Debug.LogError("rbSpaceship is null in RevisedGun Awake");
        }
    }


    //////////////////////////////// CLASS METHODS ////////////////////////////////

    // Invoked when creating an item to populate the object pool
    private RevisedMissile CreateMissile()
    {
        RevisedMissile missileInstance = Instantiate(missilePrefab, transform.position, Quaternion.identity);
        missileInstance.SpaceShipPool = missilePoolSpaceship;
        missileInstance.SpawnerPool = null;
        return missileInstance;
    }

    // Invoked when returning an item to the object pool
    private void OnReleaseToPool(RevisedMissile pooledObject)
    {
        pooledObject.gameObject.SetActive(false);
        pooledObject.currentPool = MissilePoolType.SpaceShipPool;
    }

    // Invoked when retrieving the next item from the object pool
    private void OnGetFromPool(RevisedMissile pooledObject)
    {
        pooledObject.gameObject.transform.localScale = Vector3.one;

        rbMissile = pooledObject.GetComponent<Rigidbody>(); // Get the Rigidbody component of the missile
        
        // Set position and rotation of the missile
        rbMissile.transform.position = transform.position + transform.forward * offsetDistance; // Set the spawn position of the missile
        rbMissile.transform.rotation = transform.rotation;  // Set the spawn rotation of the missile
        rbMissile.linearVelocity = rbSpaceship.linearVelocity + transform.forward * missileSpeed; // Assign velocity to the Rigidbody of the missile

        pooledObject.GetComponent<BoxCollider>().enabled = true;
        pooledObject.GetComponent<SphereCollider>().enabled = false;

        pooledObject.Fired = true;
        pooledObject.gameObject.SetActive(true); // Activate the missile game object

        pooledObject.currentPool = MissilePoolType.None;
    }

    // Invoked when we exceed the maximum number of pooled items (i.e., destroy the pooled object)
    private void OnDestroyPooledObject(RevisedMissile pooledObject)
    {
        Destroy(pooledObject.gameObject);
    }

    // Fire a missile
    public void Fire()
    {
        missilePoolSpaceship.Get();
        playerDataController.MissilesNumber = missilePoolSpaceship.CountInactive;
    }
}