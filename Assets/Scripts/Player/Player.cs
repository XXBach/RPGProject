using UnityEngine;

public class Player : MonoBehaviour, ICharacter
{
    public CharacterData CurrentDatas { get; set; }
    [SerializeField] private SpawningPosition _spawningPosition;
    [SerializeField] private CharacterBaseStats _baseStats;
    [SerializeField] private AttackSet _attackSet;
    [SerializeField] private CombatVisualData _combatVisualData;
    [SerializeField] private PlayerMovement _movementManager;
    [SerializeField] private PlayerAttackManager _attackManager;
    private void Awake()
    {
        _movementManager ??= GetComponent<PlayerMovement>();
        _attackManager ??= GetComponent<PlayerAttackManager>();
    }
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
        return _movementManager;
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
        return _attackManager;
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
}
