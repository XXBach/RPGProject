using UnityEngine;
using UnityEngine.TextCore.Text;

[System.Serializable]
public class CombatParticipantResult
{
    public ICharacter CombatParticipant;
    public int DamageTaken;
    public bool WillDie => CombatParticipant != null && (CombatParticipant.CurrentDatas.CurrentHealth - DamageTaken) <= 0;
}

public class CombatData
{
    public CombatParticipantResult Attacker;
    public CombatParticipantResult Defender;
    public ActionData UsedAction;
}
