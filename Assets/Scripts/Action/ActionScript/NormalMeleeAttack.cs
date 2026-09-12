using UnityEngine;

public class NormalMeleeAttack : IAction
{
    private ActionData _actionData;
    private int _damage;
    public NormalMeleeAttack(ActionData actionData)
    {
        _actionData = actionData;
    }
    public void PlayAttackAnim()
    {
        // Play attack animation
        Debug.Log("Playing normal melee attack animation");
    }
}
