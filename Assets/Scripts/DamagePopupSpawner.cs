using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Đặt 1 cái trong Combat Scene. CombatSceneManager gọi Spawn(...) mỗi lần có 1 hit.
/// Dùng pool để không Instantiate/Destroy liên tục.
/// </summary>
public class DamagePopupSpawner : MonoBehaviour
{
    [SerializeField] private DamagePopup _popupPrefab;

    [Header("Colors")]
    [SerializeField] private Color _normalColor = Color.white;
    [SerializeField] private Color _finalBlowColor = new Color(1f, 0.85f, 0.2f);
    [SerializeField] private Color _zeroDamageColor = new Color(0.7f, 0.7f, 0.7f);

    private readonly Queue<DamagePopup> _pool = new Queue<DamagePopup>();

    public void Spawn(Vector3 worldPosition, int amount, bool isFinalBlow)
    {
        DamagePopup popup = GetFromPool();

        Color color = amount <= 0 ? _zeroDamageColor
                    : isFinalBlow ? _finalBlowColor
                    : _normalColor;

        popup.Play(worldPosition, amount.ToString(), color, isFinalBlow, ReturnToPool);
    }

    private DamagePopup GetFromPool()
    {
        if (_pool.Count > 0) return _pool.Dequeue();
        return Instantiate(_popupPrefab, transform);
    }

    private void ReturnToPool(DamagePopup popup)
    {
        _pool.Enqueue(popup);
    }
}
