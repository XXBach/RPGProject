using System;
using DG.Tweening;
using TMPro;
using UnityEngine;

/// <summary>
/// 1 con số damage: phóng to (pop) -> nảy lên theo hình vòng cung -> dừng 1 chút -> mờ dần.
/// Gắn lên prefab có component TextMeshPro (bản 3D/world, KHÔNG phải TextMeshProUGUI).
/// </summary>
public class DamagePopup : MonoBehaviour
{
    [SerializeField] private TextMeshPro _text;

    [Header("Motion")]
    [SerializeField] private float _popDuration = 0.12f;
    [SerializeField] private float _jumpPower = 0.8f;       // độ cao của cú nảy
    [SerializeField] private float _jumpDistanceY = 0.6f;   // điểm rơi cao hơn điểm xuất phát bao nhiêu
    [SerializeField] private float _driftX = 0.5f;          // lệch ngang tối đa để các số không chồng lên nhau
    [SerializeField] private float _jumpDuration = 0.5f;
    [SerializeField] private float _holdTime = 0.2f;
    [SerializeField] private float _fadeDuration = 0.25f;

    [Header("Style")]
    [SerializeField] private float _finalBlowScale = 1.5f;
    [SerializeField] private int _sortingOrder = 100;       // đảm bảo nằm trên sprite nhân vật

    private Sequence _sequence;

    private void Awake()
    {
        if (_text == null) _text = GetComponentInChildren<TextMeshPro>();
        _text.sortingOrder = _sortingOrder;
    }

    public void Play(Vector3 startPosition, string content, Color color, bool isFinalBlow, Action<DamagePopup> onFinished)
    {
        _sequence?.Kill();

        float targetScale = isFinalBlow ? _finalBlowScale : 1f;
        float drift = UnityEngine.Random.Range(-_driftX, _driftX);

        transform.position = startPosition;
        transform.localScale = Vector3.zero;
        _text.text = content;
        _text.color = color;
        gameObject.SetActive(true);

        Vector3 landPosition = startPosition + new Vector3(drift, _jumpDistanceY, 0f);

        _sequence = DOTween.Sequence().SetLink(gameObject);

        // 1) Pop to ra với overshoot + đồng thời nảy theo vòng cung
        _sequence.Append(transform.DOScale(targetScale, _popDuration).SetEase(Ease.OutBack));
        _sequence.Join(transform.DOJump(landPosition, _jumpPower, 1, _jumpDuration).SetEase(Ease.Linear));

        // 2) Giữ lại cho người chơi kịp đọc
        _sequence.AppendInterval(_holdTime);

        // 3) Mờ dần + trôi nhẹ lên trên
        _sequence.Append(_text.DOFade(0f, _fadeDuration));
        _sequence.Join(transform.DOMoveY(landPosition.y + 0.3f, _fadeDuration));

        _sequence.OnComplete(() =>
        {
            gameObject.SetActive(false);
            onFinished?.Invoke(this);
        });
    }

    private void OnDisable()
    {
        _sequence?.Kill();
    }
}
