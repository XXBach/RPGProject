using System.Collections.Generic;
using UnityEngine;


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
    private void Start()
    {
        
    }

    private void GetPlayerCharPositionList()
    {
        foreach(ICharacter character in _currentTurnManager._characterList)
        {
            if(character is Player)
            {
                PlayerCharsPosition pcp = new PlayerCharsPosition();
                pcp.PlayerChar = character;
                pcp.PlayerCharCoodinates = new Vector2Int(0, 0);
                this._playerCharPositionList.Add(pcp);
            }
        }
    }
}
