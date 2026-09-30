using System.Collections.Generic;
using Unity.Multiplayer.PlayMode;
using UnityEngine;

public class EnemyAttackManager : MonoBehaviour, IAttackManager
{
    public AttackManagerState CurrentAttackManagerState { get; set; }
    public int _skillchoice;
    private AttackSet _currentEnemyAttackSet;
    private ICharacter _currentEnemy;
    private EnemyAI _currentAI;
    private void Awake()
    { 
        CurrentAttackManagerState = AttackManagerState.IDLE;
        _currentEnemy = GetComponent<ICharacter>();
        _currentAI = GetComponent<EnemyAI>();
        _skillchoice = 0;
    }
    private void Start()
    {
        _currentEnemyAttackSet = GetComponent<Enemy>().GetAttackSet();
    }
    public ActionData GetSelectedAction()
    {
        ActionData _selectedAction = ScriptableObject.CreateInstance<ActionData>();
        switch (_skillchoice)
        {
            case 1:
                {
                    _selectedAction = _currentEnemyAttackSet.NormalAttack;
                    break;
                }
            case 2:
                {
                    _selectedAction = _currentEnemyAttackSet.SpecialAttack;
                    break;
                }
            case 3:
                {
                    _selectedAction = _currentEnemyAttackSet.SkillAttack;
                    break;
                }
        }
        return _selectedAction;
    }
    public void AttackExecute(ActionData selectedAction, PlayerCharsPosition target)
    {
        /* Khi được gọi, lấy tầm của kỹ năng và reachableNode
         * Hướng của Target so với Enemy hiện tại 
         * Range theo hướng target xem có thêm target nào khác trong range không
         * Xem bằng cách duyệt qua từng ô, đối chiếu với danh sách PlayerCharsPosition, nếu có thì sẽ thêm vào list
         * Nếu không thì đóng gói dữ liệu và gọi scene
         * Nếu có thì thêm target đó vào trong danh sách mục tiêu
         * Sau khi có ds mục tiêu rồi, gọi sử lý như hàm Execute trong PlayerAttackManager
         * ---------------------------------------------------------------------------------------
         * Mã giả:
         * Vector2Int currentRelativePosition
         * List<PathNode> AttackReachableNodes = _currentEnemy.GetPathFinding().GetReachableNodes(selectedAction.SkillRange)
         * List<PlayerCharsPosition> targets = new List<PlayerCharsPosition>();
         * if(selectedAction.ActionType == ActionType.StraightLine){
         *  Vector2Int directionVector = GetDirectionVector(target);
         *  targets = GetTargetsInRange(target, AttackReachableNodes, directionVector);
         * }
         * else if(selectedAction.ActionType == ActionType.AreaOfEffect){
         *  List<PathNode> AOERange = _currentEnemy.GetPathFinding().GetReachableNodes(selectedAction.AreaOfEffect)
         *  targets = GetTargetsInRange(target, AttackReachableNodes, AOERange);
         * }
         * else targets.Add(target);
         * 
         * AttackProcessing(ActionData selectedAction, List<PlayerCharsPosition> targets);
         * ----------------------------------------------------------------------------------------
         */
        Vector2Int currentRelativePosition = GridSetup.Grid.GetGridPosition(transform.position);
        List<PlayerCharsPosition> validTargets = new List<PlayerCharsPosition>();
        validTargets.Add(target);
        if(selectedAction.ActionType == ActionType.StraightLineAttack)
        {
            Vector2Int directionVector = GetDirectionVector(currentRelativePosition, target.PlayerCharCoodinates);
            GetTargetsInRange(target, directionVector, selectedAction.AreaOfEffectRange, out validTargets);
        }
        else if(selectedAction.ActionType == ActionType.AreaOfEffectAttack)
        {
            GetTargetsInRange(target, selectedAction.AreaOfEffectRange, out validTargets);
        }

        AttackProcessing(selectedAction, validTargets);
    }
    public void SetCurrentAttackManagerState(AttackManagerState state)
    {
        this.CurrentAttackManagerState = state;
    }
    public void SetCurrentSkillChoice(int SkillChoice)
    {
        this._skillchoice = SkillChoice;
    }
    private Vector2Int GetDirectionVector(Vector2Int currentPosition, Vector2Int targetPosition)
    {

        int dx = targetPosition.x - currentPosition.x;
        int dy = targetPosition.y - currentPosition.y;

        int dirX = 0;
        int dirY = 0;

        if(Mathf.Abs(dx) >= Mathf.Abs(dy))
        {
            if (dx == 0) dirX = 0;
            else dirX = (int)Mathf.Sign(dx);
        }
        else
        {
            if (dy == 0) dirY = 0;
            else dirY = (int)Mathf.Sign(dy);
        }

        return new Vector2Int(dirX, dirY);
    }

    /// <summary>
    /// Thêm mục tiêu vào danh sách nếu hợp lệ và chưa có trong danh sách (so sánh theo character thật).
    /// </summary>
    private void TryAddTarget(List<PlayerCharsPosition> list, PlayerCharsPosition candidate)
    {
        if (candidate == null || candidate.PlayerChar == null) return;
        if (list.Exists(t => t.PlayerChar == candidate.PlayerChar)) return;
        list.Add(candidate);
    }

    public void GetTargetsInRange(PlayerCharsPosition target, Vector2Int DirectionVector, int AOERange, out List<PlayerCharsPosition> ValidTargets)
    {
        // Target chính luôn nằm ở đầu danh sách
        ValidTargets = new List<PlayerCharsPosition>();
        ValidTargets.Add(target);
        if (DirectionVector.x == 0 && DirectionVector.y == 0) return;

        // FIX: duyệt dọc theo hướng, bắt đầu từ chính ô của target (i = 0).
        // Trước đây i = 0 làm target bị thêm lần 2 -> nhận damage 2 lần; nay TryAddTarget loại trùng.
        // FIX: ra khỏi biên lưới (GetGridObject trả null) thì dừng thay vì truyền null vào GetCharInNode -> NullReference.
        for(int i = 0; i < AOERange; i++)
        {
            int x = target.PlayerCharCoodinates.x + DirectionVector.x * i;
            int y = target.PlayerCharCoodinates.y + DirectionVector.y * i;
            PathNode CheckNode = GridSetup.Grid.GetGridObject(x, y);
            if (CheckNode == null) break;

            PlayerCharsPosition AdditionalTarget = _currentAI.GetCharInNode(CheckNode);
            TryAddTarget(ValidTargets, AdditionalTarget);
        }
    }
    public void GetTargetsInRange(PlayerCharsPosition target, int AOERange, out List<PlayerCharsPosition> ValidTargets)
    {
        // FIX: thiếu "* 10" -> mỗi bước đi tốn 10 cost nên trước đây AOERange = 2 (cost tối đa 2) không lan tới ô nào.
        // Attack mode: target đứng ngay đó là BlockedByCharacter, không được chặn việc lan AOE ra các ô xung quanh
        List<PathNode> AttackReachableNodes = _currentEnemy.GetMovementManager().GetPathFinding().GetReachableNodes(target.PlayerCharCoodinates.x, target.PlayerCharCoodinates.y, AOERange * 10, mode: PathfindingMode.Attack);
        ValidTargets = new List<PlayerCharsPosition>();
        ValidTargets.Add(target); // ô trung tâm (GetReachableNodes đã loại ô xuất phát) luôn là target chính

        // AttackReachableNodes được sắp xếp gần tâm -> xa tâm
        foreach (PathNode node in AttackReachableNodes)
        {
            PlayerCharsPosition AdditionalTarget = _currentAI.GetCharInNode(node);
            TryAddTarget(ValidTargets, AdditionalTarget);
        }
    }

    private void AttackProcessing(ActionData selectedAction, List<PlayerCharsPosition> ValidTargets)
    {
        // FIX: tính damage và đóng gói kết quả cho TOÀN BỘ mục tiêu (trước đây chỉ ValidTargets[0] được dùng)
        List<CombatParticipantResult> defenderResults = new List<CombatParticipantResult>();
        foreach (PlayerCharsPosition target in ValidTargets)
        {
            if (target == null || target.PlayerChar == null) continue;

            defenderResults.Add(new CombatParticipantResult
            {
                CombatParticipant = target.PlayerChar,
                DamageTaken = CalculateDamage(_currentEnemy, target.PlayerChar, selectedAction)
            });
        }

        var combatData = new CombatData
        {
            UsedAction = selectedAction,
            Attacker = new CombatParticipantResult
            {
                CombatParticipant = _currentEnemy,
                DamageTaken = 0 // nếu sau này có counter-attack thì tính ở đây
            }
        };
        combatData.SetDefenders(defenderResults);

        _currentEnemy.CurrentDatas.CurrentMP -= selectedAction.ManaCost;
        CombatSignal.FireCombatScene(combatData);
    }
    private int CalculateDamage(ICharacter Attacker, ICharacter Defender, ActionData selectedAction)
    {
        int damage = (int)Attacker.CurrentDatas.CurrentAttack * (int)selectedAction.SkillMultiplier - (int)Defender.CurrentDatas.CurrentDefense;
        return damage;
    }
    // EnemyAttackManager.cs
    public void ResetAttackManager()
    {
        CurrentAttackManagerState = AttackManagerState.IDLE;
    }
}
