using System;
using UnityEngine;

public class HeatMapBoolVisual : MonoBehaviour
{
    private Grid<bool> _grid;
    private Mesh _mesh;
    private Boolean _isUpdateMesh = false;
    public void SetGrid(Grid<bool> grid)
    {
        _mesh = new Mesh();
        GetComponent<MeshFilter>().mesh = _mesh;
        Debug.Log("1");
        _grid = grid;
        Debug.Log("2");
        _grid.OnGridValueChanged.AddListener(Grid_OnGridChanged);
        Debug.Log("3");
        UpdateHeatMapVisual();
        Debug.Log("4");
    }
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        if(_mesh == null)
        {
            _mesh = new Mesh();
            GetComponent<MeshFilter>().mesh = _mesh;
        }
        _isUpdateMesh = false;
    }

    private void Grid_OnGridChanged(Grid<bool>.OnGridValueChangedEventArgs e)
    {
        Debug.Log("event fired");
        _isUpdateMesh = true;
    }
    // Update is called once per frame
    private void LateUpdate()
    {
        if (_isUpdateMesh)
        {
            _isUpdateMesh = false;
            UpdateHeatMapVisual();
        }
    }
    public void UpdateHeatMapVisual()
    {
        MeshUtils.CreateEmptyMeshArrays(_grid.Width * _grid.Height, out Vector3[] vertices, out Vector2[] uv, out int[] triangles);

        for (int i = 0; i < _grid.Width; i++)
        {
            for (int j = 0; j < _grid.Height; j++)
            {
                int index = i * _grid.Height + j;
                Vector3 quadSize = new Vector3(1, 1) * _grid.CellSize;

                bool _gridValue = _grid.GetGridObject(i, j);
                float _gridValueNormalized = _gridValue ? 1f : 0f;
                Vector2 gridValueUV = new Vector2(_gridValueNormalized, 0);
                MeshUtils.AddToMeshArrays(vertices, uv, triangles, index, _grid.GetCellWorldPosition(i, j) + quadSize * 0.5f, 0f, quadSize, gridValueUV, gridValueUV);
            }
        }
        Debug.Log($"Mesh {_mesh}");
        _mesh.vertices = vertices;
        _mesh.uv = uv;
        _mesh.triangles = triangles;
    }
    private void OnDestroy()
    {
        if (_grid != null)
        {
            _grid.OnGridValueChanged.RemoveListener(Grid_OnGridChanged);
        }
    }
}
