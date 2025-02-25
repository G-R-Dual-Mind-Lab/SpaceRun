using UnityEngine;
using Helper;

public class PlanetPositioner : MonoBehaviour
{
    [SerializeField] private GameObject[] planets; // Assign the planets in order: Neptune -> Uranus -> ... -> Mercury -> Sun
    private readonly float[] distancesInLightYears = { 0.00000f, 0.00026f, 0.00053f, 0.00086f, 0.00133f, 0.00137f, 0.00139f, 0.00142f, 0.00158f }; // Distances in light years
    private Vector2 randomPosition;

    void Start()
    {
        float previousRadius = 0f; // Previous planet's radius
        float currentPosition = 0f; // Current position

        float distanceInUnityUnits; // Distance in Unity units
        float currentRadius; // Current planet's radius

        for (int i = 0; i < planets.Length; i++)
        {
            // Convert distance from light years to Unity units
            distanceInUnityUnits = distancesInLightYears[i] * GlobalConstants.solarSystemScaleFactor;
            
            // If the planet is the Sun, position it farther from the wormhole due to its bright particle effects
            if (planets[i].name == TagsHelper.Sun)
            {
                // Calculate the current radius (taking the object's scale)
                // Necessary because distances were measured between the surfaces of the planets, whereas Unity measures from the center of the GameObjects
                currentRadius = planets[i].transform.localScale.x / 2;

                // Position = distance + previous planet's radius + current planet's radius
                currentPosition = distanceInUnityUnits + previousRadius + currentRadius;

                // For the Sun, position it at a custom distance from the wormhole due to its ParticleSystem
                randomPosition = GetRandomPointOnCircle(Vector3.zero, (float)(currentRadius * 1.5 + GlobalConstants.spawnDiameter / 1.5));
            }
            else
            {
                // Calculate the current radius (taking the object's scale)
                // Necessary because distances were measured between the surfaces of the planets, whereas Unity measures from the center of the GameObjects
                currentRadius = planets[i].transform.localScale.x / 2;

                // Position = distance + previous planet's radius + current planet's radius
                currentPosition = distanceInUnityUnits + previousRadius + currentRadius;

                // Get a random point on a circle for positioning the planet
                randomPosition = GetRandomPointOnCircle(Vector3.zero, (float)(currentRadius * 2 + GlobalConstants.spawnDiameter / 1.5));
            }

            // Set the planet's position
            planets[i].transform.position = new Vector3(randomPosition.x, currentPosition, randomPosition.y);

            // Update the previous radius
            previousRadius = currentRadius;
        }
    }

    // Get a random point on a circle
    private Vector2 GetRandomPointOnCircle(Vector2 center, float radius)
    {
        float angle = Random.Range(0f, Mathf.PI * 2); // Random angle in radians
        float x = center.x + radius * Mathf.Cos(angle);
        float y = center.y + radius * Mathf.Sin(angle);
        return new Vector2(x, y);
    }
}