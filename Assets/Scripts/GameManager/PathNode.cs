using UnityEngine;
using System;
public enum NodeState
{
    Walkable = 0,
    Blocked = 1,
    Attackable = 2,
    BlockedByCharacter = 3,
}
public class PathNode
{
    public NodeState State { get; set; }
    public int XCoordinate { get; set; }
    public int YCoordinate { get; set; }


    public int GCost { get; set; }
    public int HCost { get; set; }
    public int FCost { get; set; }

    private Grid<PathNode> _grid;
    public PathNode CameFromNode { get; set; }
    public PathNode (Grid<PathNode> grid, int xCoordinate, int yCoordinate)
    {
        _grid = grid;
        XCoordinate = xCoordinate;
        YCoordinate = yCoordinate;
    }
    public void CalculateFCost()
    {
        this.FCost = this.GCost + this.HCost;
    }
    public override string ToString()
    {
        return XCoordinate + "," + YCoordinate;
    }
}
