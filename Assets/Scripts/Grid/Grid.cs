using UnityEngine;
using UnityEngine.Events;
using System;
public class Grid<TGridObject>
{
    public const int MAX_CELL_VALUE = 100;
    public const int MIN_CELL_VALUE = 0;

    public UnityEvent<OnGridValueChangedEventArgs> OnGridValueChanged;
    public class OnGridValueChangedEventArgs
    {
        public int x { get; set; }
        public int y { get; set; }

        public OnGridValueChangedEventArgs(int x, int y)
        {
            this.x = x;
            this.y = y;
        }
    }

    private int _width;
    private int _height;
    private float _cellSize;
    private TGridObject[,] _gridArray;
    private Vector3 _originPosition; // The origin position of the grid in world space
    private TextMesh[,] _DebugtextMesh;
    public int Width => _width;
    public int Height => _height;
    public float CellSize => _cellSize;
    public Grid(int width, int height, float cellSize, Font font, Vector3 originPosition, Func<Grid<TGridObject>, int, int, TGridObject> createGridObject)
    {
        _width = width;
        _height = height;
        _originPosition = originPosition;
        _cellSize = cellSize;
        _gridArray = new TGridObject[width, height];
        _DebugtextMesh = new TextMesh[width, height];

        OnGridValueChanged = new UnityEvent<OnGridValueChangedEventArgs>();

        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                _gridArray[x, y] = createGridObject(this, x, y);
            }
        }

        bool showdebug = false;
        if (showdebug)
        {
            for (int i = 0; i < width; i++)
            {
                for (int j = 0; j < height; j++)
                {
                    _DebugtextMesh[i, j] = TextUtils.CreateWorldText(null, _gridArray[i, j]?.ToString(), GetCellWorldPosition(i, j) + new Vector3(cellSize, cellSize) * 0.5f, 50, Color.white, TextAnchor.MiddleCenter, TextAlignment.Center, font);
                    Debug.DrawLine(GetCellWorldPosition(i, j), GetCellWorldPosition(i, j + 1), Color.white, 100f);
                    Debug.DrawLine(GetCellWorldPosition(i, j), GetCellWorldPosition(i + 1, j), Color.white, 100f);
                }
            }
            Debug.DrawLine(GetCellWorldPosition(0, height), GetCellWorldPosition(width, height), Color.white, 100f);
            Debug.DrawLine(GetCellWorldPosition(width, 0), GetCellWorldPosition(width, height), Color.white, 100f);
        }
    }
    public Vector3 GetCellWorldPosition(int x, int y)
    {
        return _originPosition + new Vector3(x, y) * _cellSize;
    }
    public Vector2Int GetGridPosition(Vector3 worldPosition)
    {
        int x = Mathf.FloorToInt((worldPosition.x - _originPosition.x) / _cellSize);
        int y = Mathf.FloorToInt((worldPosition.y - _originPosition.y) / _cellSize);
        if (x < 0 || y < 0 || x >= _width || y >= _height)
        {
            return new Vector2Int(-1, -1);
        }
        return new Vector2Int(x, y);
    }
    public void SetGridObject(int x, int y, TGridObject value)
    {
        if (x >= 0 && y >= 0 && x < _width && y < _height)
        {
            _gridArray[x, y] = value;
            _DebugtextMesh[x, y].text = _gridArray[x, y]?.ToString();
            OnGridValueChanged?.Invoke(new OnGridValueChangedEventArgs(x, y)); //Không thể dùng cho custom object được;
        }
    }
    public void SetGridObject(Vector3 worldPosition, TGridObject value)
    {
        Vector2Int gridPosition = GetGridPosition(worldPosition);
        SetGridObject(gridPosition.x, gridPosition.y, value);
    }
    public void TriggerGridObjectChanged(int x, int y)
    {
        Debug.Log("Event Fired");
        OnGridValueChanged?.Invoke(new OnGridValueChangedEventArgs(x, y));
    }
    public static Vector3 GetMousePositionWithZ()
    {
        Vector3 mousePosition = Input.mousePosition;
        mousePosition.z = Camera.main.WorldToScreenPoint(Vector3.zero).z;
        return Camera.main.ScreenToWorldPoint(mousePosition);
    }
    public TGridObject GetGridObject(int x, int y)
    {
        if (x >= 0 && y >= 0 && x < _width && y < _height)
        {
            return _gridArray[x, y];
        }
        else
        {
            return default(TGridObject); // Return default ( int -1, custom type null ) if the cell is out of bounds
        }
    }
    public TGridObject GetGridObject(Vector3 worldPosition)
    {
        Vector2Int gridPosition = GetGridPosition(worldPosition);
        return GetGridObject(gridPosition.x, gridPosition.y);
    }
}
