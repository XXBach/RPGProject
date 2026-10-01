using UnityEngine;

[CreateAssetMenu(fileName = "CharacterVisualData", menuName = "Scriptable Objects/CharacterVisualData")]
public class CharacterVisualData : ScriptableObject
{
    [SerializeField] private AnimatorOverrideController _combatSceneOverrideController;
    [SerializeField] private AnimatorOverrideController _battleSceneOverrideController;
    public AnimatorOverrideController OverrideController => _combatSceneOverrideController;
    public AnimatorOverrideController BattleSceneOverrideController => _battleSceneOverrideController;
}
