using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.TextCore.Text;

public class GridTesting : MonoBehaviour
{
    [SerializeField]private Font _font;
    [SerializeField]private InputActionAsset _inputActionAsset;
    [SerializeField]private HeatMapBoolVisual _heatMapBoolVisual;
    private InputAction _selectAction;
    private Grid<HeatMapGridObject> _gridTest;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        Vector3 WorldPoint = Camera.main.ViewportToWorldPoint(Vector3.zero);
        WorldPoint.z = 0;
        _gridTest = new Grid<HeatMapGridObject> (100, 60, 1f, _font, WorldPoint, (Grid<HeatMapGridObject> g, int x, int y) => new HeatMapGridObject(g, x, y));
        _selectAction = _inputActionAsset.FindAction("Select");
        //Debug.Log($"{_gridTest}");
        //_heatMapBoolVisual.SetGrid(_gridTest);
    }

    // Update is called once per frame
    void Update()
    {
        if (_selectAction.WasPressedThisFrame())
        {
            Vector2 mousePosition = Mouse.current.position.ReadValue();
            //int currentCellValue = _gridTest.GetCellValue(Camera.main.ScreenToWorldPoint(mousePosition));
            //_gridTest.SetCellValue(Camera.main.ScreenToWorldPoint(mousePosition), true);
            //_gridTest.AddValue(Camera.main.ScreenToWorldPoint(mousePosition), 100, 5, 20);
            //Debug.Log(_gridTest.GetCellValue(Camera.main.ScreenToWorldPoint(mousePosition)).ToString());
            HeatMapGridObject heatMapGridObject = _gridTest.GetGridObject(Camera.main.ScreenToWorldPoint(mousePosition));
            if (heatMapGridObject != null) {
                heatMapGridObject.AddValue(5);
            }
        }
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
public class HeatMapGridObject // A GridCell
{
    private const int MIN = 0;
    private const int MAX = 100;    
    public int Value { get; set; }
    private Grid<HeatMapGridObject> _grid;
    private int x;
    private int y;
    public HeatMapGridObject(Grid<HeatMapGridObject> grid, int x, int y) {
        this.Value = 0;
        this._grid = grid;
        this.x = x;
        this.y = y;
    }
    public void AddValue(int addValue)
    {
        Value += addValue;
        Value = Mathf.Clamp(Value, MIN, MAX);
        _grid.TriggerGridObjectChanged(x,y);
    }
    public float GetValueNormalized() { return (float)Value / MAX; }
    public override string ToString()
    {
        return Value.ToString();
    }
}
