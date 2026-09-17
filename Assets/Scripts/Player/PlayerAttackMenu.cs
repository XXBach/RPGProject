using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public enum AttackState
{
    IDLE = 0,
    CALCULATEATTACKDAMAGE = 2,
    PLAYATTACKSCENE = 3,
    ENDATTACK = 4,
}
public class PlayerAttackMenu : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    [Header("UI References")]
    [SerializeField] private RectTransform _attackPanel;
    [SerializeField] private Button _normalAttackButton;
    [SerializeField] private Button _specialAttackButton;
    [SerializeField] private Button _skillAttackButton;

    [Header("Attack Icon")]
    [SerializeField] private Image _normalAttackIconImage;
    [SerializeField] private Image _specialAttackIconImage;
    [SerializeField] private Image _skillIconImage;

    [Header("Follow Settings")]
    [Tooltip("Khoảng lệch so với gốc unit, thường đẩy lên trên đầu")]
    [SerializeField] private Vector3 _worldOffset = new Vector3(0f, 1.0f, 0f);

    private IAttackManager _activeAttackManager;
    private Vector3 _activeUnitPosition;
    private Camera _mainCamera;

    private bool isAttackThisTurn = false;
    public bool HasActiveUnit => _activeUnitPosition != null;
    public AttackState CurrentAttackState { get; private set; }
    private void Awake()
    {
        _normalAttackButton.onClick.AddListener(OnNormalAttackButtonClicked);
        _specialAttackButton.onClick.AddListener(OnSpecialAttackButtonClicked);
        _skillAttackButton.onClick.AddListener(OnSkillButtonClicked);
        _mainCamera = Camera.main;
    }

    private void Start()
    {
        HidePanel();
    }

    /// <summary>
    /// Gọi hàm này từ Attack Manager khi bắt đầu lượt của 1 unit cụ thể.
    /// </summary>
    public void ShowMenuFor(ICharacter player)
    {
        _activeAttackManager = player.GetAttackManager();
        _activeUnitPosition = player.GetCharWorldPosition();
        CurrentAttackState = AttackState.IDLE;
        this.isAttackThisTurn = false;
        //Khi được gọi đến, AttackManager sẽ hiện một menu để chọn loại attack, menu này cũng sẽ hiện icon theo từng nút
        AttackSet _activeAttackSet = player.GetAttackSet();
        _normalAttackIconImage.sprite = _activeAttackSet.NormalAttack.SkillIcon;
        _specialAttackIconImage.sprite = _activeAttackSet.SpecialAttack.SkillIcon;
        _skillIconImage.sprite = _activeAttackSet.SkillAttack.SkillIcon;

        _attackPanel.gameObject.SetActive(true);
        UpdatePanelPosition(); // đặt vị trí ngay, tránh nháy 1 frame ở vị trí cũ
    }
    /// <summary>
    /// Gọi khi kết thúc lượt của unit (dọn reference, không còn follow ai nữa).
    /// </summary>
    public void ClearActiveUnit()
    {
        _activeAttackManager = null;
        _activeUnitPosition = Vector3.zero;
        HidePanel();
    }

    private void HidePanel()
    {
        _attackPanel.gameObject.SetActive(false);
    }
    private void LateUpdate()
    {
        // LateUpdate để đảm bảo unit đã di chuyển xong trong frame trước khi tính vị trí UI
        if (_activeUnitPosition != null && _attackPanel.gameObject.activeSelf)
        {
            UpdatePanelPosition();
        }
    }

    private void UpdatePanelPosition()
    {
        Vector2 screenPoint = _mainCamera.WorldToScreenPoint(_activeUnitPosition + _worldOffset);
        _attackPanel.position = screenPoint;
    }
    //Sau khi player đã chọn action, HideMenu
    private void OnNormalAttackButtonClicked()
    {
        if (isAttackThisTurn) return;
        isAttackThisTurn = true;
        CurrentAttackState = AttackState.CALCULATEATTACKDAMAGE;
        _activeAttackManager.SetCurrentSkillChoice(0);
        _activeAttackManager.SetCurrentAttackManagerState(AttackManagerState.ATTACKRANGEVISUALIZE);
        HidePanel();
    }

    private void OnSpecialAttackButtonClicked()
    {
        if (isAttackThisTurn) return;
        isAttackThisTurn = true;
        CurrentAttackState = AttackState.CALCULATEATTACKDAMAGE;
        _activeAttackManager.SetCurrentSkillChoice(1);
        _activeAttackManager.SetCurrentAttackManagerState(AttackManagerState.ATTACKRANGEVISUALIZE);
        HidePanel();
        // TODO: khi có PlayerATTACK, gọi tương tự:
        // _activeATTACK.BeginATTACKTargeting();
    }

    private void OnSkillButtonClicked()
    {
        if (isAttackThisTurn) return;
        isAttackThisTurn = true;
        CurrentAttackState = AttackState.CALCULATEATTACKDAMAGE;
        _activeAttackManager.SetCurrentSkillChoice(2);
        _activeAttackManager.SetCurrentAttackManagerState(AttackManagerState.ATTACKRANGEVISUALIZE);
        HidePanel();
        // TODO: khi có PlayerENDTURN, gọi tương tự:
        // _activeENDTURN.EndTurn();
    }
}
