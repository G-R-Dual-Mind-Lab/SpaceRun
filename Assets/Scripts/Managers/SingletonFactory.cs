using System;
using System.Collections.Generic;
using UnityEngine;

public class SingletonFactory
{
    private static Dictionary<Type, IManager> managers = new();

    public static T GetManager<T>(Dictionary<Type, ScriptableObject> channels) where T : MonoBehaviour, IManager
    {
        Type type = typeof(T);

        if (!managers.ContainsKey(type))
        {
            // Create or retrieve the singleton instance
            T instance = Singleton<T>.Instance;
            Debug.Log("Instantiated " + type + " by the Singleton Factory.");

            if (instance is IManager configurable) // if the manager implements the IManager interface, configure the channels
            {
                configurable.ConfigureChannels(channels); // configure channels
            }

            // Add to the list of managers
            managers[type] = instance;

            // Initialize the manager
            instance.Initialize();
        }

        return (T)managers[type];
    }
}
