using UnityEngine;

public interface IAttackManager
{
    public void SetCurrentAttackManagerState(AttackManagerState state);
    public void SetCurrentSkillChoice(int SkillChoice);
}
