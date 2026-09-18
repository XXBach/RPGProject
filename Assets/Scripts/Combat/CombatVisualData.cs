using UnityEngine;

[CreateAssetMenu(fileName = "CombatVisualData", menuName = "Scriptable Objects/CombatVisualData")]
public class CombatVisualData : ScriptableObject
{
    [SerializeField] private AnimatorOverrideController _overrideController;
    public AnimatorOverrideController OverrideController => _overrideController; //????? Cái này là cái gì
}
