using UnityEngine;
using UnityEngine.Pool;
using Helper;
using System.Collections.Generic;
using System;

public static class BonusTypes
{
    public const string Missile = "Missile";
    public const string Boost = "Boost";
}

public class BonusSpawnerController : MonoBehaviour
{
    [Header("Bonus Prefab")]
    [SerializeField] GameObject missilePrefab; // Prefab for the missile bonus.
    [SerializeField] GameObject boostPrefab; // Prefab for the boost bonus.

    [Header("Event Channels")]
    [SerializeField] private TransformEventChannelSO playerTransformEventChannel; // Event channel for the player's Transform component.
    [SerializeField] private StringEventChannelSO spawnBonusEventChannel; // Event channel to trigger the spawning of a bonus.

    [Header("Spawning properties")]
    [Range(0, 1)] [SerializeField] private float bonusSpawningProbability; // Probability of spawning a bonus.
    [Range(0, 2000)] [SerializeField] private float offsetBonusSpawningPosition; // Offset for the spawning position of the bonus.
    [SerializeField] int defaultCapacityMissilePool = 10; // Default capacity of the missile pool.
    [SerializeField] int maxSizeMissilePool = 10; // Maximum size of the missile pool.
    [SerializeField] int defaultCapacityBoostPool = 10; // Default capacity of the boost pool.
    [SerializeField] int maxSizeBoostPool = 10; // Maximum size of the boost pool.

    // References to the bonus pools.
    private IObjectPool<RevisedMissile> missilePoolSpawner;
    private IObjectPool<RevisedBoost> boostPoolSpawner;

    // The position where the bonus will be spawned.
    private Vector3 spawnPosition;

    // Transform component of the player.
    private Transform playerTransform;

    // Number of bonus types available.
    private int bonusTypesNumber = 0;

    // Dictionary containing only the spawned bonuses that have not yet returned to the pool.
    private Dictionary<GameObject, Component> spawnedBonusComponents;

    private void Awake()
    {
        spawnedBonusComponents = new Dictionary<GameObject, Component>();

        if (playerTransformEventChannel == null || spawnBonusEventChannel == null)
        {
            Debug.LogError("One or more channels are null in BonusSpawnerController Awake");
        }
    }

    private void OnEnable()
    {
        // Subscribe to the event channels when the object is enabled.
        playerTransformEventChannel.OnEventRaised += HandlePlayerTransformEvent;
        spawnBonusEventChannel.OnEventRaised += HandleSpawnBonusEvent;
    }

    private void OnDisable()
    {
        // Unsubscribe from the event channels when the object is disabled.
        playerTransformEventChannel.OnEventRaised -= HandlePlayerTransformEvent;
        spawnBonusEventChannel.OnEventRaised -= HandleSpawnBonusEvent;
    }

    // Initialize the pools for the bonuses.
    public void PoolsFiller()
    {
        missilePoolSpawner = CreatePool<RevisedMissile>(TagsHelper.Missile, missilePrefab, defaultCapacityMissilePool, maxSizeMissilePool);
        boostPoolSpawner = CreatePool<RevisedBoost>(TagsHelper.Boost, boostPrefab, defaultCapacityBoostPool, maxSizeBoostPool);

        // Populate the missile pool.
        for (int i = 0; i < defaultCapacityMissilePool; i++)
        {
            var missile = CreateBonus<RevisedMissile>(TagsHelper.Missile, missilePrefab);
            missilePoolSpawner.Release(missile);
        }

        // Populate the boost pool.
        for (int i = 0; i < defaultCapacityBoostPool; i++)
        {
            var boost = CreateBonus<RevisedBoost>(TagsHelper.Boost, boostPrefab);
            boostPoolSpawner.Release(boost);
        }
    }

    // Create a pool for a specific bonus type.
    private IObjectPool<T> CreatePool<T>(string tag, GameObject prefab, int defaultCapacity, int maxSize) where T : Component
    {
        bonusTypesNumber++;

        return new ObjectPool<T>(
            () => CreateBonus<T>(tag, prefab),
            OnGetFromPool,
            OnReleaseToPool,
            OnDestroyPooledObject,
            collectionCheck: false,
            defaultCapacity,
            maxSize
        );
    }

    // Create a bonus of a specific type.
    private T CreateBonus<T>(string tag, GameObject prefab) where T : Component
    {
        GameObject instance = Instantiate(prefab, Vector3.zero, Quaternion.identity);
        T component = instance.GetComponent<T>();

        if (component is RevisedMissile missile)
        {
            missile.SpawnerPool = missilePoolSpawner as IObjectPool<RevisedMissile>;
            missile.SpaceShipPool = null; // Initially, we don't have a reference to the player's pool, which will be initialized when the player collects the missile.
        }
        else if (component is RevisedBoost boost)
        {
            boost.SpawnerPool = boostPoolSpawner as IObjectPool<RevisedBoost>;
        }

        return component;
    }

    // Release a bonus back to its pool.
    private void OnReleaseToPool<T>(T pooledObject) where T : Component
    {
        pooledObject.gameObject.SetActive(false);
        if (pooledObject is RevisedMissile missile)
        {
            missile.currentPool = MissilePoolType.SpawnerPool;
        }
        spawnedBonusComponents.Remove(pooledObject.gameObject); // Update the list of spawned bonuses.
    }

    // Get a bonus from its pool.
    private void OnGetFromPool<T>(T pooledObject) where T : Component
    {
        pooledObject.transform.position = spawnPosition;

        if (pooledObject is RevisedMissile missile)
        {
            // Reset the missile's velocity (it might have been fired before returning to the pool).
            var rb = pooledObject.GetComponent<Rigidbody>();
            rb.linearVelocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;

            // Reset the dirty flag.
            missile.Fired = false;
            pooledObject.transform.localScale = Vector3.one * 7; // Change the size to make it more visible as a collectible bonus.

            pooledObject.GetComponent<BoxCollider>().enabled = false; // Collider for when it is fired.
            pooledObject.GetComponent<SphereCollider>().enabled = true; // Collider for when it can be collected.

            missile.currentPool = MissilePoolType.None;
        }

        pooledObject.gameObject.SetActive(true);
        spawnedBonusComponents.Add(pooledObject.gameObject, pooledObject);
    }

    // Destroy a pooled object.
    private void OnDestroyPooledObject<T>(T pooledObject) where T : Component
    {
        Destroy(pooledObject.gameObject);
    }

    // With a certain probability (bonusSpawningProbability), invoke Get from a random pool among all existing bonus pools.
    // The input parameter represents the position where the player touched the probe (plane) and from which the spawning position of the bonus should be calculated.
    public void SpawnRandomBonus(Vector3 position)
    {
        if (UnityEngine.Random.value <= bonusSpawningProbability)
        {
            int spawnBounds = (int)(GlobalConstants.spawnDiameter / 2.5);
            spawnPosition = new Vector3(UnityEngine.Random.Range(-spawnBounds, spawnBounds), position.y, UnityEngine.Random.Range(-spawnBounds, spawnBounds));
            spawnPosition.y += offsetBonusSpawningPosition;

            switch (UnityEngine.Random.Range(0, bonusTypesNumber))
            {
                case 0:
                    missilePoolSpawner.Get();
                    break;

                case 1:
                    boostPoolSpawner.Get();
                    break;
            }
        }
    }

    // Method called by the Controller at the end of the run.
    public void AllObjectsInPool()
    {
        ExtractAllFromSpaceShipPool();

        // Iterate over a copy of the keys to avoid modification issues during iteration.
        List<GameObject> keys = new List<GameObject>(spawnedBonusComponents.Keys);
        foreach (GameObject obj in keys)
        {
            Component comp = spawnedBonusComponents[obj];

            if (comp is RevisedMissile missile)
            {
                missile.SpawnerPool?.Release(missile);
            }
            else if (comp is RevisedBoost boost)
            {
                boost.SpawnerPool?.Release(boost);
            }
        }
    }

    // Extract all missiles from the player's pool.
    private void ExtractAllFromSpaceShipPool()
    {
        List<RevisedMissile> missilesToRelease = new List<RevisedMissile>();

        // Iterate over a copy of the keys to avoid modification issues during iteration.
        List<GameObject> keys = new List<GameObject>(spawnedBonusComponents.Keys);
        foreach (GameObject obj in keys)
        {
            Component comp = spawnedBonusComponents[obj];

            if (comp is RevisedMissile missile && missile.currentPool == MissilePoolType.SpaceShipPool)
            {
                missilesToRelease.Add(missile);
            }
        }

        // Extract all missiles from the SpaceShipPool.
        foreach (var missile in missilesToRelease)
        {
            missile.SpaceShipPool?.Get();
        }
    }

    // Handle the event to update the player's transform.
    private void HandlePlayerTransformEvent(Transform transform)
    {
        playerTransform = transform;
    }

    // Handle the event to spawn a specific type of bonus.
    private void HandleSpawnBonusEvent(string bonusType)
    {
        if (playerTransform == null)
        {
            Debug.LogError("Player Transform is null in BonusSpawnerController HandleSpawnBonusEvent");
            return;
        }

        spawnPosition = playerTransform.position;
        spawnPosition.y += offsetBonusSpawningPosition;

        switch (bonusType)
        {
            case BonusTypes.Missile:
                missilePoolSpawner.Get();
                break;

            case BonusTypes.Boost:
                boostPoolSpawner.Get();
                break;
        }
    }
}