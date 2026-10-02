using DG.Tweening;
using TMPro;
using UnityEngine;

/// <summary>
/// Banner hiển thị "ai đang tấn công" và "dùng skill gì" trong Combat Scene.
/// Gắn lên object có CanvasGroup. Banner ẩn bằng alpha (không SetActive) để tween mượt.
/// </summary>
[RequireComponent(typeof(CanvasGroup))]
public class CombatInfoBanner : MonoBehaviour
{
    [Header("Text References")]
    [SerializeField] private TextMeshProUGUI _attackerNameText;
    [SerializeField] private TextMeshProUGUI _skillNameText;

    [Header("Animation")]
    [SerializeField] private float _fadeInDuration = 0.2f;
    [SerializeField] private float _fadeOutDuration = 0.25f;
    [SerializeField] private float _slideDistance = 30f; // trượt từ trên xuống

    private CanvasGroup _canvasGroup;
    private RectTransform _rect;
    private Vector2 _originalPos;
    private Sequence _sequence;

    private void Awake()
    {
        _canvasGroup = GetComponent<CanvasGroup>();
        _rect = transform as RectTransform;
        _originalPos = _rect.anchoredPosition;

        _canvasGroup.alpha = 0f;
        _canvasGroup.blocksRaycasts = false;
        _canvasGroup.interactable = false;
    }

    public void Show(string attackerName, string skillName)
    {
        _attackerNameText.text = attackerName;
        _skillNameText.text = skillName;

        _sequence?.Kill();
        _rect.anchoredPosition = _originalPos + Vector2.up * _slideDistance;
        _canvasGroup.alpha = 0f;

        _sequence = DOTween.Sequence().SetLink(gameObject);
        _sequence.Append(_canvasGroup.DOFade(1f, _fadeInDuration));
        _sequence.Join(_rect.DOAnchorPos(_originalPos, _fadeInDuration).SetEase(Ease.OutCubic));
    }

    public void Hide()
    {
        _sequence?.Kill();
        _sequence = DOTween.Sequence().SetLink(gameObject);
        _sequence.Append(_canvasGroup.DOFade(0f, _fadeOutDuration));
    }

    private void OnDisable()
    {
        _sequence?.Kill();
    }
}