using JetBrains.Annotations;
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
    [Header("Preview Attack Range")]
    [SerializeField] private GameObject _attackableTilePrefab;
    private List<GameObject> _previewTilesPrefabPool;
    private int _activePreviewCount;
    private PathNode _lastHoveredNode;



    [Header("Player Control")]
    [SerializeField] private InputActionAsset _inputActionAsset;
    private InputAction _selectAction;



    [Header("Interior Data")]
    [SerializeField] TurnManager _currentTurnManager;
    public AttackManagerState CurrentAttackManagerState { get; set; }    
    public int SkillChoice { get; set; }

    
    
    [Header("Player Relevant")]
    private ICharacter CurrentPlayer { get; set; }
    private PlayerAttackMenu _attackMenu;
    private PathFinding _currentPlayerPathFinding;
    private AttackSet _currentPlayerAttackSet;


    private void Awake()
    {
        CurrentAttackManagerState = AttackManagerState.IDLE;
        _currentTurnManager = FindAnyObjectByType<TurnManager>();
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
        ActionData _selectedAction = ScriptableObject.CreateInstance<ActionData>();
        List<PathNode> hoveredNode = new List<PathNode>();
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
                _selectedAction = GetSelectedAction();
                List<PathNode> reachableNode = this._currentPlayerPathFinding.GetReachableNodes(PlayerRelativePosition.x, PlayerRelativePosition.y, _selectedAction.AttackRange * 10, true);
                this.CurrentPlayer.GetMovementManager().RangeVisualize(reachableNode);
                CurrentAttackManagerState = AttackManagerState.ATTACKAREAOFEFFECTPREVIEW;
                break;
            }
            case (AttackManagerState.ATTACKAREAOFEFFECTPREVIEW):
            {
                UpdatePreviewTiles(_selectedAction, out hoveredNode);
                if (_selectAction.WasPressedThisFrame())
                {
                    CurrentAttackManagerState = AttackManagerState.ATTACKEXECUTION;
                }
                break;
            }
            case (AttackManagerState.ATTACKEXECUTION):
            {
                AttackExecute(_selectedAction, hoveredNode);
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
    private void UpdatePreviewTiles(ActionData _selectedAction, out List<PathNode> previewNodes)
    {
        Vector2 mousePosition = Mouse.current.position.ReadValue();
        Vector3 mouseWorldPosition = Camera.main.ScreenToWorldPoint(mousePosition);
        Vector2Int hoveredGridPos = GridSetup.Grid.GetGridPosition(mouseWorldPosition);
        previewNodes = new List<PathNode>();



        if (hoveredGridPos.x == -1 && hoveredGridPos.y == -1)
        {
            HideAllPreview();
            _lastHoveredNode = null;
            return;
        }

        PathNode hoveredNode = GridSetup.Grid.GetGridObject(hoveredGridPos.x, hoveredGridPos.y);
        if (hoveredNode == _lastHoveredNode) return; // ô chưa đổi, khỏi tính lại
        _lastHoveredNode = hoveredNode;

        Vector2Int playerGridPos = GridSetup.Grid.GetGridPosition(transform.position);
        if (hoveredNode.XCoordinate == playerGridPos.x && hoveredNode.YCoordinate == playerGridPos.y)
        {
            HideAllPreview();
            return;
        }

        

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
        //Destroy(_selectedAction);
    }
    private ActionData GetSelectedAction()
    {
        ActionData _selectedAction = ScriptableObject.CreateInstance<ActionData>();
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
        return _selectedAction;
    }
    private List<PathNode> GetStraightLineNodes(PathNode hoveredNode, int AOERange)
    {
        List<PathNode> nodes = new List<PathNode>();
        Vector2Int playerGridPos = GridSetup.Grid.GetGridPosition(transform.position);

        int dx = hoveredNode.XCoordinate - playerGridPos.x;
        int dy = hoveredNode.YCoordinate - playerGridPos.y;

        int dirX = 0, dirY = 0;
        if (Mathf.Abs(dx) >= Mathf.Abs(dy))
        {
            dirX = dx == 0 ? 0 : (int)Mathf.Sign(dx);
        }
        else
        {
            dirY = dy == 0 ? 0 : (int)Mathf.Sign(dy);
        }

        // Player đứng đúng ô đó -> không xác định được hướng, bỏ qua
        if (dirX == 0 && dirY == 0) return nodes;

        for (int i = 0; i < AOERange; i++)
        {
            int x = hoveredNode.XCoordinate + dirX * i;
            int y = hoveredNode.YCoordinate + dirY * i;
            PathNode node = GridSetup.Grid.GetGridObject(x, y);
            if (node == null) break; // ra khỏi biên grid thì dừng
            nodes.Add(node);
        }
        return nodes;
    }
    private List<PathNode> GetDiamondAreaNodes(PathNode hoveredNode, int AOERange)
    {
        List<PathNode> nodes = new List<PathNode>();
        Vector2Int playerGridPos = GridSetup.Grid.GetGridPosition(transform.position);

        int dx = hoveredNode.XCoordinate - playerGridPos.x;
        int dy = hoveredNode.YCoordinate - playerGridPos.y;

        int dirX = 0, dirY = 0;
        if (Mathf.Abs(dx) >= Mathf.Abs(dy))
        {
            dirX = dx == 0 ? 0 : (int)Mathf.Sign(dx);
        }
        else
        {
            dirY = dy == 0 ? 0 : (int)Mathf.Sign(dy);
        }

        // Player đứng đúng ô đó -> không xác định được hướng, bỏ qua
        if (dirX == 0 && dirY == 0) return nodes;

        for (int i = 0; i < AOERange; i++)
        {
            int x = hoveredNode.XCoordinate + dirX * i;
            int y = hoveredNode.YCoordinate + dirY * i;
            PathNode node = GridSetup.Grid.GetGridObject(x, y);
            if (node == null) break; // ra khỏi biên grid thì dừng
            nodes.Add(node);
        }

        return nodes;
    }
    private List<ICharacter> GetTargetAtHoveredNode(List<PathNode> hoveredNodes)
    {
        List<ICharacter> targets = new List<ICharacter>();
        foreach(PathNode node in hoveredNodes) {
            Vector3 nodeCoordinates = GridSetup.Grid.GetCellWorldPosition(node.XCoordinate, node.YCoordinate);
            targets.Add(_currentTurnManager.FindCharAtPosition(nodeCoordinates));
        }
        return targets;
    }
    private bool IsOriginInAttackRange(PathNode originNode)
    {
        Vector2Int playerGridPos = GridSetup.Grid.GetGridPosition(transform.position);

        int dx = Mathf.Abs(originNode.XCoordinate - playerGridPos.x);
        int dy = Mathf.Abs(originNode.YCoordinate - playerGridPos.y);
        int gridDistance = dx + dy; // Manhattan: không cho phép đi chéo

        return gridDistance <= CurrentPlayer.CurrentDatas.CurrentAttackRange;
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
            Vector3 cellCenter = GridSetup.Grid.GetCellWorldPosition(AttackReachableNode[i].XCoordinate, AttackReachableNode[i].YCoordinate);
            cellCenter.x += 0.5f;
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
    public void SetCurrentSkillChoice(int SkillChoice)
    {
        this.SkillChoice = SkillChoice;
    }
    private void AttackExecute(ActionData selectedAction, List<PathNode> hoveredNodes)
    {
        List<ICharacter> targets = GetTargetAtHoveredNode(hoveredNodes); // lấy từ preview đã chọn, có thể null
        List<int> damagetotargets = new List<int>();
        foreach (ICharacter target in targets)
        {
            int damageToTarget = target != null
                ? CalculateDamage(CurrentPlayer, target, selectedAction)
                : 0;
            damagetotargets.Add(damageToTarget);
        }

        var combatData = new CombatData
        {
            UsedAction = selectedAction,
            Attacker = new CombatParticipantResult
            {
                CombatParticipant = CurrentPlayer,
                DamageTaken = 0 // nếu sau này có counter-attack thì tính ở đây
            },
            Defender = targets[0] != null ? new CombatParticipantResult
            {
                CombatParticipant = targets[0],
                DamageTaken = damagetotargets[0]
            } : null
        };

        CombatSignal.FireCombatScene(combatData);
        CurrentAttackManagerState = AttackManagerState.SHOWCOMBATSCENE;
    }
    private int CalculateDamage(ICharacter Attacker, ICharacter Defender, ActionData selectedAction)
    {
        int damage = (int)Attacker.CurrentDatas.CurrentAttack * (int)selectedAction.SkillMultiplier - (int)Defender.CurrentDatas.CurrentDefense;
        return damage;
    }
}
