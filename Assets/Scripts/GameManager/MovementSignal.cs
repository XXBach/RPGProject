using UnityEngine;
public static class MovementSignal
{
    public static event System.Action<OnMovementEndArgs> MovementEnded;
    public static void FireMovementEnded(OnMovementEndArgs args) => MovementEnded?.Invoke(args);
}
