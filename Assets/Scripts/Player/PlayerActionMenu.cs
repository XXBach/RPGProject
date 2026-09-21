using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
public enum CharacterState
{
    IDLE = 0,
    CALCULATING = 1,
    MOVE = 2,
    ATTACK = 3,
    ENDTURN = 4
}

/// <summary>
/// Menu hành động (MOVE/ATTACK) dùng chung cho mọi unit trong game turn-based.
/// KHÔNG gán cố định 1 PlayerMOVEment nào trong Inspector.
/// TurnManager (hoặc script điều khiển lượt) sẽ gọi ShowMenuFor(unit) mỗi khi
/// tới lượt 1 unit, panel sẽ tự bám theo vị trí unit đó trên màn hình.
/// </summary>
public class PlayerActionMenu : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private RectTransform _menuPanel;
    [SerializeField] private PlayerAttackMenu _attackSubMenu;
    [SerializeField] private Button _moveButton;
    [SerializeField] private Button _attackButton;
    [SerializeField] private Button _endTurnButton;

    [Header("Follow Settings")]
    [Tooltip("Khoảng lệch so với gốc unit, thường đẩy lên trên đầu")]
    [SerializeField] private Vector3 _worldOffset = new Vector3(0f, 1.0f, 0f);

    private IMovement _activeMovement;
    private IAttackManager _activeAttackManager;
    private Vector3 _activeUnitPosition;
    private Camera _mainCamera;

    private bool isMoveThisTurn = false;
    private bool isAttackThisTurn = false;

    public UnityEvent OnTurnEnd;
    public CharacterState CurrentAction { get; private set; } = CharacterState.IDLE;
    public bool HasActiveUnit => _activeUnitPosition != null;

    private void Awake()
    {
        _moveButton.onClick.AddListener(OnMoveButtonClicked);
        _attackButton.onClick.AddListener(OnAttackButtonClicked);
        _endTurnButton.onClick.AddListener(OnEndTurnButtonClicked);
        _mainCamera = Camera.main;
    }

    private void Start()
    {
        HidePanel();
    }

    /// <summary>
    /// Gọi hàm này từ TurnManager khi bắt đầu lượt của 1 unit cụ thể.
    /// </summary>
    public void ShowMenuFor(ICharacter player)
    {
        _activeMovement = player.GetMovementManager();
        _activeUnitPosition = player.GetCharWorldPosition();
        _activeAttackManager = player.GetAttackManager();
        //Debug.Log($"Player's Name: {player.GetName()}");
        //Debug.Log("_activeUnitPosition: " + _activeUnitPosition);   
        CurrentAction = CharacterState.IDLE;
        _menuPanel.gameObject.SetActive(true);
        UpdatePanelPosition(); // đặt vị trí ngay, tránh nháy 1 frame ở vị trí cũ
    }
    /// <summary>
    /// Gọi khi kết thúc lượt của unit (dọn reference, không còn follow ai nữa).
    /// </summary>
    public void ClearActiveUnit()
    {
        _activeMovement = null;
        _activeUnitPosition = Vector3.zero;
        HidePanel();
    }

    private void HidePanel()
    {
        _menuPanel.gameObject.SetActive(false);
    }
    private void LateUpdate()
    {
        // LateUpdate để đảm bảo unit đã di chuyển xong trong frame trước khi tính vị trí UI
        if (_activeUnitPosition != null && _menuPanel.gameObject.activeSelf)
        {
            UpdatePanelPosition();
        }
    }

    private void UpdatePanelPosition()
    {
        Vector2 screenPoint = _mainCamera.WorldToScreenPoint(_activeUnitPosition + _worldOffset);
        _menuPanel.position = screenPoint;
    }

    private void OnMoveButtonClicked()
    {
        HidePanel();
        if (isMoveThisTurn) return;
        isMoveThisTurn = true;
        CurrentAction = CharacterState.MOVE;
        _activeMovement.SetMovementState(MovementState.MOVEMENTRANGEVISUAL);
    }

    private void OnAttackButtonClicked()
    {
        HidePanel();
        if (isAttackThisTurn) return;
        isAttackThisTurn = true;
        _attackSubMenu.ShowMenuFor(_activeMovement.gameObject.GetComponent<ICharacter>());

        //_activeAttackManager.SetCurrentAttackManagerState(AttackManagerState.ATTACKMENUSHOW);
        
        // TODO: khi có PlayerATTACK, gọi tương tự:
        // _activeATTACK.BeginATTACKTargeting();
    }

    private void OnEndTurnButtonClicked()
    {
        CurrentAction = CharacterState.IDLE;
        OnTurnEnd?.Invoke();
        HidePanel();
        ResetFlags();
        // TODO: khi có PlayerENDTURN, gọi tương tự:
        // _activeENDTURN.EndTurn();
    }
    private void ResetFlags()
    {
        isMoveThisTurn = false;
        isAttackThisTurn = false;   
    }
}