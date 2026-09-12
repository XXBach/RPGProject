using System;
using UnityEngine;

/// <summary>
/// Kênh sự kiện tĩnh cho việc yêu cầu di chuyển camera.
/// Bất kỳ hệ thống nào (pathfinding, player movement, cutscene, item spawn...)
/// chỉ cần gọi CameraSignals.RequestMove(...) mà không cần tham chiếu trực tiếp
/// tới camera controller nào cả.
/// </summary>
public static class CameraSignals
{
    /// <summary>
    /// Payload: vị trí world muốn camera di chuyển tới, và có di chuyển mượt hay không.
    /// </summary>
    public static event Action<Vector3, bool> MoveRequested;

    public static void RequestMove(Vector3 worldPosition, bool smooth = true)
    {
        MoveRequested?.Invoke(worldPosition, smooth);
    }
}