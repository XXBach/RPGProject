using NUnit.Framework;
using Unity.VisualScripting;
using UnityEngine;
using System.Collections.Generic;
public enum PathfindingMode
{
    Movement, // Vật cản tĩnh (Blocked) VÀ character (BlockedByCharacter) đều chặn đường đi
    Attack    // Chỉ vật cản tĩnh (Blocked) chặn; character đứng trên ô không chặn tầm đánh/AOE
}
public class PathFinding
{
    private const int MOVE_STRAIGHT_COST = 10;
    private const int MOVE_DIAGONAL_COST = 14;

    private Grid<PathNode> _grid;
    public Grid<PathNode> Grid {
        get { return this._grid; }
        set { this._grid = value; } }
    private List<PathNode> _openList;
    private List<PathNode> _closedList;
    public PathFinding(Grid<PathNode> grid)
    {
        _grid = grid;
    }
    public List<PathNode> GetNeighborList(PathNode currentNode, bool canMoveDiagonal)
    {
        List<PathNode> neighborList = new List<PathNode>();

        // 4 hướng thẳng - luôn có, dùng cho cả movement thường lẫn action
        if (currentNode.XCoordinate - 1 >= 0) neighborList.Add(GetNode(currentNode.XCoordinate - 1, currentNode.YCoordinate));
        if (currentNode.XCoordinate + 1 < _grid.Width) neighborList.Add(GetNode(currentNode.XCoordinate + 1, currentNode.YCoordinate));
        if (currentNode.YCoordinate - 1 >= 0) neighborList.Add(GetNode(currentNode.XCoordinate, currentNode.YCoordinate - 1));
        if (currentNode.YCoordinate + 1 < _grid.Height) neighborList.Add(GetNode(currentNode.XCoordinate, currentNode.YCoordinate + 1));

        if (!canMoveDiagonal) return neighborList;

        // 4 hướng chéo - chỉ thêm khi action cho phép
        if (currentNode.XCoordinate - 1 >= 0)
        {
            if (currentNode.YCoordinate - 1 >= 0) neighborList.Add(GetNode(currentNode.XCoordinate - 1, currentNode.YCoordinate - 1));
            if (currentNode.YCoordinate + 1 < _grid.Height) neighborList.Add(GetNode(currentNode.XCoordinate - 1, currentNode.YCoordinate + 1));
        }
        if (currentNode.XCoordinate + 1 < _grid.Width)
        {
            if (currentNode.YCoordinate - 1 >= 0) neighborList.Add(GetNode(currentNode.XCoordinate + 1, currentNode.YCoordinate - 1));
            if (currentNode.YCoordinate + 1 < _grid.Height) neighborList.Add(GetNode(currentNode.XCoordinate + 1, currentNode.YCoordinate + 1));
        }

        return neighborList;
    }
    private PathNode GetNode(int x, int y)
    {
        return _grid.GetGridObject(x, y);
    }
    public List<PathNode> FindPath(int destinationx, int destinationy)
    {
        PathNode endNode = GetNode(destinationx, destinationy);
        if (endNode == null) return null; // FIX: tọa độ ngoài lưới (vd sentinel -100,-100) -> không có path, không crash
        return CalculatePath(endNode);
    }
    public List<PathNode> CalculatePath(PathNode endNode)
    {
        List<PathNode> pathList = new List<PathNode>();
        pathList.Add(endNode);
        PathNode currentNode = endNode;
        while (currentNode.CameFromNode != null) {
            pathList.Add((PathNode)currentNode.CameFromNode);
            currentNode = currentNode.CameFromNode;
        }
        pathList.Reverse();
        return pathList;
    }
    private int CalculateDistanceCost(PathNode a, PathNode b) {
        int xDistance = Mathf.Abs(a.XCoordinate - b.XCoordinate);
        int yDistance = Mathf.Abs(a.YCoordinate - b.YCoordinate);
        int remaining = Mathf.Abs(xDistance - yDistance);
        return MOVE_DIAGONAL_COST * Mathf.Min(xDistance, yDistance) + MOVE_STRAIGHT_COST * remaining;
    }
    public List<PathNode> GetReachableNodes( int startX, int startY, int movementRange, bool canMoveDiagonal = false,  PathfindingMode mode = PathfindingMode.Movement) // mặc định giữ hành vi cũ cho các chỗ gọi movement
    {
        PathNode startNode = _grid.GetGridObject(startX, startY);

        _openList = new List<PathNode> { startNode };
        _closedList = new List<PathNode>();

        for (int x = 0; x < _grid.Width; x++)
        {
            for (int y = 0; y < _grid.Height; y++)
            {
                PathNode pathNode = _grid.GetGridObject(x, y);
                pathNode.GCost = int.MaxValue;
                pathNode.CameFromNode = null;
            }
        }

        startNode.GCost = 0;

        List<PathNode> reachableNodes = new List<PathNode>();

        while (_openList.Count > 0)
        {
            PathNode currentNode = getLowestGCostNode(_openList);
            _openList.Remove(currentNode);
            _closedList.Add(currentNode);
            reachableNodes.Add(currentNode);

            foreach (PathNode neighborNode in GetNeighborList(currentNode, canMoveDiagonal))
            {
                if (_closedList.Contains(neighborNode)) continue;

                if (IsBlockedForMode(neighborNode.State, mode)) // FIX: dùng hàm chung thay vì check cứng
                {
                    _closedList.Add(neighborNode);
                    continue;
                }

                int tentativeGCost = currentNode.GCost + CalculateDistanceCost(currentNode, neighborNode);
                if (tentativeGCost > movementRange) continue;

                if (tentativeGCost < neighborNode.GCost)
                {
                    neighborNode.CameFromNode = currentNode;
                    neighborNode.GCost = tentativeGCost;
                    if (!_openList.Contains(neighborNode)) _openList.Add(neighborNode);
                }
            }
        }

        reachableNodes.Remove(startNode);
        return reachableNodes;
    }

    public PathNode getLowestGCostNode(List<PathNode> pathNodeList)
    {
        PathNode lowest = pathNodeList[0];
        for (int i = 1; i < pathNodeList.Count; i++)
        {
            if (pathNodeList[i].GCost < lowest.GCost) lowest = pathNodeList[i];
        }
        return lowest;
    }

    private bool IsBlockedForMode(NodeState state, PathfindingMode mode)
    {
        if (state == NodeState.Blocked) return true; // vật cản tĩnh luôn luôn chặn, bất kể mode
        if (state == NodeState.BlockedByCharacter && mode == PathfindingMode.Movement) return true;
        return false; // Attack mode: BlockedByCharacter được coi như Walkable
    }
}
