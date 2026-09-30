using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;
using UnityEngine.Tilemaps;

public enum ObstacleType
{
    None = 0,
    Blocking = 1,
}

[System.Serializable]
public class ObstacleLayerData
{
    public Tilemap TileMap;
    public ObstacleType ObstacleType;
}
[DefaultExecutionOrder(-200)]
public class GridSetup : MonoBehaviour
{
    public static Grid<PathNode> Grid { get; set; }
    [SerializeField] private Font _font;
    [SerializeField] private List<ObstacleLayerData> obstacleLayers;
    [SerializeField] private int width, height;
    [SerializeField] private float cellSize;
    [SerializeField] private Transform PathFindingDebugObjectPrefab;
    [SerializeField] private bool isShowDebug = false;
    [SerializeField] private bool showGridInEditor = false;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    private void Awake()
    {
        Grid = new Grid<PathNode>(width, height, cellSize, _font, Vector3.zero, (Grid<PathNode> g, int x, int y) => CreatePathNode(g, x, y));
    }
    private void Start()
    {
        if (isShowDebug) SetupGridDebugObjects();
    }
    private void SetupGridDebugObjects()
    {
        for (int x = 0; x < Grid.Width; x++)
        {
            for (int y = 0; y < Grid.Height; y++)
            {
                Transform gridDebugObjectTransform = Instantiate(
                    PathFindingDebugObjectPrefab,
                    Grid.GetCellWorldPosition(x, y) + new Vector3(Grid.CellSize, Grid.CellSize) * .5f,
                    Quaternion.identity
                );
                gridDebugObjectTransform.gameObject.GetComponent<PathNodeVisualize>()
                    .PathNode = Grid.GetGridObject(x, y);
                Debug.DrawLine(Grid.GetCellWorldPosition(x, y), Grid.GetCellWorldPosition(x, y + 1), Color.white, 100f);
                Debug.DrawLine(Grid.GetCellWorldPosition(x, y), Grid.GetCellWorldPosition(x + 1, y), Color.white, 100f);
            }
        }
        Debug.DrawLine(Grid.GetCellWorldPosition(0, Grid.Height), Grid.GetCellWorldPosition(Grid.Width, Grid.Height), Color.white, 100f);
        Debug.DrawLine(Grid.GetCellWorldPosition(Grid.Width, 0), Grid.GetCellWorldPosition(Grid.Width, Grid.Height), Color.white, 100f);
    }
    private PathNode CreatePathNode(Grid<PathNode> grid, int x, int y)
    {
        Vector3 worldPos = grid.GetCellWorldPosition(x, y);
        NodeState state = NodeState.Walkable;

        foreach (var layer in obstacleLayers)
        {
            Vector3Int cellPos = layer.TileMap.WorldToCell(worldPos);
            if (!layer.TileMap.HasTile(cellPos)) continue;

            // Blocking luôn có ưu tiên cao nhất, nếu đã Blocked thì không cần check tiếp
            if (layer.ObstacleType == ObstacleType.Blocking)
            {
                state = NodeState.Blocked;
                break;
            }
            else
            {
                state = NodeState.Walkable;
            }
        }

        return new PathNode(grid, x, y) { State = state };
    }
    public void UpdateNodeState(Vector2Int FinalPosition, NodeState newState)
    {
        Grid.GetGridObject(FinalPosition.x, FinalPosition.y).State = newState;
    }
    private void OnDrawGizmos()
    {
        if (!showGridInEditor) return;

        Gizmos.color = Color.white;

        for (int x = 0; x <= width; x++)
        {
            Vector3 start = Vector3.zero + new Vector3(x, 0) * cellSize;
            Vector3 end = Vector3.zero + new Vector3(x, height) * cellSize;
            Gizmos.DrawLine(start, end);
        }

        for (int y = 0; y <= height; y++)
        {
            Vector3 start = Vector3.zero + new Vector3(0, y) * cellSize;
            Vector3 end = Vector3.zero + new Vector3(width, y) * cellSize;
            Gizmos.DrawLine(start, end);
        }
    }
    private void OnEnable() { MovementSignal.MovementEnded += HandleMovementEnd; }
    private void OnDisable() { MovementSignal.MovementEnded -= HandleMovementEnd; }

    private void HandleMovementEnd(OnMovementEndArgs args)
    {
        PathNode previousNode = Grid.GetGridObject(args.PreviousPosition.x, args.PreviousPosition.y);
        if (previousNode != null && previousNode.State == NodeState.BlockedByCharacter)
            previousNode.State = NodeState.Walkable;
        UpdateNodeState(args.FinalPosition, args.FinalNodeState);
    }

}
