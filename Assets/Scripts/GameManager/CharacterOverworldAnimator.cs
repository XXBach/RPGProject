using UnityEngine;

public enum FacingDirection { Down, Up, Left, Right }

[RequireComponent(typeof(Animator))]
public class CharacterOverworldAnimator : MonoBehaviour
{
    // Tên state CHUNG: mọi Override Controller đều phải giữ nguyên các tên này
    private static readonly int IDLE = Animator.StringToHash("Idle");
    private static readonly int WALK_DOWN = Animator.StringToHash("Walk_Down");
    private static readonly int WALK_UP = Animator.StringToHash("Walk_Up");
    private static readonly int WALK_LEFT = Animator.StringToHash("Walk_Left");
    private static readonly int WALK_RIGHT = Animator.StringToHash("Walk_Right");

    private Animator _animator;
    private int _currentStateHash = -1;

    private void Awake()
    {
        _animator = GetComponent<Animator>();

        ICharacter character = GetComponent<ICharacter>();
        CharacterVisualData visual = character?.GetCharacterVisualData();

        if (visual == null)
        {
            Debug.LogWarning($"[{name}] Chưa gán CharacterVisualData trên Player/Enemy.", this);
            return;
        }
        if (visual.BattleSceneOverrideController == null)
        {
            Debug.LogWarning($"[{name}] {visual.name} chưa gán Battle Scene Override Controller.", this);
            return;
        }
        _animator.runtimeAnimatorController = visual.BattleSceneOverrideController;
        Debug.Log($"[{name}] Dùng controller: {_animator.runtimeAnimatorController.name}", this);
    }

    public void PlayIdle() => PlayState(IDLE);

    /// <summary>Phát anim đi bộ theo hướng từ ô 'from' sang ô 'to'.</summary>
    public void PlayWalk(Vector2Int from, Vector2Int to)
    {
        if (to.x > from.x) PlayState(WALK_RIGHT);
        else if (to.x < from.x) PlayState(WALK_LEFT);
        else if (to.y > from.y) PlayState(WALK_UP);
        else if (to.y < from.y) PlayState(WALK_DOWN);
    }

    public void PlayWalk(FacingDirection dir)
    {
        switch (dir)
        {
            case FacingDirection.Down: PlayState(WALK_DOWN); break;
            case FacingDirection.Up: PlayState(WALK_UP); break;
            case FacingDirection.Left: PlayState(WALK_LEFT); break;
            case FacingDirection.Right: PlayState(WALK_RIGHT); break;
        }
    }

    private void PlayState(int hash)
    {
        if (_currentStateHash == hash) return;
        if (!_animator.HasState(0, hash))
        {
            Debug.LogWarning($"[{name}] Animator không có state này (sai tên state?).", this);
            return;
        }
        _currentStateHash = hash;
        _animator.Play(hash);
    }
}