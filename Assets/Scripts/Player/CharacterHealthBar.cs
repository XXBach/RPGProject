using UnityEngine;
using UnityEngine.UI;

public class CharacterHealthBar : MonoBehaviour
{
    [SerializeField] private Image _fillImage; // kéo Image "Fill" vào đây

    private ICharacter _character;
    private TurnManager _turnManager;
    private bool _isDead = false;

    private void Awake()
    {
        // Vì script này là con của Player/Enemy, lấy thẳng ICharacter ở object cha
        _character = GetComponentInParent<ICharacter>();
    }

    private void Start()
    {
        _turnManager = FindAnyObjectByType<TurnManager>();
    }

    private void Update()
    {
        if (_character == null || _isDead) return;


        int currentHP = _character.CurrentDatas.CurrentHealth;
        int maxHP = _character.CurrentDatas.MaxHealth;
        Debug.Log($"HP: {currentHP}/{maxHP}");
        if (maxHP <= 0) return;

        _fillImage.fillAmount = Mathf.Clamp01((float)currentHP / maxHP);


        if (currentHP <= 0)
        {
            HandleCharacterDeath();
        }
    }

    private void HandleCharacterDeath()
    {
        _isDead = true;

        // Trả lại trạng thái Walkable cho ô lưới character đang đứng
        Vector2Int gridPos = GridSetup.Grid.GetGridPosition(_character.GetCharWorldPosition());
        PathNode occupiedNode = GridSetup.Grid.GetGridObject(gridPos.x, gridPos.y);
        if (occupiedNode != null)
        {
            occupiedNode.State = NodeState.Walkable;
        }

        if (_turnManager != null)
        {
            _turnManager.RemoveCharacter(_character);
        }

        Destroy(_character.gameObject);
    }
}
