using NUnit.Framework;
using UnityEngine;
using System.Collections.Generic;
[CreateAssetMenu(fileName = "SpawnScenario", menuName = "Scriptable Objects/SpawnScenario")]
public class SpawnScenario : ScriptableObject
{
    public List<SpawnData> Scenarios = new List<SpawnData>();
}
