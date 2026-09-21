using System.Collections.Generic;
using System.Collections;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Events;
public class EnemyMovement : MonoBehaviour, IMovement
{
    public UnityEvent<OnMovementEndArgs> OnMovementEnd;
    private int _moveSpeed;
    private PathFinding _pathFinding;
    private List<PathNode> _path;
    private Coroutine _handleMovementCoroutine;
    private bool _isMoving = false;
    private Animator _animator;
    private MovementState _movementState;


    private void Awake()
    {
        _animator = GetComponent<Animator>();
        _movementState = MovementState.IDLE;
    }
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        _pathFinding = new PathFinding(GridSetup.Grid);
        this._moveSpeed = this.GetComponent<Enemy>().CurrentDatas.CurrentSpeed;

    }
    public void MoveToTargetNearestCell(PlayerCharsPosition target)
    {
        Vector2Int CurrentPosition = GridSetup.Grid.GetGridPosition(transform.position);
        List<PathNode> MovementRange = _pathFinding.GetReachableNodes(CurrentPosition.x, CurrentPosition.y, _moveSpeed);
        Vector2Int NearestCoordinate = FindNearestPossibleCoordinatesToTarget(target.PlayerCharCoodinates, MovementRange);
        MoveToTargetCell(NearestCoordinate);
    }
    public void MoveToTargetCell(Vector2Int CellCoodinates)
    {
        _path = _pathFinding.FindPath(CellCoodinates.x, CellCoodinates.y);
        StartCoroutine(HandleMovement());
    }
    private IEnumerator HandleMovement()
    {
        PathNode currentPoint = _path[0];
        int i = 0;
        Vector3 currentPointPosition = new Vector3(currentPoint.XCoordinate, currentPoint.YCoordinate);
        currentPointPosition = currentPointPosition + Vector3.one * 0.5f;
        currentPointPosition.z = 0;
        PathNode endPoint = _path[_path.Count - 1];
        while (true) {
            _isMoving = true;
            if(isArrived(currentPointPosition))
            {
                if (_path[i] == endPoint)
                {
                    _isMoving = false;
                    OnMovementEnd?.Invoke(new OnMovementEndArgs
                    {
                        FinalPosition = new Vector2Int(endPoint.XCoordinate, endPoint.YCoordinate),
                        FinalNodeState = endPoint.State
                    });
                    yield break;
                }
                i++;
                currentPoint = _path[i];
                if (currentPoint.XCoordinate > _path[i - 1].XCoordinate)
                {
                    _animator.Play("PrinceRaelan_TurnRightAnim");
                }
                else if (currentPoint.XCoordinate < _path[i - 1].XCoordinate) { _animator.Play("PrinceRaelan_TurnLeftAnim"); }
                else if (currentPoint.YCoordinate > _path[i - 1].YCoordinate) { _animator.Play("PrinceRaelan_BehindAnim"); }
                else if (currentPoint.YCoordinate < _path[i - 1].YCoordinate) { _animator.Play("PrinceRaelan_WalkingAnim"); }
                currentPointPosition = new Vector3(currentPoint.XCoordinate, currentPoint.YCoordinate);
                currentPointPosition = currentPointPosition + Vector3.one * 0.5f;
                currentPointPosition.z = 0;
            }
            transform.position = Vector3.MoveTowards(transform.position, currentPointPosition, _moveSpeed * Time.deltaTime);
            yield return null;
        }
    }
    private bool isArrived(Vector3 currentPointToReach)
    {

        return (Vector3.Distance(this.transform.position, currentPointToReach) <= 0.05f);
    }
    public void SetMovementState(MovementState movementState)
    {
        _movementState = movementState;
    }
    public UnityEvent<OnMovementEndArgs> GetOnMovementEnd()
    {
        return this.OnMovementEnd;
    }
    public PathFinding GetPathFinding() { 
        return _pathFinding;
    }
    public void RangeVisualize(List<PathNode> reachableNodes)
    {

    }
    public Vector2Int FindNearestPossibleCoordinatesToTarget(Vector2Int TargetCoordinates, List<PathNode> MovementRange)
    {
        Vector2Int NearestCoordinates = new Vector2Int(MovementRange[0].XCoordinate, MovementRange[0].YCoordinate);
        float NearestRelativeDistance = Vector2.Distance(TargetCoordinates, NearestCoordinates);
        foreach (var Cell in MovementRange)
        {
            float relativeDistance = Vector2.Distance(TargetCoordinates, new Vector2Int(Cell.XCoordinate, Cell.YCoordinate));
            if (NearestRelativeDistance > relativeDistance)
            {
                NearestRelativeDistance = relativeDistance;
                NearestCoordinates = new Vector2Int(Cell.XCoordinate, Cell.YCoordinate);
            }
        }
        return NearestCoordinates;
    }
    public Vector2Int FindFurthestPossibleCoodinates(PlayerCharsPosition target, List<PathNode> MovementRange)
    {
        Vector2Int FurthestPossibleCoordinates = new Vector2Int();
        return FurthestPossibleCoordinates;
    }
    public bool IsCharInValidRange(PlayerCharsPosition target, List<PathNode> ValidRange)
    {
        foreach (PathNode gridcell in ValidRange)
        {
            if (target.PlayerCharCoodinates.x == gridcell.XCoordinate && target.PlayerCharCoodinates.y == gridcell.YCoordinate)
            {
                return true;
            }
        }
        return false;
    }
}
