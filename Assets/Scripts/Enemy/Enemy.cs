using UnityEngine;
using UnityEngine.UI;
[System.Serializable]
public class CharacterData
{
    public Sprite CharAvt;
    public string Name;
    public int MaxHealth;
    public int CurrentHealth;
    public int CurrentAttack;
    public int CurrentDefense;
    public int CurrentMovementRange;
    public int CurrentSpeed;
    public int CurrentMP;
    public int CurrentAttackRange;
    public int MaxMP;
}
[DefaultExecutionOrder(-100)]
public class Enemy : MonoBehaviour, ICharacter
{
    public CharacterData CurrentDatas { get; set; }
    [SerializeField] private CharacterBaseStats _baseStats;
    [SerializeField] private AttackSet _attackSet;
    [SerializeField] private CharacterVisualData _visualData;
    private EnemyAI _currentAIAgent;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    private void Start()
    {
        CurrentDatas = new CharacterData();
        CurrentDatas.MaxHealth = _baseStats.MaxHealth;
        CurrentDatas.CurrentHealth = _baseStats.MaxHealth;
        CurrentDatas.CurrentAttack = _baseStats.BaseDamage;
        CurrentDatas.CurrentDefense = _baseStats.BaseDef;
        CurrentDatas.CurrentMovementRange = _baseStats.MovementRange;
        CurrentDatas.CurrentSpeed = _baseStats.BaseSpeed;
        CurrentDatas.CurrentMP = _baseStats.MaxMP;
        CurrentDatas.Name = _baseStats.CharName;
        CurrentDatas.CurrentAttackRange = _baseStats.BaseAttackRange;
        CurrentDatas.CharAvt = _baseStats.CharAvt;
        CurrentDatas.MaxMP = _baseStats.MaxMP;
        _currentAIAgent = GetComponent<EnemyAI>();
    }

    // Update is called once per frame
    private void Update()
    {
    }
    public Vector3 GetCharWorldPosition()
    {
        return transform.position;
    }
    public IMovement GetMovementManager()
    {
        return _currentAIAgent.GetEnemyMovement();
    }
    public int GetCurrentSpeed()
    {
        return CurrentDatas.CurrentSpeed;
    }
    public int GetCurrentMP()
    {
        return CurrentDatas.CurrentMP;
    }
    public string GetName()
    {
        return CurrentDatas.Name;
    }
    public IAttackManager GetAttackManager()
    {
        return _currentAIAgent.GetEnemyAttackManager();
    }
    public AttackSet GetAttackSet()
    {
        return _attackSet;
    }
    public CharacterVisualData GetCharacterVisualData()
    {
        return this._visualData;
    }
    public EnemyAI? GetEnemyAI()
    {
        return _currentAIAgent;
    }
}
