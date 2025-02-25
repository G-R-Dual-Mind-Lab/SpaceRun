using System;
using System.Collections.Generic;
using UnityEngine;

public interface IManager
{
    void Initialize(); // Optional method to initialize the manager

    void ConfigureChannels(Dictionary<Type, ScriptableObject> channels); // Method to initialize the channels

    void RegisterDelegates(); // Method to register event delegates
}