using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class StatsMenuManager : MonoBehaviour
{
    [SerializeField] private Image CharAvatar;
    [SerializeField] private Image HPUI;
    [SerializeField] private Image MPUI;
    [SerializeField] private TextMeshProUGUI AtkStat;
    [SerializeField] private TextMeshProUGUI DefStat;
    [SerializeField] private TextMeshProUGUI SpeedStat;
    public void ShowStatMenuFor(ICharacter activeUnit)
    {
        CharAvatar.sprite = activeUnit.CurrentDatas.CharAvt;
        HPUI.fillAmount = Mathf.Clamp01((float)activeUnit.CurrentDatas.CurrentHealth / activeUnit.CurrentDatas.MaxHealth);
        MPUI.fillAmount = Mathf.Clamp01((float)activeUnit.CurrentDatas.CurrentMP / activeUnit.CurrentDatas.MaxMP);
        AtkStat.text = activeUnit.CurrentDatas.CurrentAttack.ToString();
        DefStat.text = activeUnit.CurrentDatas.CurrentDefense.ToString();
        SpeedStat.text = activeUnit.CurrentDatas.CurrentSpeed.ToString();
    }
}

