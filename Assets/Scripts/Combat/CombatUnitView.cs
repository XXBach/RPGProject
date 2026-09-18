using UnityEngine;

public class CombatUnitView : MonoBehaviour
{
    [SerializeField] private SpriteRenderer _spriteRenderer;
    [SerializeField] private Animator _animator; // dùng AnimatorOverrideController theo từng ICharacter

    // Tên trigger animation - khớp với Animator Controller dùng riêng cho Combat Scene
    private const string TRIGGER_RUN = "Run";
    private const string TRIGGER_IDLE = "Idle";
    private const string TRIGGER_ATTACK = "Attack";
    private const string TRIGGER_HIT = "Hit";
    private const string TRIGGER_DEATH = "Death";

    public void Setup(ICharacter character, Vector3 spawnPosition, bool isFacingRight)
    {
        transform.position = spawnPosition;
        _spriteRenderer.flipX = !isFacingRight;

        // Lấy dữ liệu visual riêng cho combat (xem CombatVisualData bên dưới)
        var visualData = character.GetCombatVisualData();
        if (visualData != null && visualData.OverrideController != null)
        {
            _animator.runtimeAnimatorController = visualData.OverrideController;
        }
        PlayIdle();
    }

    public void PlayRun() => _animator.SetTrigger(TRIGGER_RUN);
    public void PlayIdle() => _animator.SetTrigger(TRIGGER_IDLE);
    public void PlayAttack(ActionData usedAction) => _animator.SetTrigger(TRIGGER_ATTACK);
    // isDying = true để animator chuyển sang Death thay vì Hit thường
    public void PlayHit(bool isDying) => _animator.SetTrigger(isDying ? TRIGGER_DEATH : TRIGGER_HIT);
}
