using UnityEngine;

[CreateAssetMenu(fileName = "SettingsDataScriptableObject", menuName = "Scriptable Objects/SettingsDataScriptableObject")]
public class SettingsDataScriptableObject : ScriptableObject
{
    public int volumePercentage;
    public bool isTutorialEnabled;
}
