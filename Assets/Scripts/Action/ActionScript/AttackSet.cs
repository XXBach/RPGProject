using UnityEngine;

[CreateAssetMenu(fileName = "AttackSet", menuName = "Scriptable Objects/AttackSet")]
public class AttackSet : ScriptableObject
{
    [SerializeField] private ActionData _normalAttack;
    [SerializeField] private ActionData _specialAttack;
    [SerializeField] private ActionData _skillAttack;

    public ActionData NormalAttack { get { return _normalAttack; } set { _normalAttack = value; } }
    public ActionData SpecialAttack { get { return _specialAttack; } set { _specialAttack = value; } }
    public ActionData SkillAttack { get { return _skillAttack; } set { _skillAttack = value; } }
}
