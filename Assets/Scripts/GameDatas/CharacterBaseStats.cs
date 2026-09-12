using UnityEngine;

[CreateAssetMenu(fileName = "CharacterBaseStats", menuName = "Scriptable Objects/CharacterBaseStats")]
public class CharacterBaseStats : ScriptableObject
{
    [SerializeField] private string _charName;
    [SerializeField] private int _maxHealth;
    [SerializeField] private int _baseDamage;
    [SerializeField] private int _baseDef;
    [SerializeField] private int _baseSpeed;
    [SerializeField] private int _baseAttackRange;
    [SerializeField] private int _movementRange;
    [SerializeField] private int _maxMP;

    public string CharName => _charName;
    public int MaxHealth => _maxHealth;
    public int BaseDamage => _baseDamage;
    public int BaseDef => _baseDef;
    public int BaseSpeed => _baseSpeed;
    public int BaseAttackRange => _baseAttackRange;
    public int MovementRange => _movementRange;
    public int MaxMP => _maxMP;
}
