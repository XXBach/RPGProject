using DG.Tweening;
using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// Gắn lên root của Stats Menu (object có Image nền + các con Image/Text).
/// - Khi chuột lại gần/vào vùng panel: hạ alpha xuống _hoverAlpha.
/// - Khi chuột rời đi: trả alpha về _normalAlpha.
/// - Panel KHÔNG BAO GIỜ chặn raycast (blocksRaycasts = false), nên UI/nút nằm bên dưới luôn bấm được.
///   Vì vậy việc phát hiện chuột được làm thủ công bằng toạ độ, không dùng IPointerEnter.
/// </summary>
[RequireComponent(typeof(CanvasGroup))]
public class StatsMenuFader : MonoBehaviour
{
    [SerializeField] private RectTransform _panel;   // để trống = chính object này
    [SerializeField] private Canvas _canvas;         // để trống = tự tìm Canvas cha

    [Header("Fade")]
    [SerializeField, Range(0f, 1f)] private float _normalAlpha = 1f;
    [SerializeField, Range(0f, 1f)] private float _hoverAlpha = 0.25f;
    [SerializeField] private float _fadeDuration = 0.15f;

    [Header("Detect")]
    [Tooltip("Mở rộng vùng nhận biết ra ngoài panel (đơn vị canvas) để mờ đi trước khi chuột chạm vào")]
    [SerializeField] private float _detectPadding = 40f;

    private CanvasGroup _canvasGroup;
    private Camera _uiCamera;
    private bool _isFaded = false;

    private void Awake()
    {
        _canvasGroup = GetComponent<CanvasGroup>();
        if (_panel == null) _panel = transform as RectTransform;
        if (_canvas == null) _canvas = GetComponentInParent<Canvas>();

        // Panel chỉ để hiển thị, không cần nhận chuột
        _canvasGroup.blocksRaycasts = false;
        _canvasGroup.interactable = false;
        _canvasGroup.alpha = _normalAlpha;

        // Screen Space - Overlay thì camera = null
        _uiCamera = _canvas.renderMode == RenderMode.ScreenSpaceOverlay ? null : _canvas.worldCamera;
    }

    private void Update()
    {
        if (Mouse.current == null) return;

        bool isMouseNear = IsMouseNearPanel(Mouse.current.position.ReadValue());
        if (isMouseNear == _isFaded) return; // trạng thái chưa đổi -> không tạo tween mới mỗi frame

        _isFaded = isMouseNear;
        _canvasGroup.DOKill();
        _canvasGroup.DOFade(_isFaded ? _hoverAlpha : _normalAlpha, _fadeDuration)
                    .SetLink(gameObject);
    }

    private bool IsMouseNearPanel(Vector2 screenPoint)
    {
        if (!RectTransformUtility.ScreenPointToLocalPointInRectangle(_panel, screenPoint, _uiCamera, out Vector2 localPoint))
            return false;

        Rect rect = _panel.rect;
        rect.xMin -= _detectPadding;
        rect.xMax += _detectPadding;
        rect.yMin -= _detectPadding;
        rect.yMax += _detectPadding;
        return rect.Contains(localPoint);
    }

    private void OnDisable()
    {
        _canvasGroup.DOKill();
    }
}
