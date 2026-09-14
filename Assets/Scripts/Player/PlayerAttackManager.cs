using System.Collections.Generic;
using System.Net;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;

public enum AttackManagerState
{
    IDLE = 0,
    ATTACKRANGEVISUALIZE = 1,
    ATTACKAREAOFEFFECTPREVIEW = 2,
    ATTACKEXECUTION = 4,
    SHOWCOMBATSCENE = 5
}
public class PlayerAttackManager : MonoBehaviour, IAttackManager
{
    [SerializeField] private GameObject _attackableTilePrefab;
    [SerializeField] private InputActionAsset _inputActionAsset;
    public AttackManagerState CurrentAttackManagerState { get; set; }
    
    public int SkillChoice { get; set; }
    private ICharacter CurrentPlayer { get; set; }

    private PlayerAttackMenu _attackMenu;

    private PathFinding _currentPlayerPathFinding;

    private AttackSet _currentPlayerAttackSet;

    private InputAction _selectAction;

    private List<GameObject> _previewTilesPrefabPool;
    private int _activePreviewCount;
    private PathNode _lastHoveredNode;
    private void Awake()
    {
        CurrentAttackManagerState = AttackManagerState.IDLE;
        _previewTilesPrefabPool = new List<GameObject>();
        _activePreviewCount = 0;
    }
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        CurrentPlayer = GetComponent<Player>();
        _currentPlayerPathFinding = CurrentPlayer.GetMovementManager().GetPathFinding();
        _currentPlayerAttackSet = CurrentPlayer.GetAttackSet();
        _selectAction = _inputActionAsset.FindAction("Select");
    }

    // Update is called once per frame
    void Update()
    {
        //Lấy vị trí Player và biên ra tọa độ x, y
        Vector3 PlayerPosition = this.gameObject.transform.position;
        Vector2Int PlayerRelativePosition = GridSetup.Grid.GetGridPosition(PlayerPosition);
        //Nếu không có target nào trên ô đó thì hiện là không attack được, gọi hiện menu lại một lần nữa
        //Nếu chọn ở ngoài vùng attack khả dĩ thì hiện là không attack được, gọi hiện menu lại một lần nữa
        //Về việc kiểm tra range này, nếu hành động là single attack thì check đơn giản, nếu hành động là straightline attack hoặc AOE thì yêu cầu phải có ít nhất 1 character mục tiêu nằm trong range
        //Nếu thỏa mãn toàn bộ điều kiện trên thì tiến hành handle attack, chúng sẽ không khác nhau về công thức tính damage, chỉ khác nhau về số lần lặp lại trên vùng attack đã được chọn
        switch (CurrentAttackManagerState)
        {
            case (AttackManagerState.IDLE):
            {
                return;
            }
            case (AttackManagerState.ATTACKRANGEVISUALIZE):
            {
                this._currentPlayerPathFinding.GetReachableNodes(PlayerRelativePosition.x, PlayerRelativePosition.y, CurrentPlayer.CurrentDatas.CurrentAttackRange, true);
                    CurrentAttackManagerState = AttackManagerState.ATTACKAREAOFEFFECTPREVIEW;
                break;
            }
            case (AttackManagerState.ATTACKAREAOFEFFECTPREVIEW):
            {
                UpdatePreviewTiles();
                if (_selectAction.WasPressedThisFrame())
                {
                    CurrentAttackManagerState = AttackManagerState.ATTACKEXECUTION;
                }
                break;
            }
            case (AttackManagerState.ATTACKEXECUTION):
            {
                break;
            }
            case (AttackManagerState.SHOWCOMBATSCENE):
            {
                break;
            }
            default:
            {
                return;
            }
        }
    }
    public bool isAttackInRange(Vector3 targetPosition, ActionData actionData)
    {
        float distance = Vector3.Distance(transform.position, targetPosition);
        return distance <= GetComponent<Player>().CurrentDatas.CurrentAttackRange * actionData.AreaOfEffectRange;
    }


    private void ExecuteSingleTargetAttack(ActionData actionData, ICharacter target)
    {

    }

    private void HandleSingleAttackChoice(ActionData actionData, ICharacter target)
    {
        //Lấy hành động được chọn, gọi hàm hiện range của hành động
        
        //Để player chọn thêm 1 lần nữa, gọi hàm kiểm tra
    }
    private void UpdatePreviewTiles()
    {
        Vector2 mousePosition = Mouse.current.position.ReadValue();
        Vector3 mouseWorldPosition = Camera.main.ScreenToWorldPoint(mousePosition);
        Vector2Int hoveredGridPos = GridSetup.Grid.GetGridPosition(mouseWorldPosition);
        ActionData _selectedAction = new ActionData();

        switch (SkillChoice)
        {
            case 1:
                {
                    _selectedAction = _currentPlayerAttackSet.NormalAttack;
                    break;
                }
            case 2:
                {
                    _selectedAction = _currentPlayerAttackSet.SpecialAttack;
                    break;
                }
            case 3:
                {
                    _selectedAction = _currentPlayerAttackSet.SkillAttack;
                    break;
                }
        }

        if (hoveredGridPos.x == -1 && hoveredGridPos.y == -1)
        {
            HideAllPreview();
            _lastHoveredNode = null;
            return;
        }

        PathNode hoveredNode = GridSetup.Grid.GetGridObject(hoveredGridPos.x, hoveredGridPos.y);
        if (hoveredNode == _lastHoveredNode) return; // ô chưa đổi, khỏi tính lại
        _lastHoveredNode = hoveredNode;

        List<PathNode> previewNodes = new List<PathNode>();

        if (IsOriginInAttackRange(hoveredNode))
        {
            switch (_selectedAction.ActionType)
            {
                case ActionType.SingleAttack:
                    previewNodes.Add(hoveredNode);
                    break;
                case ActionType.StraightLineAttack:
                    previewNodes = GetStraightLineNodes(hoveredNode, _selectedAction.AreaOfEffectRange);
                    break;
                case ActionType.AreaOfEffectAttack:
                    previewNodes = GetDiamondAreaNodes(hoveredNode, _selectedAction.AreaOfEffectRange);
                    break;
            }
        }

        ShowPreview(previewNodes);
    }
    private List<PathNode> GetStraightLineNodes(PathNode hoveredNode, int AOERange)
    {
        List<PathNode > nodes = new List<PathNode>();
        return nodes;
    }
    private List<PathNode> GetDiamondAreaNodes(PathNode hoaveredNode, int AOERange)
    {
        List<PathNode> nodes = new List<PathNode>();
        return nodes;
    }
    private bool IsOriginInAttackRange(PathNode originNode)
    {
        Vector3 nodeWorldPos = GridSetup.Grid.GetCellWorldPosition(originNode.XCoordinate, originNode.YCoordinate);
        float distance = Vector3.Distance(transform.position, nodeWorldPos);
        return distance <= CurrentPlayer.CurrentDatas.CurrentAttackRange;
    }
    private void ShowPreview(List<PathNode> AttackReachableNode)
    {
        int needed = AttackReachableNode.Count;
        while (_previewTilesPrefabPool.Count < needed) { 
            GameObject tileprefab = Instantiate(_attackableTilePrefab);
            tileprefab.SetActive(false);
            _previewTilesPrefabPool.Add(tileprefab);
        }
        for (int i = 0; i < needed; i++) { 
            GameObject tile = _previewTilesPrefabPool[i];
            Vector3 cellCenter = GridSetup.Grid.GetCellWorldPosition(AttackReachableNode[i].XCoordinate, AttackReachableNode[i].YCoordinate) + new Vector3(GridSetup.Grid.CellSize, GridSetup.Grid.CellSize) * 0.5f;
            tile.transform.position = cellCenter;
            tile.SetActive(true);
        }
        for (int i = needed; i < _activePreviewCount; i++) {
            _previewTilesPrefabPool[i].SetActive(false);
        }
        _activePreviewCount = 0;
    }
    private void HideAllPreview()
    {
        foreach (GameObject tile in _previewTilesPrefabPool)
        {
            tile.SetActive(false);
        }
        _activePreviewCount = 0;
    }
    public void SetCurrentAttackManagerState(AttackManagerState state) { 
        CurrentAttackManagerState = state;
    }
}
