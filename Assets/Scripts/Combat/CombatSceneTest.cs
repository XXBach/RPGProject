using UnityEngine;
using UnityEngine.InputSystem;

public class CombatSceneTest : MonoBehaviour
{
    [SerializeField] private CombatUnitView _view;
    [SerializeField] private CombatVisualData _testVisual;
    [SerializeField] private InputActionAsset _inputActionAsset;

    private InputAction _runAction;
    private InputAction _attackAction;
    private InputAction _hitAction;
    private InputAction _deathAction;

    private void Awake()
    {
        // Nếu chưa có action map/action riêng cho test, tạo tạm 1 map "Debug"
        // trong InputActionAsset với các action Run/Attack/Hit/Death, binding phím tuỳ ý (R, A, H, K...)
        _runAction = _inputActionAsset.FindAction("Run");
        _attackAction = _inputActionAsset.FindAction("Attack");
        _hitAction = _inputActionAsset.FindAction("Hit");
        _deathAction = _inputActionAsset.FindAction("Death");
    }

    private void Start()
    {
        // Gán trực tiếp override controller để test, không cần ICharacter thật
        if (_testVisual != null && _testVisual.OverrideController != null)
        {
            _view.SetupVisualOnly(_testVisual.OverrideController, transform.position, isFacingRight: true);
        }
    }

    private void Update()
    {
        if (_runAction.WasPressedThisFrame()) _view.PlayRun();
        if (_attackAction.WasPressedThisFrame()) _view.PlayAttack(null);
        if (_hitAction.WasPressedThisFrame()) _view.PlayHit(isDying: false);
        if (_deathAction.WasPressedThisFrame()) _view.PlayHit(isDying: true);
    }

    private void OnEnable()
    {
        _inputActionAsset.FindActionMap("Debug").Enable();
    }
    private void OnDisable()
    {
        _inputActionAsset.FindActionMap("Debug").Disable();
    }
}
