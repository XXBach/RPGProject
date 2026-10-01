using UnityEngine;
[DefaultExecutionOrder(-100)]
public class Player : MonoBehaviour, ICharacter
{
    public CharacterData CurrentDatas { get; set; }
    [SerializeField] private CharacterBaseStats _baseStats;
    [SerializeField] private AttackSet _attackSet;
    [SerializeField] private CharacterVisualData _visualData;
    [SerializeField] private PlayerMovement _movementManager;
    [SerializeField] private PlayerAttackManager _attackManager;
    [SerializeField] private bool _isKeyPlayer = false;
    public bool IsKeyPlayer => _isKeyPlayer;
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
        CurrentDatas.MaxHealth = _baseStats.MaxHealth;
        CurrentDatas.CurrentAttack = _baseStats.BaseDamage;
        CurrentDatas.CurrentDefense = _baseStats.BaseDef;
        CurrentDatas.CurrentMovementRange = _baseStats.MovementRange;
        CurrentDatas.CurrentSpeed = _baseStats.BaseSpeed;
        CurrentDatas.CurrentMP = _baseStats.MaxMP;
        CurrentDatas.Name = _baseStats.CharName;
        CurrentDatas.CurrentAttackRange = _baseStats.BaseAttackRange;
        CurrentDatas.CharAvt = _baseStats.CharAvt;
        CurrentDatas.MaxMP = _baseStats.MaxMP;
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
    public CharacterVisualData GetCharacterVisualData()
    {
        return this._visualData;
    }
    public EnemyAI? GetEnemyAI()
    {
        return null;
    }
}
