using System.Collections.Generic;
using TMPro;
using UnityEngine;

[System.Serializable]
public class Spaceship
{
    public string name;
    public int maxSpeed;
    public float acceleration;
}

[System.Serializable]
public class SpaceshipData
{
    public List<Spaceship> spaceShipsDataList;
}

public class HangarController : MonoBehaviour
{
    [Header("Hangar Publishing Channels")]
    [SerializeField] private GameObjectEventChannelSO choosedSpaceShipEventChannel;

    [Header("Scriptable Objects")]
    [SerializeField] private SpaceShipProperties shipProperties;

    [Header("UI Elements")]
    [SerializeField] private TextMeshPro maxSpeedTextMeshPro;
    [SerializeField] private TextMeshPro throttleResponseTextMeshPro;

    // Shared ScriptableObject containing spaceship characteristics
    private SpaceshipData spaceshipData;

    // Other Variables
    private int currentShip;
    private GameObject instantiatedShip;
    private Vector3 position;
    private Quaternion rotation;
    private List<object> spaceShipPrefabs;


    //////////////////////////////// LIFECYCLE METHODS ////////////////////////////////

    private void Awake()
    {
        if (choosedSpaceShipEventChannel == null)
        {
            Debug.LogError("choosedSpaceShipEventChannel is null in HangarController Awake");
        }

        LoadSpaceshipData();

        spaceShipPrefabs = new List<object>();

        position = new Vector3(-17.4f, -6199.415f, -28.84f);
        rotation = Quaternion.Euler(new Vector3(14.65f, 108.9f, 4.05f));

        currentShip = 0;
    }

    private void OnEnable()
    {
        LoadAllSpaceships();

        instantiatedShip = Instantiate((GameObject)spaceShipPrefabs[currentShip], position, rotation);
        shipProperties.maxSpeed = spaceshipData.spaceShipsDataList[currentShip].maxSpeed;
        shipProperties.acceleration = spaceshipData.spaceShipsDataList[currentShip].acceleration;

        maxSpeedTextMeshPro.text = "MAX SPEED:\n" + (shipProperties.maxSpeed * 0.001 * 3600).ToString("F2") + " KM/H";
        throttleResponseTextMeshPro.text = "THROTTLE RESPONSE:\n" + shipProperties.acceleration.ToString("F1");
    }

    private void OnDisable()
    {
        Destroy(instantiatedShip);
        UnloadAllSpaceships();
    }


    //////////////////////////////// CLASS METHODS ////////////////////////////////

    // Load spaceship data from a JSON file in the Resources folder
    private void LoadSpaceshipData()
    {
        TextAsset jsonFile = Resources.Load<TextAsset>("Data/SpaceshipsData");

        if (jsonFile == null)
        {
            Debug.LogError("JSON file not found in Resources.");
            return;
        }

        // Deserialize the JSON into a SpaceshipData object
        spaceshipData = JsonUtility.FromJson<SpaceshipData>(jsonFile.text);
    }

    // Load all spaceship prefabs
    private void LoadAllSpaceships()
    {
        foreach (var spaceship in spaceshipData.spaceShipsDataList)
        {
            spaceShipPrefabs.Add(Resources.Load("Prefabs/Spaceship/Models/" + spaceship.name));
        }
    }

    // Unload all spaceship prefabs
    private void UnloadAllSpaceships()
    {
        spaceShipPrefabs.Clear();
        Resources.UnloadUnusedAssets();
    }

    // Select the next spaceship
    public void OnNext()
    {
        Destroy(instantiatedShip);
        currentShip = (currentShip + 1) % spaceShipPrefabs.Count;
        instantiatedShip = Instantiate((GameObject)spaceShipPrefabs[currentShip], position, rotation);

        maxSpeedTextMeshPro.text = "MAX SPEED:\n" + (spaceshipData.spaceShipsDataList[currentShip].maxSpeed * 0.001 * 3600).ToString("F2") + " KM/H";
        throttleResponseTextMeshPro.text = "THROTTLE RESPONSE:\n" + spaceshipData.spaceShipsDataList[currentShip].acceleration.ToString("F1");
    }

    // Select the previous spaceship
    public void OnPrevious()
    {
        Destroy(instantiatedShip);
        currentShip = (currentShip - 1 + spaceShipPrefabs.Count) % spaceShipPrefabs.Count;
        instantiatedShip = Instantiate((GameObject)spaceShipPrefabs[currentShip], position, rotation);

        maxSpeedTextMeshPro.text = "MAX SPEED:\n" + (spaceshipData.spaceShipsDataList[currentShip].maxSpeed * 0.001 * 3600).ToString("F2") + " KM/H";
        throttleResponseTextMeshPro.text = "THROTTLE RESPONSE:\n" + spaceshipData.spaceShipsDataList[currentShip].acceleration.ToString("F1");
    }

    // Choose the current spaceship
    public void OnChoose()
    {
        shipProperties.maxSpeed = spaceshipData.spaceShipsDataList[currentShip].maxSpeed;
        shipProperties.acceleration = spaceshipData.spaceShipsDataList[currentShip].acceleration;
        choosedSpaceShipEventChannel.RaiseEvent(instantiatedShip);
    }
}