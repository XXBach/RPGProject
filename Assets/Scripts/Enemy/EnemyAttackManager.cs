using UnityEngine;

public class EnemyAttackManager : MonoBehaviour, IAttackManager
{
    public AttackManagerState CurrentAttackManagerState { get; set; }
    private int _skillchoice;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    public void SetCurrentAttackManagerState(AttackManagerState state)
    {
        this.CurrentAttackManagerState = state;
    }
    public void SetCurrentSkillChoice(int SkillChoice)
    {
        this._skillchoice = SkillChoice;
    }
}
