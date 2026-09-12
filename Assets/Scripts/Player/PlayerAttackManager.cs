using System.Collections.Generic;
using System.Net;
using Unity.VisualScripting;
using UnityEngine;

public enum AttackManagerState
{
    IDLE = 0,
    ATTACKMENUSHOW = 1,
    ATTACKRANGEVISUALIZE = 2,

}
public class PlayerAttackManager : MonoBehaviour, IAttackManager
{
    public AttackManagerState CurrentAttackManagerState { get; set; }
    private ICharacter CurrentPlayer { get; set; }

    private PlayerAttackMenu _attackMenu;
    private void Awake()
    {
        CurrentAttackManagerState = AttackManagerState.IDLE;
        
    }
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        CurrentPlayer = GetComponent<Player>();
    }

    // Update is called once per frame
    void Update()
    {


        //Nếu không có target nào trên ô đó thì hiện là không attack được, gọi hiện menu lại một lần nữa
        //Nếu chọn ở ngoài vùng attack khả dĩ thì hiện là không attack được, gọi hiện menu lại một lần nữa
        //Về việc kiểm tra range này, nếu hành động là single attack thì check đơn giản, nếu hành động là straightline attack hoặc AOE thì yêu cầu phải có ít nhất 1 character mục tiêu nằm trong range
        //Nếu thỏa mãn toàn bộ điều kiện trên thì tiến hành handle attack, chúng sẽ không khác nhau về công thức tính damage, chỉ khác nhau về số lần lặp lại trên vùng attack đã được chọn
        switch (CurrentAttackManagerState)
        {
            case (AttackManagerState.IDLE):
                {
                    return;
                }
            case (AttackManagerState.ATTACKMENUSHOW):
                {
                    Debug.Log(CurrentPlayer);
                    _attackMenu.ShowMenuFor(CurrentPlayer);
                    break;
                }
            default:
                {
                    return;
                }
        }

    }
    public bool isAttackInRange(Vector3 targetPosition, ActionData actionData)
    {
        float distance = Vector3.Distance(transform.position, targetPosition);
        return distance <= GetComponent<Player>().CurrentDatas.CurrentAttackRange * actionData.AreaOfEffectRange;
    }


    private void ExecuteSingleTargetAttack(ActionData actionData, ICharacter target)
    {

    }

    private void HandleSingleAttackChoice(ActionData actionData, ICharacter target)
    {
        //Lấy hành động được chọn, gọi hàm hiện range của hành động
        
        //Để player chọn thêm 1 lần nữa, gọi hàm kiểm tra
    }
    private void SkillRangeVisualize(ActionData actionData) {
        //Hàm hiện range
        Vector2Int playerPosition = GridSetup.Grid.GetGridPosition(this.transform.position);
        List<PathNode> ReachableNodes = CurrentPlayer.GetMovementManager().GetPathFinding().GetReachableNodes(playerPosition.x, playerPosition.y, CurrentPlayer.CurrentDatas.CurrentAttackRange);
    }
    public void SetCurrentAttackManagerState(AttackManagerState state) { 
        this.CurrentAttackManagerState = state;
    }
}
