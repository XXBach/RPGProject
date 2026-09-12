using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Experimental.GlobalIllumination;
public interface IMovement
{
    GameObject gameObject { get; }
    public UnityEvent<OnMovementEndArgs> GetOnMovementEnd();
    public void SetMovementState(MovementState state);
    public PathFinding GetPathFinding();
}
