using UnityEngine;

public class MeshTest : MonoBehaviour
{
    private int _width = 5;
    private int _height = 5;
    private int _tileSize = 1;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        CreateRectangularTile();
    }
    public void CreateRectangularTile()
    {
        Mesh mesh = new Mesh();
        Vector3[] vertices = new Vector3[4 * _width * _height];
        Vector2[] uvs = new Vector2[4 * _width * _height];
        int[] triangles = new int[6 * _width * _height];

        for(int i = 0; i < _width; i++)
        {
            for (int j = 0; j < _height; j++)
            {
                int index = i * _height + j;

                vertices[index * 4 + 0] = new Vector3(_tileSize * i, _tileSize * j, 0);
                vertices[index * 4 + 1] = new Vector3(_tileSize * i, _tileSize * (j + 1), 0);
                vertices[index * 4 + 2] = new Vector3(_tileSize * (i + 1), _tileSize * j, 0);
                vertices[index * 4 + 3] = new Vector3(_tileSize * (i + 1), _tileSize * (j + 1), 0);

                uvs[index * 4 + 0] = new Vector2(0, 0);
                uvs[index * 4 + 1] = new Vector2(0, 1);
                uvs[index * 4 + 2] = new Vector2(1, 0);
                uvs[index * 4 + 3] = new Vector2(1, 1);

                triangles[index * 6 + 0] = index * 4 + 0;
                triangles[index * 6 + 1] = index * 4 + 1;
                triangles[index * 6 + 2] = index * 4 + 2;

                triangles[index * 6 + 3] = index * 4 + 1;
                triangles[index * 6 + 4] = index * 4 + 3;
                triangles[index * 6 + 5] = index * 4 + 2;
            }
        }
        mesh.vertices = vertices;
        mesh.uv = uvs;
        mesh.triangles = triangles;
        GetComponent<MeshFilter>().mesh = mesh;
    }
}
