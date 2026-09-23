using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.TextCore.Text;
using UnityEngine.Events;

[System.Serializable]
public class PlayerCharsPosition
{
    public ICharacter PlayerChar;
    public Vector2Int PlayerCharCoodinates;
}


public class EnemyAI : MonoBehaviour
{
    [Header("Turn Manager")]
    [SerializeField]private TurnManager _currentTurnManager;

    [Header("Enemy Managers")]
    [SerializeField] private EnemyMovement _enemyMovementManager;
    [SerializeField] private EnemyAttackManager _enemyAttackManager;
    private AttackSet _currentAttackSet;
    private CharacterData _currentData;
    public CharacterState _characterCurrentState;
    public UnityEvent OnTurnEnd;

    private List<PlayerCharsPosition> _playerCharPositionList;
    private void Awake()
    {
        _enemyMovementManager ??= GetComponent<EnemyMovement>();
        _enemyAttackManager ??= GetComponent<EnemyAttackManager>();
        _characterCurrentState = CharacterState.IDLE;
        _playerCharPositionList = new List<PlayerCharsPosition>();
    }
    private void Start()
    {
        _currentTurnManager ??= FindAnyObjectByType<TurnManager>();
        _currentAttackSet = GetComponent<Enemy>().GetAttackSet();
        _currentData = GetComponent<Enemy>().CurrentDatas;
    }
    private void Update()
    {
        PlayerCharsPosition Target = new PlayerCharsPosition();
        switch (_characterCurrentState)
        {
            case CharacterState.IDLE:
                {
                    _playerCharPositionList = GetPlayerCharPositionList();
                    break;
                }
            case CharacterState.CALCULATING:
                {
                    Vector2Int CurrentPosition = GridSetup.Grid.GetGridPosition(transform.position);
                    List<PathNode> ValidRange = _enemyMovementManager.GetPathFinding().GetReachableNodes(CurrentPosition.x, CurrentPosition.y, (_currentData.CurrentMovementRange + GetHighestSkillRange(out _enemyAttackManager._skillchoice)) * 10, false);
                    List<PathNode> AttackRange = _enemyMovementManager.GetPathFinding().GetReachableNodes(CurrentPosition.x, CurrentPosition.y, (GetHighestSkillRange(out _enemyAttackManager._skillchoice)) * 10, false);
                    List<PlayerCharsPosition> PCSortedList = PCSortingHP();
                    Target = GetNearestPC(PCSortedList);
                    //Nếu mục tiêu không yêu cầu di chuyển để đánh - không di chuyển, chỉ đánh
                    //Nếu mục tiêu yêu cầu di chuyển để đánh - di chuyển mức thấp nhất để đánh
                    //Nếu mục tiêu không nằm trong tầm di chuyển + đánh - di chuyển đến ô gần mục tiêu nhất có thể
                    if (IsCharInValidRange(Target, ValidRange))
                    {
                        if (IsCharInValidRange(Target, AttackRange))
                        {
                            AttackTarget(Target);
                            _characterCurrentState = CharacterState.ENDTURN;
                        }
                        else
                        {
                            List<PathNode> MovementRange = _enemyMovementManager.GetPathFinding().GetReachableNodes(CurrentPosition.x, CurrentPosition.y, (_currentData.CurrentMovementRange) * 10, false);
                            Vector2Int TargetCoordinates = _enemyMovementManager.FindNearestPossibleCoordinates(Target, MovementRange, GetHighestSkillRange(out _enemyAttackManager._skillchoice));
                            _enemyMovementManager.MoveToTargetCell(TargetCoordinates);
                            AttackTarget(Target);
                            _characterCurrentState = CharacterState.ENDTURN;
                        }
                    }
                    else
                    {
                        _enemyMovementManager.MoveToTargetNearestCell(Target);
                        _characterCurrentState = CharacterState.ENDTURN;
                    }
                    break;
                }
            default:
                {
                    OnTurnEnd?.Invoke();
                    _currentTurnManager.SetIsTurnEnded();
                    _characterCurrentState = CharacterState.IDLE;
                    break;
                }
        }
    }

    public List<PlayerCharsPosition> GetPlayerCharPositionList()
    {
        List<PlayerCharsPosition> PCCharPositionList = new List<PlayerCharsPosition>();
        foreach(ICharacter character in _currentTurnManager._characterList)
        {
            if(character is Player)
            {
                PlayerCharsPosition pcp = new PlayerCharsPosition();
                pcp.PlayerChar = character;
                Vector3 PCCurrentWorldPos = character.GetCharWorldPosition();
                Vector2Int PCCurrentRelativePos = GridSetup.Grid.GetGridPosition(PCCurrentWorldPos);
                pcp.PlayerCharCoodinates = PCCurrentRelativePos;
                PCCharPositionList.Add(pcp);
            }
        }
        return PCCharPositionList;
    }

    public int GetHighestSkillRange(out int skillchoice)
    {
        int HighestSkillRange = 0;
        skillchoice = 0;
        if (HighestSkillRange <= _currentAttackSet.SkillAttack.AttackRange && _currentData.CurrentMP >= _currentAttackSet.SkillAttack.ManaCost)
        {
            HighestSkillRange = _currentAttackSet.SkillAttack.AttackRange;
            skillchoice = 1;
        }
        else if (HighestSkillRange <= _currentAttackSet.SpecialAttack.AttackRange && _currentData.CurrentMP >= _currentAttackSet.SpecialAttack.ManaCost)
        {
            HighestSkillRange = _currentAttackSet.SpecialAttack.AttackRange;
            skillchoice = 2;
        }
        else if (HighestSkillRange <= _currentAttackSet.NormalAttack.AttackRange && _currentData.CurrentMP >= _currentAttackSet.NormalAttack.ManaCost)
        {
            HighestSkillRange = _currentAttackSet.NormalAttack.AttackRange;
            skillchoice = 3;
        }
        else
        {
            HighestSkillRange = 0;
            skillchoice = 0;
        }
        return HighestSkillRange;
    }



    public List<PlayerCharsPosition> PCSortingHP(){
        //Điều chỉnh phần này
        List<PlayerCharsPosition> PCSortedHPList = new List<PlayerCharsPosition>();
        int count = this._playerCharPositionList.Count;
        for(int i = 0; i < count; i++)
        {
            PlayerCharsPosition LowestHPPCthisturn = GetLowestHPPC();
            Debug.Log(LowestHPPCthisturn);
            PCSortedHPList.Add(LowestHPPCthisturn);
            _playerCharPositionList.Remove(LowestHPPCthisturn);
        }
        _playerCharPositionList = PCSortedHPList;
        return PCSortedHPList;
    }
    public PlayerCharsPosition GetLowestHPPC()
    {
        PlayerCharsPosition LowestHPPC = _playerCharPositionList[0];
        Debug.Log(LowestHPPC);
        int LowestHP = LowestHPPC.PlayerChar.CurrentDatas.CurrentHealth;
        foreach (var Character in _playerCharPositionList)
        { 
            if (LowestHP > Character.PlayerChar.CurrentDatas.CurrentHealth)
            {
                LowestHP = Character.PlayerChar.CurrentDatas.CurrentHealth;
                LowestHPPC = Character;
            }
        }
        return LowestHPPC;
    }
    public PlayerCharsPosition GetNearestPC()
    {
        PlayerCharsPosition NearestPC = _playerCharPositionList[0];
        float NearestRelativeDistance = Vector2.Distance(NearestPC.PlayerCharCoodinates, GridSetup.Grid.GetGridPosition(transform.position));
        foreach (var Character in _playerCharPositionList)
        {
            float relativeDistance = Vector2.Distance(Character.PlayerCharCoodinates, GridSetup.Grid.GetGridPosition(transform.position));
            if (NearestRelativeDistance > relativeDistance)
            {
                NearestRelativeDistance = relativeDistance;
                NearestPC = Character;
            }
        }
        return NearestPC;
    }
    public PlayerCharsPosition GetNearestPC(List<PlayerCharsPosition> CharList)
    {
        PlayerCharsPosition NearestPC = CharList[0];
        Vector2Int selfGridPos = GridSetup.Grid.GetGridPosition(transform.position);
        int NearestRelativeDistance = GetManhattanDistance(NearestPC.PlayerCharCoodinates, selfGridPos);

        foreach (var Character in CharList)
        {
            int relativeDistance = GetManhattanDistance(Character.PlayerCharCoodinates, selfGridPos);
            if (relativeDistance < NearestRelativeDistance)
            {
                NearestRelativeDistance = relativeDistance;
                NearestPC = Character;
            }
        }
        return NearestPC;
    }

    public bool IsCharInValidRange(PlayerCharsPosition target, List<PathNode> ValidRange)
    {
        foreach(PathNode gridcell in ValidRange)
        {
            if(target.PlayerCharCoodinates.x == gridcell.XCoordinate && target.PlayerCharCoodinates.y == gridcell.YCoordinate)
            {
                return true;
            }
        }
        return false;
    }

    public PlayerCharsPosition? GetCharInNode(PathNode Node)
    {
        foreach(PlayerCharsPosition character in _playerCharPositionList)
        {
            Vector2Int NodePosition = new Vector2Int(Node.XCoordinate, Node.YCoordinate);
            if(Vector2Int.Distance(NodePosition, character.PlayerCharCoodinates) <= 0.03)
            {
                return character;
            }
        }
        return null;
    }

    public void AttackTarget(PlayerCharsPosition target)
    {
        ActionData selectedAction = _enemyAttackManager.GetSelectedAction();
        _enemyAttackManager.AttackExecute(selectedAction, target);
    }

    public EnemyMovement GetEnemyMovement()
    {
        return _enemyMovementManager;
    }
    public EnemyAttackManager GetEnemyAttackManager()
    {
        return _enemyAttackManager;
    }
    private int GetManhattanDistance(Vector2Int a, Vector2Int b)
    {
        return Mathf.Abs(a.x - b.x) + Mathf.Abs(a.y - b.y);
    }
}
