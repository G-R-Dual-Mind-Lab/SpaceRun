﻿using UnityEngine;

[RequireComponent(typeof(Rigidbody))]

public class SingularityPullable : MonoBehaviour
{
    // Add this script to objects you want to be pulled by the Singularity script.
    public bool pullable = true;
}
