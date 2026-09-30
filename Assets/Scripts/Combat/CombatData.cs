using System.Collections.Generic;
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

    /// <summary>Mục tiêu CHÍNH: mục tiêu hợp lệ đầu tiên (gần nhất) trong vùng effect. Đây là người được hiển thị trong Combat Scene.</summary>
    public CombatParticipantResult Defender;

    /// <summary>Các mục tiêu còn lại trúng AOE/StraightLine: vẫn nhận damage nhưng không có view trong Combat Scene.</summary>
    public List<CombatParticipantResult> AdditionalDefenders = new List<CombatParticipantResult>();

    public ActionData UsedAction;

    /// <summary>
    /// Nhận danh sách kết quả ĐÃ SẮP XẾP theo thứ tự gần -> xa (chỉ chứa mục tiêu hợp lệ).
    /// Phần tử đầu thành Defender, phần còn lại thành AdditionalDefenders.
    /// Danh sách rỗng -> Defender = null (đánh vào khoảng không).
    /// </summary>
    public void SetDefenders(List<CombatParticipantResult> orderedResults)
    {
        AdditionalDefenders = new List<CombatParticipantResult>();
        if (orderedResults == null || orderedResults.Count == 0)
        {
            Defender = null;
            return;
        }

        Defender = orderedResults[0];
        for (int i = 1; i < orderedResults.Count; i++)
        {
            AdditionalDefenders.Add(orderedResults[i]);
        }
    }

    /// <summary>Mục tiêu chính + toàn bộ mục tiêu phụ.</summary>
    public IEnumerable<CombatParticipantResult> GetAllDefenders()
    {
        if (Defender != null) yield return Defender;
        foreach (CombatParticipantResult extra in AdditionalDefenders)
        {
            if (extra != null) yield return extra;
        }
    }
}
