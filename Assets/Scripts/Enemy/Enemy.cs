using UnityEngine;

[System.Serializable]
public class CharacterData
{
    public string Name;
    public int CurrentHealth;
    public int CurrentAttack;
    public int CurrentDefense;
    public int CurrentMovementRange;
    public int CurrentSpeed;
    public int CurrentMP;
    public int CurrentAttackRange;
}
[DefaultExecutionOrder(-100)]
public class Enemy : MonoBehaviour, ICharacter
{
    public CharacterData CurrentDatas { get; set; }
    [SerializeField] private SpawningPosition _spawningPosition;
    [SerializeField] private CharacterBaseStats _baseStats;
    [SerializeField] private AttackSet _attackSet;
    [SerializeField] private CombatVisualData _combatVisualData;
    private EnemyAI _currentAIAgent;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    private void Start()
    {
        CurrentDatas = new CharacterData();
        CurrentDatas.CurrentHealth = _baseStats.MaxHealth;
        CurrentDatas.CurrentAttack = _baseStats.BaseDamage;
        CurrentDatas.CurrentDefense = _baseStats.BaseDef;
        CurrentDatas.CurrentMovementRange = _baseStats.MovementRange;
        CurrentDatas.CurrentSpeed = _baseStats.BaseSpeed;
        CurrentDatas.CurrentMP = _baseStats.MaxMP;
        CurrentDatas.Name = _baseStats.CharName;
        CurrentDatas.CurrentAttackRange = _baseStats.BaseAttackRange;
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
    public CombatVisualData GetCombatVisualData()
    {
        return this._combatVisualData;
    }
    public SpawningPosition GetSpawningPosition()
    {
        return this._spawningPosition;
    }
    public EnemyAI? GetEnemyAI()
    {
        return _currentAIAgent;
    }
}
