using UnityEngine;

public class CombatUnitView : MonoBehaviour
{
    [SerializeField] private SpriteRenderer _spriteRenderer;
    [SerializeField] private Animator _animator;

    private const string PARAM_IS_RUNNING = "IsRunning"; // FIX: đổi Trigger -> Bool vì Run là state kéo dài
    private const string TRIGGER_ATTACK = "Attack";
    private const string TRIGGER_HIT = "Hit";
    private const string TRIGGER_DEATH = "Death";

    private bool _isFacingRight;

    public void Setup(ICharacter character, Vector3 spawnPosition, bool isFacingRight)
    {
        transform.position = spawnPosition;
        _isFacingRight = isFacingRight;
        _spriteRenderer.flipX = !isFacingRight;

        var visualData = character.GetCombatVisualData();
        if (visualData != null && visualData.OverrideController != null)
        {
            _animator.runtimeAnimatorController = visualData.OverrideController;
        }
        PlayIdle();
    }

    public void SetupVisualOnly(AnimatorOverrideController overrideController, Vector3 spawnPosition, bool isFacingRight)
    {
        transform.position = spawnPosition;
        _isFacingRight = isFacingRight;
        _spriteRenderer.flipX = !isFacingRight;

        if (overrideController != null)
        {
            _animator.runtimeAnimatorController = overrideController;
        }
        PlayIdle();
    }

    // FIX: chạy SAU khi Animator evaluate xong curve của frame -> ép lại flip đúng,
    // bất kể clip Attack/Run có bake sẵn keyframe flipX ghi đè hay không
    private void LateUpdate()
    {
        _spriteRenderer.flipX = !_isFacingRight;
    }

    public void PlayRun() => _animator.SetBool(PARAM_IS_RUNNING, true);
    public void PlayIdle() => _animator.SetBool(PARAM_IS_RUNNING, false);
    public void PlayAttack(ActionData usedAction) => _animator.SetTrigger(TRIGGER_ATTACK);
    public void PlayHit(bool isDying) => _animator.SetTrigger(isDying ? TRIGGER_DEATH : TRIGGER_HIT);
}
