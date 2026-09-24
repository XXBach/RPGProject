using UnityEditor;
using UnityEngine;

[InitializeOnLoad]
public static class MouseGridDebugger
{
    static MouseGridDebugger()
    {
        SceneView.duringSceneGui += OnSceneGUI;
    }

    private static void OnSceneGUI(SceneView sceneView)
    {
        Event e = Event.current;

        if (e.type != EventType.MouseMove &&
            e.type != EventType.MouseDrag)
        {
            return;
        }

        // Vị trí chuột trong Scene View
        Vector2 mousePosition = e.mousePosition;

        // Scene View GUI Position -> World Ray
        Ray ray = HandleUtility.GUIPointToWorldRay(mousePosition);

        // Game 2D: Grid nằm trên mặt phẳng Z = 0
        Plane gridPlane = new Plane(
            Vector3.forward,
            Vector3.zero
        );

        if (!gridPlane.Raycast(ray, out float distance))
            return;

        Vector3 worldPosition = ray.GetPoint(distance);

        // Tìm GridSetup trong Scene
        GridSetup gridSetup = Object.FindFirstObjectByType<GridSetup>();

        if (gridSetup == null)
            return;

        // Vì Grid của bạn bắt đầu tại (0,0)
        // và cellSize được dùng để tạo Grid
        int x = Mathf.FloorToInt(
            worldPosition.x / 1f
        );

        int y = Mathf.FloorToInt(
            worldPosition.y / 1f
        );

        Debug.Log(
            $"Mouse World: {worldPosition} | Cell: ({x}, {y})"
        );
    }
}