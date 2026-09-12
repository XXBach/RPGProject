using UnityEngine;
public enum ActionType
{
    SingleAttack = 0,
    StraightLineAttack = 1,
    AreaOfEffectAttack = 2,
}

[CreateAssetMenu(fileName = "ActionData", menuName = "Scriptable Objects/ActionData")]
public class ActionData : ScriptableObject
{
    [SerializeField] private ActionType _actionType;
    [SerializeField] private int _areaOfEffectRange;
    [SerializeField] private float _skillMultiplier;
    [SerializeField] private int _manaCost;
    [SerializeField] private int _cooldownTime;
    [SerializeField] private Sprite _skillIcon;

    public ActionType ActionType {  get { return _actionType; } set { _actionType = value; } }
    public int AreaOfEffectRange {  get { return _areaOfEffectRange; } set { _areaOfEffectRange = value; } }
    public float SkillMultiplier {  get { return _skillMultiplier; } set { _skillMultiplier = value; } }
    public int ManaCost {  get { return _manaCost; } set { _manaCost = value; } }
    public int CooldownTime {  get { return _cooldownTime; } set { _cooldownTime = value; } }
    public Sprite SkillIcon {  get { return _skillIcon; } set { _skillIcon = value; } }
}
