using UnityEngine;
public interface ICharacter
{
    public Vector3 GetCharWorldPosition();
    public IMovement GetMovementManager();
    public IAttackManager GetAttackManager();
    public int GetCurrentSpeed();
    public int GetCurrentMP();
    public string GetName();
    public CharacterData CurrentDatas { get; }
    public AttackSet GetAttackSet();
}
