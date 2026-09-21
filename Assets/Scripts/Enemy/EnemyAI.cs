using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.TextCore.Text;


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
    private CharacterState _characterCurrentState;
    private CharacterState _characterPreviousState;

    private List<PlayerCharsPosition> _playerCharPositionList;
    private void Awake()
    {
        _enemyMovementManager ??= GetComponent<EnemyMovement>();
        _enemyAttackManager ??= GetComponent<EnemyAttackManager>();
        _currentAttackSet = GetComponent<Enemy>().GetAttackSet();
        _currentData = GetComponent<Enemy>().CurrentDatas;
        _characterCurrentState = CharacterState.IDLE;
        _playerCharPositionList = new List<PlayerCharsPosition>();
    }

    private void Update()
    {
        PlayerCharsPosition Target = new PlayerCharsPosition();
        switch (_characterCurrentState)
        {
            case CharacterState.IDLE:
                {
                    break;
                }
            case CharacterState.CALCULATING:
                {
                    Vector2Int CurrentPosition = GridSetup.Grid.GetGridPosition(transform.position);
                    List<PathNode> ValidRange = _enemyMovementManager.GetPathFinding().GetReachableNodes(CurrentPosition.x, CurrentPosition.y, (_currentData.CurrentMovementRange + GetHighestSkillRange()) * 10, false);
                    Target = GetNearestPC(PCSortingHP());
                    //Nếu mục tiêu không yêu cầu di chuyển để đánh - không di chuyển, chỉ đánh
                    //Nếu mục tiêu yêu cầu di chuyển để đánh - di chuyển mức thấp nhất để đánh
                    //Nếu mục tiêu không nằm trong tầm di chuyển + đánh - di chuyển đến ô gần mục tiêu nhất có thể
                    //if (isInValidRange(target, ValidRange))
                    //{
                    //  if(isInValidRange(target, AttackRange)){
                    //      AttackTarget(target);
                    //  }
                    //  else{
                    //      FindFurthestPossibleCell(target, AttackRange);
                    //      MoveToTargetCell(Cell);
                    //      AttackTarget(target);
                    //  }
                    //}
                    //else{
                    //  MoveToTargetNearestCell(target);
                    //  EndTurn
                    //}
                    break;
                }
            case CharacterState.MOVE:
                {
                    break;
                }
            case CharacterState.ATTACK:
                {
                    break;
                }
            default:
                {
                    _characterCurrentState = CharacterState.IDLE;
                    break;
                }
        }
    }

    public void GetPlayerCharPositionList()
    {
        foreach(ICharacter character in _currentTurnManager._characterList)
        {
            if(character is Player)
            {
                PlayerCharsPosition pcp = new PlayerCharsPosition();
                pcp.PlayerChar = character;
                pcp.PlayerCharCoodinates = new Vector2Int(character.GetSpawningPosition().XCoordinate, character.GetSpawningPosition().YCoordinate);
                this._playerCharPositionList.Add(pcp);
            }
        }
    }

    public int GetHighestSkillRange()
    {
        int HighestSkillRange = 0;
        if (HighestSkillRange <= _currentAttackSet.SkillAttack.AttackRange && _currentData.CurrentMP >= _currentAttackSet.SkillAttack.ManaCost) HighestSkillRange = _currentAttackSet.SkillAttack.AttackRange;
        else if (HighestSkillRange <= _currentAttackSet.SpecialAttack.AttackRange && _currentData.CurrentMP >= _currentAttackSet.SpecialAttack.ManaCost) HighestSkillRange = _currentAttackSet.SpecialAttack.AttackRange;
        else if (HighestSkillRange <= _currentAttackSet.NormalAttack.AttackRange && _currentData.CurrentMP >= _currentAttackSet.NormalAttack.ManaCost) HighestSkillRange = _currentAttackSet.NormalAttack.AttackRange;
        else HighestSkillRange = 0;
        return HighestSkillRange;
    }



    public List<PlayerCharsPosition> PCSortingHP(){
        List<PlayerCharsPosition> PCSortedHPList = new List<PlayerCharsPosition>();
        foreach(PlayerCharsPosition PCCharsPos in this._playerCharPositionList)
        {
            PlayerCharsPosition LowestHPPCthisturn = GetLowestHPPC();
            PCSortedHPList.Add(LowestHPPCthisturn);
            _playerCharPositionList.Remove(LowestHPPCthisturn);
        }
        _playerCharPositionList = PCSortedHPList;
        return PCSortedHPList;
    }
    public PlayerCharsPosition GetLowestHPPC()
    {
        PlayerCharsPosition LowestHPPC = _playerCharPositionList[0];
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
        PlayerCharsPosition NearestPC = _playerCharPositionList[0];
        float NearestRelativeDistance = Vector2.Distance(NearestPC.PlayerCharCoodinates, GridSetup.Grid.GetGridPosition(transform.position));
        foreach (var Character in CharList)
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
}
