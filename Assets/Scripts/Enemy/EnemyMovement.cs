using System.Collections.Generic;
using System.Collections;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Events;
public class EnemyMovement : MonoBehaviour, IMovement
{
    public UnityEvent<OnMovementEndArgs> OnMovementEnd;
    [SerializeField] private InputActionAsset _inputActionAsset;
    private float _moveSpeed;
    private InputAction _selectAction;
    private PathFinding _pathFinding;
    private List<PathNode> _path;
    private Coroutine _handleMovementCoroutine;
    private bool _isMoving = false;
    private Animator _animator;
    private MovementState _movementState;

    
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        _pathFinding = new PathFinding(GridSetup.Grid);
        _selectAction = _inputActionAsset.FindAction("Select");
        this._moveSpeed = this.GetComponent<Enemy>().CurrentDatas.CurrentSpeed;
        _animator = GetComponent<Animator>();
    }

    // Update is called once per frame
    void Update()
    {
        if (_selectAction.WasPressedThisFrame() && !_isMoving)
        {
            Vector2 mousePosition = Mouse.current.position.ReadValue();
            Vector3 mouseWorldPosition = Camera.main.ScreenToWorldPoint(mousePosition);
            Vector2Int DestinationGridPosition = new Vector2Int();
            DestinationGridPosition = _pathFinding.Grid.GetGridPosition(mouseWorldPosition);
            if (DestinationGridPosition.x == -1 && DestinationGridPosition.y == -1)
            {
                Debug.Log("Outofbound");
                return;
            }

            PathNode destinationNode = _pathFinding.Grid.GetGridObject(DestinationGridPosition.x, DestinationGridPosition.y);
            if (destinationNode.State != 0)
            {
                Debug.Log("Unwalkable");
                return;
            }
            Vector2Int CurrentGridPosition = GridSetup.Grid.GetGridPosition(transform.position);
            _path = _pathFinding.FindPath(DestinationGridPosition.x, DestinationGridPosition.y);
            _handleMovementCoroutine = StartCoroutine(HandleMovement());
            CameraSignals.RequestMove(mouseWorldPosition, smooth: true);
        }
    }
    private void HandleTurn()
    {

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
}
