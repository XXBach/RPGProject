using UnityEngine;
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
    [Header("Spawn / Clash Points")]
    [SerializeField] private Transform _leftSpawnPoint;
    [SerializeField] private Transform _rightSpawnPoint;
    [SerializeField] private Transform _leftClashPoint;
    [SerializeField] private Transform _rightClashPoint;

    [Header("Unit Views")]
    [SerializeField] private CombatUnitView _leftUnitView;
    [SerializeField] private CombatUnitView _rightUnitView;

    [Header("Timing")]
    [SerializeField] private float _runSpeed = 8f;
    [SerializeField] private float _fightDuration = 1.2f;
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

        yield return StartCoroutine(RunInPhase());
        yield return StartCoroutine(FightingPhase());
        ApplyDamagePhase();
        yield return new WaitForSeconds(_postDamageDelay);

        OnCombatFinished?.Invoke(this, _data);
    }

    // ---------------- INIT ----------------
    private void SetupViews()
    {
        // Attacker luôn hiện, chọn bên trái/phải theo vị trí lưới thật
        _attackerView = _leftUnitView;
        _defenderView = _rightUnitView;

        var attackerSpawn = _leftSpawnPoint;
        _attackerView.gameObject.SetActive(true);
        _attackerView.Setup(_data.Attacker.CombatParticipant, attackerSpawn.position, isFacingRight: true);

        if (_data.Defender?.CombatParticipant != null)
        {
            var defenderSpawn = _rightSpawnPoint;
            _defenderView.gameObject.SetActive(true);
            _defenderView.Setup(_data.Defender.CombatParticipant, defenderSpawn.position, isFacingRight: false);
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

        // Chạy đồng thời cả 2 bên tới điểm giữa
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
        if (_data.Defender?.CombatParticipant != null)
        {
            // Nếu muốn: defender không chết ngay -> play Hit anim,
            // nếu damage sẽ giết chết -> play Death anim sau delay nhỏ
            _defenderView.PlayHit(_data.Defender.WillDie);
        }
        yield return new WaitForSeconds(_fightDuration);
    }

    // ---------------- APPLY DAMAGE ----------------
    private void ApplyDamagePhase()
    {
        ApplyDamageTo(_data.Attacker);   // trường hợp có phản đòn (counter-attack), damage=0 nếu không có
        ApplyDamageTo(_data.Defender);
    }

    private void ApplyDamageTo(CombatParticipantResult result)
    {
        if (result?.CombatParticipant == null || result.DamageTaken <= 0) return;
        var data = result.CombatParticipant.CurrentDatas;
        data.CurrentHealth = Mathf.Max(0, data.CurrentHealth - result.DamageTaken);
    }
}
