using UnityEngine;
using System.Collections.Generic;
using UnityEngine.Pool;
using System.Threading.Tasks;
using Helper;

[System.Serializable]
public class SpawnableObject
{
    public GameObject prefab; // Prefab of the object.

    [Range(0, 100)] public float percentage; // Percentage of the total.

    public float minSelfRotationSpeed = 2f;  // Minimum self-rotation speed.
    public float maxSelfRotationSpeed = 5f;  // Maximum self-rotation speed.
}

public class SpaceObjectSpawnerController : MonoBehaviour
{   
    [Header("Prefab Settings")]
    [SerializeField] private List<SpawnableObject> planetPrefabs;  // List of planets with percentages.
    [SerializeField] private List<SpawnableObject> asteroidPrefabs;  // List of asteroids with percentages.
    [SerializeField] private List<SpawnableObject> shipwreckPrefabs; // List of shipwrecks.
    [SerializeField] private List<SpawnableObject> satellitePrefabs; // List of satellites.
    [SerializeField] private List<SpawnableObject> minePrefabs;      // List of space mines.

    [Header("Spawn Settings")]
    [SerializeField] private int numberOfPlanets = 200; // Number of planets to spawn.
    [SerializeField] private int numberOfAsteroids = 250; // Number of asteroids to spawn.
    [SerializeField] private int numberOfShipwrecks = 100; // Number of shipwrecks to spawn.
    [SerializeField] private int numberOfSatellites = 50; // Number of satellites to spawn.
    [SerializeField] private int numberOfMines = 100; // Number of mines to spawn.

    [SerializeField] private bool collectionCheck = true; // If enabled, throws an exception if an object that is already in the pool is added to the pool again.
    
    // Pool
    private IObjectPool<RevisedSpaceObject> spaceObjectPool;
    private int defaultCapacity; // Initial capacity of the pool.
    private int maxSize; // Maximum capacity of the pool.

    // List to store objects for which a valid position was found during the initial position calculation.
    private List<GameObject> positionedObjectsList = new List<GameObject>();
    
    // List containing all objects in the pool at any given time.
    private List<GameObject> pooledObjectsList = new List<GameObject>();

    // List of objects that have been spawned and not yet returned to the pool.
    private List<RevisedSpaceObject> spawnedObjectsList = new List<RevisedSpaceObject>();

    // List to temporarily store objects that have just been spawned.
    private List<GameObject> recentlySpawnedList = new List<GameObject>();

    // Components
    private RevisedSpaceObject pooledObject;
    private RevisedSpaceObject newPooledObject;

    private Vector3 positionOnTop;

    // Variable to track the current game state.
    private GameState currentState;

    // Property to set the current game state.
    public GameState CurrentState
    {
        set => currentState = value;
    }

    private void Awake()
    {
        // Set the pool capacity based on the number of space objects to be spawned.
        maxSize = numberOfPlanets + numberOfAsteroids + numberOfShipwrecks + numberOfSatellites + numberOfMines; 
        defaultCapacity = maxSize;

        // Initialize the pool.
        spaceObjectPool = new ObjectPool<RevisedSpaceObject>(
            () => CreateSpaceObject(null, Vector3.zero, Quaternion.identity),
            OnGetFromPool,
            OnReleaseToPool,
            OnDestroyPooledObject,
            collectionCheck,
            defaultCapacity,
            maxSize
        );
    }

    // Create a space object and add the RevisedSpaceObject component to it.
    private RevisedSpaceObject CreateSpaceObject(GameObject prefab, Vector3 position, Quaternion rotation)
    {
        newPooledObject = Instantiate(prefab, position, rotation).AddComponent<RevisedSpaceObject>();
        newPooledObject.ObjectPool = spaceObjectPool; // Set the pool to which the object belongs.
        return newPooledObject;
    }

    // Release a space object back to the pool.
    private void OnReleaseToPool(RevisedSpaceObject pooledObject)
    {
        spawnedObjectsList.Remove(pooledObject);
        pooledObject.gameObject.SetActive(false);

        if (currentState == GameState.Playing)
        {
            RespawnSpaceObject();
        }
    }

    // Get a space object from the pool.
    private async void OnGetFromPool(RevisedSpaceObject pooledObject)
    {
        spawnedObjectsList.Add(pooledObject);
        pooledObject.gameObject.SetActive(true);

        await RemoveObjectFromList<GameObject>(2000, recentlySpawnedList, pooledObject.gameObject); // 2000 milliseconds = 2 seconds.
    }

    // Fill the pool with space objects.
    public void PoolFiller() 
    {
        PoolAdder(planetPrefabs, numberOfPlanets);
        PoolAdder(asteroidPrefabs, numberOfAsteroids);
        PoolAdder(shipwreckPrefabs, numberOfShipwrecks);
        PoolAdder(satellitePrefabs, numberOfSatellites);
        PoolAdder(minePrefabs, numberOfMines);
    }

    // Add space objects to the pool.
    private void PoolAdder(List<SpawnableObject> spawnableObjects, int totalCount) 
    {
        Dictionary<GameObject, int> objectSpawnCounts = CalculateSpawnCounts(spawnableObjects, totalCount); // Calculate the number of objects to spawn for each prefab based on the percentages.

        foreach (var kvp in objectSpawnCounts) 
        {
            GameObject prefab = kvp.Key; // Object to create.
            int count = kvp.Value; // Number of objects to create.

            SpawnableObject spawnable = spawnableObjects.Find(x => x.prefab == prefab); // Find the corresponding SpawnableObject from the dictionary key.

            for (int i = 0; i < count; i++)
            {
                RevisedSpaceObject revisedSpaceObject = CreateSpaceObject(prefab, Vector3.zero, Quaternion.identity); // Create the object.
                spaceObjectPool.Release(revisedSpaceObject); // Add the object to the pool (set active == false).
                pooledObjectsList.Add(revisedSpaceObject.gameObject); // Add to the list of pooled objects.

                ComputeObjectRotation(spawnable, revisedSpaceObject); // Calculate and initialize the object's rotation.
                revisedSpaceObject.AddImpulse(); // Apply an initial impulse to the object.
            }
        }
    }

    // Calculate the number of objects to spawn for each type based on the percentage and the total defined percentages.
    private Dictionary<GameObject, int> CalculateSpawnCounts(List<SpawnableObject> spawnableObjects, int totalCount)
    {
        Dictionary<GameObject, int> spawnCounts = new Dictionary<GameObject, int>();
        float totalPercentage = 0f;

        // Calculate the total percentage.
        foreach (var spawnableObject in spawnableObjects)
        {
            totalPercentage += spawnableObject.percentage;
        }

        if (totalPercentage > 100f)
        {
            Debug.LogWarning("The sum of the percentages exceeds 100%. Normalizing...");
        }

        // Normalize the percentages and calculate the counts.
        foreach (var spawnableObject in spawnableObjects)
        {
            float normalizedPercentage = (spawnableObject.percentage / totalPercentage);
            int count = Mathf.RoundToInt(normalizedPercentage * totalCount);
            spawnCounts[spawnableObject.prefab] = count;
        }

        return spawnCounts;
    }

    // Calculate the rotation speed and initialize the object's rotation.
    private void ComputeObjectRotation(SpawnableObject spawnable, RevisedSpaceObject revisedSpaceObject) 
    {
        float rotationSpeed = Random.Range(spawnable.minSelfRotationSpeed, spawnable.maxSelfRotationSpeed); // Calculate a random rotation speed based on the parameters from the inspector.
        revisedSpaceObject.InitializeObjectRotation(rotationSpeed); // Initialize the rotation.
    }

    // Pre-calculate the positions where the objects will be spawned when the game starts.
    // This method is called at the beginning of each run to provide a different experience each time.
    private void ArrangePooledObjects()
    {
        Vector3 randomPosition;
        bool positionFound;

        for (int i = 0; i < pooledObjectsList.Count; i++)
        {
            if (pooledObjectsList[i] != null)
            {
                positionFound = false;

                for (int attempts = 0; attempts < 100; attempts++) // Max 100 attempts.
                {
                    randomPosition = GenerateRandomPositionInCylinder(GlobalConstants.spawnLength, GlobalConstants.spawnDiameter / 2);

                    if (IsPositionValid(randomPosition, pooledObjectsList[i], positionedObjectsList))
                    {
                        positionFound = true;
                        pooledObjectsList[i].transform.position = randomPosition;
                        positionedObjectsList.Add(pooledObjectsList[i]);
                        break;
                    }
                }

                if (!positionFound)
                {
                    Debug.LogWarning($"Unable to find a valid position for an object of {pooledObjectsList[i].name}.");
                }
            }
        }
    }

    // Generate a random position within the wormhole.
    private Vector3 GenerateRandomPositionInCylinder(float height, float radius)
    {
        float angle = Random.Range(0f, Mathf.PI * 2); // Random angle between 0 and 360 degrees.
        float distance = Random.Range(0f, radius);   // Random distance from the center (for uniform distribution).

        float y = Random.Range(-((height/2)-GlobalConstants.spawnLength/5), height / 2); // Random height along the Y axis.

        // Convert from polar to Cartesian coordinates.
        float x = distance * Mathf.Cos(angle);
        float z = distance * Mathf.Sin(angle);

        return new Vector3(x, y, z); // Random position within the cylinder.
    }

    // Check if the position (first parameter) is valid using the positionedObjectsList.
    private bool IsPositionValid(Vector3 position, GameObject prefab, List<GameObject> list)
    {
        // Get the bounding box of the prefab to be spawned.
        Bounds prefabBounds = prefab.GetComponent<Renderer>().bounds;
        prefabBounds.center = position; // Position the bounding box at the target position.

        // Check if it overlaps with already spawned objects.
        foreach (GameObject existingObject in list)
        {
            Bounds existingBounds = existingObject.GetComponent<Renderer>().bounds;

            if (prefabBounds.Intersects(existingBounds))
            {
                return false; // The bounding boxes overlap.
            }
        }

        return true; // The position is valid.
    }

    // Draw and display the wormhole in the scene for visual reference.
    void OnDrawGizmos()
    {
        Gizmos.color = Color.green;
        float radius = GlobalConstants.spawnDiameter / 2;
        float height = GlobalConstants.spawnLength;

        // Draw a cylinder centered at Vector3.zero.
        for (int i = 0; i < 36; i++) // Divide the circle into segments.
        {
            float angle1 = Mathf.Deg2Rad * (i * 10);
            float angle2 = Mathf.Deg2Rad * ((i + 1) * 10);

            Vector3 point1 = new Vector3(radius * Mathf.Cos(angle1), -height / 2, radius * Mathf.Sin(angle1));
            Vector3 point2 = new Vector3(radius * Mathf.Cos(angle2), -height / 2, radius * Mathf.Sin(angle2));
            Gizmos.DrawLine(point1, point2);

            point1.y += height;
            point2.y += height;
            Gizmos.DrawLine(point1, point2);

            // Connect the top and bottom circles.
            Gizmos.DrawLine(new Vector3(radius * Mathf.Cos(angle1), -height / 2, radius * Mathf.Sin(angle1)),
                            new Vector3(radius * Mathf.Cos(angle1), height / 2, radius * Mathf.Sin(angle1)));
        }
    }

    // Release all objects in the pool.
    public void SpawnAllObjectsInPool()
    {
        while (spaceObjectPool.CountInactive != 0)
        {
            spaceObjectPool.Get();
        }
    }

    // Reset the spawner.
    public void ResetSpawner()
    {
        AllObjectsInPool(); // Add all objects to the pool.
        ArrangePooledObjects(); // Pre-calculate and assign initial positions to the space objects in the pool.
        positionedObjectsList.Clear();
    }

    // Called when the space object enters the pool, but only if the game state is "Playing".
    private void RespawnSpaceObject()
    {
        if(spaceObjectPool.CountInactive > 0)
        {
            pooledObject = spaceObjectPool.Get();
            positionOnTop = GenerateRandomPositionInCylinder(2, GlobalConstants.spawnDiameter / 2);
            positionOnTop.y = positionOnTop.y + transform.position.y + (GlobalConstants.spawnLength/2);

            if (IsPositionValid(positionOnTop, pooledObject.gameObject, recentlySpawnedList))
            {
                pooledObject.transform.position = positionOnTop;
                recentlySpawnedList.Add(pooledObject.gameObject);
            }
            else
            {
                spaceObjectPool.Release(pooledObject);
            }
        }
    }

    // Remove an object from the list after a delay.
    private async Task RemoveObjectFromList<T>(int delayMilliseconds, List<T> list, T gameObject)
    {
        await Task.Delay(delayMilliseconds); // Wait for the specified milliseconds.
        list.Remove(gameObject);
    }

    // Destroy a pooled object.
    private void OnDestroyPooledObject(RevisedSpaceObject pooledObject)
    {
        Destroy(pooledObject.gameObject);
    }

    // Add all objects to the pool.
    private void AllObjectsInPool()
    {
        Rigidbody rbSpaceObject;
        
        for (int i = spawnedObjectsList.Count - 1; i >= 0; i--)
        {
            rbSpaceObject = spawnedObjectsList[i].GetComponent<Rigidbody>();
            rbSpaceObject.linearVelocity = Vector3.zero;
            rbSpaceObject.angularVelocity = Vector3.zero;
            spawnedObjectsList[i].AddImpulse();
            spaceObjectPool.Release(spawnedObjectsList[i]);
        }
    }
}