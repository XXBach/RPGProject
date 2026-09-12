using UnityEngine;
using UnityEngine.InputSystem;

public class GameCameraController : MonoBehaviour
{
    [Header("Camera")]
    [SerializeField] private Camera _camera;

    [Header("Zoom")]
    [SerializeField] private float _zoomSpeed = 5f;
    [SerializeField] private float _minZoom = 2f;
    [SerializeField] private float _maxZoom = 50f;

    [Header("Pan")]
    [SerializeField] private float _panSpeed = 1f;

    [Header("Move To Target")]
    [SerializeField] private float _moveSmoothTime = 0.15f;
    [SerializeField] private float _snapThreshold = 0.01f;

    private Vector3 _moveVelocity;
    private Vector3? _targetPosition = null;
    private bool _smoothMoveEnabled = true;

    private void Awake()
    {
        if (_camera == null)
            _camera = GetComponent<Camera>();
    }
    private void Start()
    {
        float halfHeight = _camera.orthographicSize;
        float halfWidth = halfHeight * _camera.aspect;
        _camera.transform.position = new Vector3(halfWidth, halfHeight, _camera.transform.position.z);
    }
    private void OnEnable()
    {
        // Lắng nghe kênh chung, không quan tâm ai/khi nào phát sự kiện này
        CameraSignals.MoveRequested += HandleMoveRequested;
    }

    private void OnDisable()
    {
        CameraSignals.MoveRequested -= HandleMoveRequested;
    }

    private void LateUpdate()
    {
        HandleZoom();
        HandlePan();
        HandleMoveToTarget();
    }

    // =========================
    // Zoom
    // =========================

    private void HandleZoom()
    {
        float scroll = Mouse.current.scroll.ReadValue().y;
        if (scroll == 0f) return;

        _camera.orthographicSize -= scroll * _zoomSpeed * Time.deltaTime;
        _camera.orthographicSize = Mathf.Clamp(_camera.orthographicSize, _minZoom, _maxZoom);
    }

    // =========================
    // Pan
    // =========================

    private void HandlePan()
    {
        if (!Mouse.current.middleButton.isPressed) return;

        Vector2 mouseDelta = Mouse.current.delta.ReadValue();
        Vector3 movement = new Vector3(mouseDelta.x, mouseDelta.y, 0f);
        transform.position -= movement * _panSpeed * Time.deltaTime;

        // Người dùng tự pan bằng tay -> hủy animation di chuyển tới target đang chạy dở
        _targetPosition = null;
    }

    // =========================
    // Move To (nhận từ CameraSignals, KHÔNG tự đọc input để trigger nữa)
    // =========================

    private void HandleMoveRequested(Vector3 worldPosition, bool smooth)
    {
        MoveTo(worldPosition, smooth);
    }

    private void MoveTo(Vector3 worldPosition, bool smooth)
    {
        worldPosition.z = transform.position.z;
        _smoothMoveEnabled = smooth;

        if (smooth)
        {
            _targetPosition = worldPosition;
        }
        else
        {
            transform.position = worldPosition;
            _targetPosition = null;
        }
    }

    private void HandleMoveToTarget()
    {
        if (_targetPosition == null) return;

        Vector3 target = _targetPosition.Value;

        if (!_smoothMoveEnabled)
        {
            transform.position = target;
            _targetPosition = null;
            return;
        }

        transform.position = Vector3.SmoothDamp(
            transform.position, target, ref _moveVelocity, _moveSmoothTime
        );

        if (Vector3.Distance(transform.position, target) < _snapThreshold)
        {
            transform.position = target;
            _targetPosition = null;
            _moveVelocity = Vector3.zero;
        }
    }
}