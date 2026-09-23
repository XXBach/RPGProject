using System.Collections.Generic;
using System.Collections;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Events;
[DefaultExecutionOrder(-90)]
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
        int NearestRelativeDistance = GetManhattanDistance(TargetCoordinates, NearestCoordinates);

        foreach (var Cell in MovementRange)
        {
            Vector2Int cellCoordinates = new Vector2Int(Cell.XCoordinate, Cell.YCoordinate);
            int relativeDistance = GetManhattanDistance(TargetCoordinates, cellCoordinates);

            if (relativeDistance < NearestRelativeDistance)
            {
                NearestRelativeDistance = relativeDistance;
                NearestCoordinates = cellCoordinates;
            }
        }
        return NearestCoordinates;
    }
    public Vector2Int FindNearestPossibleCoordinates(PlayerCharsPosition target, List<PathNode> MovementRange, int SkillRange)
    {
        //Mục tiêu: Lấy ô gần nhất trong tầm di chuyển của Enemy mà kẻ địch nằm trong tầm tấn công
        //Lây ô với LowestGCost
        //Duyệt xem từ ô đó với tầm tấn công đó thì có thể đánh tới target không
        //Không thì duyệt tiếp bỏ ô đó ra
        //Có thì return tọa độ ô đó
        PathNode NearestPossibleNode = new PathNode(GridSetup.Grid, 0, 0);
        List<PathNode> Checked = new List<PathNode>();
        List<PathNode> ReachableNodesFromNode = new List<PathNode>();
        while(MovementRange.Count != 0)
        {
            NearestPossibleNode = _pathFinding.getLowestGCostNode(MovementRange);
            MovementRange.Remove(NearestPossibleNode);
            Checked.Add(NearestPossibleNode);

            ReachableNodesFromNode = _pathFinding.GetReachableNodes(NearestPossibleNode.XCoordinate, NearestPossibleNode.YCoordinate, SkillRange * 10);
            if (IsCharInValidRange(target, ReachableNodesFromNode))
            {
                MovementRange.AddRange(Checked);
                Vector2Int Result = new Vector2Int(NearestPossibleNode.XCoordinate, NearestPossibleNode.YCoordinate);
                return Result;
            }
        }
        return new Vector2Int(-100, -100);
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

    private int GetManhattanDistance(Vector2Int a, Vector2Int b)
    {
        return Mathf.Abs(a.x - b.x) + Mathf.Abs(a.y - b.y);
    }
}
