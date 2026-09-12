using NUnit.Framework;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.InputSystem;

public class PathFindingTesting : MonoBehaviour
{
    [SerializeField] private Font _font;
    [SerializeField] private InputActionAsset _inputActionAsset;
    [SerializeField] private Transform PathFindingDebugObjectPrefab;
    private InputAction _selectAction;
    private PathFinding _pathFinding;
    private List<PathNode> path;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        _pathFinding = new PathFinding(GridSetup.Grid);
        SetupGridDebugObjects(_pathFinding.Grid);
        _selectAction = _inputActionAsset.FindAction("Select");
    }

    // Update is called once per frame
    void Update()
    {
        if (_selectAction.WasPressedThisFrame())
        {
            Vector2 mousePosition = Mouse.current.position.ReadValue();
            Vector3 mouseWorldPosition = Camera.main.ScreenToWorldPoint(mousePosition);
            Vector2Int gridPosition = new Vector2Int();
            gridPosition = _pathFinding.Grid.GetGridPosition(mouseWorldPosition);
            path = _pathFinding.FindPath(gridPosition.x, gridPosition.y);
            CameraSignals.RequestMove(mouseWorldPosition, smooth: true);
        }
    }
    private void OnDrawGizmos()
    {
        if (path == null) return;

        Gizmos.color = Color.green;
        for (int i = 0; i < path.Count - 1; i++)
        {
            Vector3 from = new Vector3(path[i].XCoordinate, path[i].YCoordinate) + Vector3.one * 0.5f;
            Vector3 to = new Vector3(path[i + 1].XCoordinate, path[i + 1].YCoordinate) + Vector3.one * 0.5f;
            Gizmos.DrawLine(from, to);
        }
    }
    private void SetupGridDebugObjects(Grid<PathNode> grid)
    {
        for (int x = 0; x < grid.Width; x++)
        {
            for (int y = 0; y < grid.Height; y++)
            {
                Transform gridDebugObjectTransform = Instantiate(
                    PathFindingDebugObjectPrefab,
                    grid.GetCellWorldPosition(x, y) + new Vector3(grid.CellSize, grid.CellSize) * .5f,
                    Quaternion.identity
                );
                gridDebugObjectTransform.gameObject.GetComponent<PathNodeVisualize>()
                    .PathNode = grid.GetGridObject(x,y) ;
                Debug.DrawLine(grid.GetCellWorldPosition(x, y), grid.GetCellWorldPosition(x, y + 1), Color.white, 100f);
                Debug.DrawLine(grid.GetCellWorldPosition(x, y), grid.GetCellWorldPosition(x + 1, y), Color.white, 100f);
            }
        }
        Debug.DrawLine(grid.GetCellWorldPosition(0, grid.Height), grid.GetCellWorldPosition(grid.Width, grid.Height), Color.white, 100f);
        Debug.DrawLine(grid.GetCellWorldPosition(grid.Width, 0), grid.GetCellWorldPosition(grid.Width, grid.Height), Color.white, 100f);
    }
    private void OnEnable()
    {
        _inputActionAsset.FindActionMap("Player").Enable();
    }
    private void OnDisable()
    {
        _inputActionAsset.FindActionMap("Player").Disable();
    }
}
