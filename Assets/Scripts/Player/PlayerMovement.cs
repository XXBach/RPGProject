using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections;
using System.Linq;
using UnityEngine.Events;
public enum MovementState 
{
    IDLE = 0,
    MOVEMENTRANGEVISUAL = 1,
    MOVING = 2,
    ARRIVED = 3,
}

public class OnMovementEndArgs
{
    public Vector2Int FinalPosition;
    public NodeState FinalNodeState;
}
public class PlayerMovement : MonoBehaviour, IMovement
{
    public MovementState MCMovementState;
    public UnityEvent<OnMovementEndArgs> OnMovementEnd;

    [SerializeField] private InputActionAsset _inputActionAsset;
    [SerializeField] private GameObject _movablePrefab;
    [SerializeField] private GameObject _unmovablePrefab;
    [SerializeField] private GameObject _attackablePrefab;
    private float _moveSpeed;
    private int _activeTileCount = 0;
    private int _movementRange;
    private InputAction _selectAction;
    private PathFinding _pathFinding;
    private List<PathNode> _path;
    private Coroutine _handleMovementCoroutine;
    private bool _isMoving = false;
    private Animator _animator;
    private List<GameObject> _tilePool = new List<GameObject>();
    private MovementState _previousState;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        this.MCMovementState = MovementState.IDLE;
        _pathFinding = new PathFinding(GridSetup.Grid);
        _selectAction = _inputActionAsset.FindAction("Select");
        this._moveSpeed = this.gameObject.GetComponent<Player>().CurrentDatas.CurrentSpeed;
        this._movementRange = this.gameObject.GetComponent<Player>().CurrentDatas.CurrentMovementRange;
        _animator = GetComponent<Animator>();
    }

    // Update is called once per frame
    void Update()
    {
       if(this.MCMovementState != MovementState.IDLE)
       {
           HandleTurn();
       }
       else
       {
            return;
       }
    }
    private void HandleTurn()
    {
        Vector2Int playerPosition = GridSetup.Grid.GetGridPosition(this.transform.position);
        List<PathNode> ReachableNodes = _pathFinding.GetReachableNodes(playerPosition.x, playerPosition.y, this._movementRange * 10);

        if (this.MCMovementState == MovementState.MOVEMENTRANGEVISUAL)
        {
            if (this._previousState != MovementState.MOVEMENTRANGEVISUAL)
            {
                RangeVisualize(ReachableNodes);
                _previousState = MovementState.MOVEMENTRANGEVISUAL;
            }
            if (_selectAction.WasPressedThisFrame())
            {
                Vector2 mousePosition = Mouse.current.position.ReadValue();
                Vector3 mouseWorldPosition = Camera.main.ScreenToWorldPoint(mousePosition);
                Vector2Int DestinationGridPosition = new Vector2Int();
                DestinationGridPosition = _pathFinding.Grid.GetGridPosition(mouseWorldPosition);
                PathNode destinationNode = _pathFinding.Grid.GetGridObject(DestinationGridPosition.x, DestinationGridPosition.y);
                if (!IsMovementValid(ReachableNodes, destinationNode))
                {
                    Debug.Log("Outofbound");
                    this.MCMovementState = MovementState.MOVEMENTRANGEVISUAL;
                    return;
                }
                if (destinationNode.State != 0)
                {
                    Debug.Log("Unwalkable");
                    this.MCMovementState = MovementState.MOVEMENTRANGEVISUAL;
                    return;
                }
                Vector2Int CurrentGridPosition = GridSetup.Grid.GetGridPosition(transform.position);
                _path = _pathFinding.FindPath(DestinationGridPosition.x, DestinationGridPosition.y);
                this.MCMovementState = MovementState.MOVING;
            }
            else
            {
                //Debug.Log("No Action this frame");
                this.MCMovementState = MovementState.MOVEMENTRANGEVISUAL;
            }
        }
        else if (this.MCMovementState == MovementState.MOVING)
        {
            _handleMovementCoroutine = StartCoroutine(HandleMovement());
        }
        else if (this.MCMovementState == MovementState.ARRIVED)
        {
            CameraSignals.RequestMove(transform.position, smooth: true);
            HideAllTiles();
            this._previousState = MCMovementState;
            this.MCMovementState = MovementState.IDLE;
        }
    }
    private void RangeVisualize(List<PathNode>ReachableNodes)
    {
        int needed = ReachableNodes.Count;
        while (_tilePool.Count < needed)
        {
            GameObject go = Instantiate(_movablePrefab);
            go.SetActive(false);
            _tilePool.Add(go);
        }
        for (int i = 0; i < needed; i++)
        {
            PathNode node = ReachableNodes[i];
            node.State = NodeState.Walkable;

            GameObject tile = _tilePool[i];
            tile.transform.position = GridSetup.Grid.GetCellWorldPosition(node.XCoordinate, node.YCoordinate);
            tile.transform.position = new Vector3(tile.transform.position.x + 0.5f, tile.transform.position.y);
            tile.SetActive(true);
        }
        for (int i = needed; i < _activeTileCount; i++)
        {
            _tilePool[i].SetActive(false);
        }
        _activeTileCount = needed;
    }
    private void HideAllTiles()
    {
        foreach (GameObject tile in _tilePool)
        {
            tile.SetActive(false);
        }
        _activeTileCount = 0;
    }
    private bool IsMovementValid(List<PathNode> ReachableNodes, PathNode DestinationNode)
    {
        bool isMovementValid = ReachableNodes.Any(x => x.XCoordinate == DestinationNode.XCoordinate && x.YCoordinate == DestinationNode.YCoordinate);
        return isMovementValid;
    }
    private IEnumerator HandleMovement()
    {
        PathNode currentPoint = _path[0];
        int i = 0;
        Vector3 currentPointPosition = new Vector3(currentPoint.XCoordinate, currentPoint.YCoordinate);
        currentPointPosition = currentPointPosition + Vector3.one * 0.5f;
        currentPointPosition.z = 0;
        PathNode endPoint = _path[_path.Count - 1];
        while (true)
        {
            _isMoving = true;
            if (isArrived(currentPointPosition))
            {
                if (_path[i] == endPoint)
                {
                    _isMoving = false;
                    OnMovementEnd?.Invoke(new OnMovementEndArgs
                    {
                        FinalPosition = new Vector2Int(endPoint.XCoordinate, endPoint.YCoordinate),
                        FinalNodeState = NodeState.BlockedByCharacter
                    });
                    yield break;
                }
                i++;
                currentPoint = _path[i];
                if (currentPoint.XCoordinate > _path[i - 1].XCoordinate)
                {
                    _animator.Play("PrinceEldric_TurnRightAnim");
                }
                else if (currentPoint.XCoordinate < _path[i - 1].XCoordinate) { _animator.Play("PrinceEldric_TurnLeftAnim"); }
                else if (currentPoint.YCoordinate > _path[i - 1].YCoordinate) { _animator.Play("PrinceEldric_BehindAnim"); }
                else if (currentPoint.YCoordinate < _path[i - 1].YCoordinate) { _animator.Play("PrinceEldric_IdleAnim"); }
                currentPointPosition = new Vector3(currentPoint.XCoordinate, currentPoint.YCoordinate);
                currentPointPosition = currentPointPosition + Vector3.one * 0.5f;
                currentPointPosition.z = 0;
            }
            transform.position = Vector3.MoveTowards(transform.position, currentPointPosition, _moveSpeed * Time.deltaTime);
            this.MCMovementState = MovementState.ARRIVED;
            this._previousState = MovementState.MOVING;
            yield return null;
        }
    }
    private bool isArrived(Vector3 currentPointToReach)
    {

        return (Vector3.Distance(this.transform.position, currentPointToReach) <= 0.05f);
    }
    public void SetMovementState(MovementState state)
    {
        this.MCMovementState = state;
    }
    public UnityEvent<OnMovementEndArgs> GetOnMovementEnd()
    {
        return this.OnMovementEnd;
    }
    private void OnEnable()
    {
        _inputActionAsset.FindActionMap("Player").Enable();
    }
    private void OnDisable()
    {
        _inputActionAsset.FindActionMap("Player").Disable();
    }
    private void OnDrawGizmos()
    {
        if (_path == null) return;

        Gizmos.color = Color.green;
        for (int i = 0; i < _path.Count - 1; i++)
        {
            Vector3 from = new Vector3(_path[i].XCoordinate, _path[i].YCoordinate) + Vector3.one * 0.5f;
            Vector3 to = new Vector3(_path[i + 1].XCoordinate, _path[i + 1].YCoordinate) + Vector3.one * 0.5f;
            Gizmos.DrawLine(from, to);
        }
    }
    public PathFinding GetPathFinding()
    {
        return _pathFinding;
    }
}
