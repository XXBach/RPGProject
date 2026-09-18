using UnityEngine;
using System;

public static class CombatSignal
{
    public static event Action<CombatData> CombatRequested;

    public static event Action<CombatData> CombatEnded;

    public static void FireCombatScene(CombatData data)
    {
        CombatRequested?.Invoke(data);
    }

    public static void EndCombatScene(CombatData data)
    {
        CombatEnded?.Invoke(data);
    }
}
