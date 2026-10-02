using System;
using System.Collections;
using UnityEngine;

public enum CombatPhase
{
    INIT = 0,
    RUN_IN = 1,
    FIGHTING = 2,
    APPLY_DAMAGE = 3,
    RUN_OUT = 4,
    DONE = 5,
}

public class CombatSceneManager : MonoBehaviour
{
    [Header("Info Banner")]
    [SerializeField] private CombatInfoBanner _infoBanner;

    [Header("Spawn / Clash Points")]
    [SerializeField] private Transform _leftSpawnPoint;
    [SerializeField] private Transform _rightSpawnPoint;
    [SerializeField] private Transform _leftClashPoint;
    [SerializeField] private Transform _rightClashPoint;

    [Header("Unit Views")]
    [SerializeField] private CombatUnitView _leftUnitView;
    [SerializeField] private CombatUnitView _rightUnitView;

    [Header("Damage Popup")]
    [SerializeField] private DamagePopupSpawner _popupSpawner; // NEW

    [Header("Timing")]
    [SerializeField] private float _runSpeed = 8f;
    [SerializeField] private float _firstHitDelay = 0.35f;   // NEW: chờ anim Attack vung tới đoạn chạm
    [SerializeField] private float _hitInterval = 0.3f;      // NEW: khoảng cách giữa các hit
    [SerializeField] private float _tailDuration = 0.8f;     // NEW: chờ sau hit cuối cho popup diễn xong (thay _fightDuration)
    [SerializeField] private float _postDamageDelay = 0.4f;

    public event Action<CombatSceneManager, CombatData> OnCombatFinished;

    private CombatData _data;
    private CombatUnitView _attackerView;
    private CombatUnitView _defenderView; // có thể null nếu không có Defender

    public void StartCombat(CombatData data)
    {
        _data = data;
        StartCoroutine(RunCombatSequence());
    }

    private IEnumerator RunCombatSequence()
    {
        SetupViews();
        ShowInfoBanner();                       // MỚI

        yield return StartCoroutine(RunInPhase());
        yield return StartCoroutine(FightingPhase());
        ApplyDamagePhase();
        _infoBanner?.Hide();                    // MỚI: ẩn khi đã xong đòn đánh
        yield return new WaitForSeconds(_postDamageDelay);

        OnCombatFinished?.Invoke(this, _data);
    }

    // ---------------- INIT ----------------
    private void SetupViews()
    {
        _attackerView = _leftUnitView;
        _defenderView = _rightUnitView;

        var attackerSpawn = _leftSpawnPoint;
        _attackerView.gameObject.SetActive(true);
        _attackerView.Setup(_data.Attacker.CombatParticipant, attackerSpawn.position, isFacingRight: false);

        if (_data.Defender?.CombatParticipant != null)
        {
            var defenderSpawn = _rightSpawnPoint;
            _defenderView.gameObject.SetActive(true);
            _defenderView.Setup(_data.Defender.CombatParticipant, defenderSpawn.position, isFacingRight: true);
        }
        else
        {
            _defenderView.gameObject.SetActive(false);
        }
    }

    // ---------------- RUN IN ----------------
    private IEnumerator RunInPhase()
    {
        _attackerView.PlayRun();
        _defenderView?.PlayRun();

        Vector3 attackerTarget = _leftClashPoint.position;
        Vector3 defenderTarget = _rightClashPoint.position;

        while (true)
        {
            bool attackerArrived = MoveTowards(_attackerView.transform, attackerTarget);
            bool defenderArrived = _defenderView == null || !_defenderView.gameObject.activeSelf
                                     || MoveTowards(_defenderView.transform, defenderTarget);

            if (attackerArrived && defenderArrived) break;
            yield return null;
        }

        _attackerView.PlayIdle();
        _defenderView?.PlayIdle();
    }

    private bool MoveTowards(Transform t, Vector3 target)
    {
        t.position = Vector3.MoveTowards(t.position, target, _runSpeed * Time.deltaTime);
        return Vector3.Distance(t.position, target) < 0.02f;
    }

    // ---------------- FIGHTING ----------------
    private IEnumerator FightingPhase()
    {
        _attackerView.PlayAttack(_data.UsedAction);

        bool hasDefender = _data.Defender?.CombatParticipant != null;
        if (!hasDefender)
        {
            yield return new WaitForSeconds(_firstHitDelay + _tailDuration);
            yield break;
        }

        // Chia tổng damage thành nhiều hit
        int hitCount = _data.UsedAction != null ? Mathf.Max(1, _data.UsedAction.HitCount) : 1;
        int[] hitDamages = SplitDamage(Mathf.Max(0, _data.Defender.DamageTaken), hitCount);

        yield return new WaitForSeconds(_firstHitDelay);

        for (int i = 0; i < hitCount; i++)
        {
            bool isLastHit = i == hitCount - 1;

            // Chỉ hit cuối mới chơi Death (nếu chết), các hit trước chơi Hit bình thường
            _defenderView.PlayHit(isLastHit && _data.Defender.WillDie);
            _popupSpawner.Spawn(_defenderView.PopupWorldPosition, hitDamages[i], isLastHit);

            if (!isLastHit)
                yield return new WaitForSeconds(_hitInterval);
        }

        yield return new WaitForSeconds(_tailDuration);
    }

    /// <summary>
    /// Chia đều total thành count phần, phần dư dồn vào các hit đầu. Tổng luôn = total.
    /// VD: 10 chia 3 -> [4, 3, 3]
    /// </summary>
    private int[] SplitDamage(int total, int count)
    {
        int[] result = new int[count];
        int baseValue = total / count;
        int remainder = total % count;
        for (int i = 0; i < count; i++)
            result[i] = baseValue + (i < remainder ? 1 : 0);
        return result;
    }

    // ---------------- APPLY DAMAGE ----------------
    private void ApplyDamagePhase()
    {
        ApplyDamageTo(_data.Attacker);

        // FIX: áp damage cho mục tiêu chính VÀ toàn bộ mục tiêu phụ trúng AOE/StraightLine
        foreach (CombatParticipantResult defender in _data.GetAllDefenders())
        {
            ApplyDamageTo(defender);
        }
    }

    private void ApplyDamageTo(CombatParticipantResult result)
    {
        if (result?.CombatParticipant == null || result.DamageTaken <= 0) return;
        var data = result.CombatParticipant.CurrentDatas;
        data.CurrentHealth = Mathf.Max(0, data.CurrentHealth - result.DamageTaken);
    }

    private void ShowInfoBanner()
    {
        if (_infoBanner == null) return;

        ICharacter attacker = _data.Attacker?.CombatParticipant;
        string attackerName = attacker != null ? attacker.GetName() : "???";
        string skillName = _data.UsedAction != null ? _data.UsedAction.ActionName : "";

        _infoBanner.Show(attackerName, skillName);
    }
}
