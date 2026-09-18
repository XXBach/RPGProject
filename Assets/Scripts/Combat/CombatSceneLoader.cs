using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
public class CombatSceneLoader : MonoBehaviour
{
    [SerializeField] private string _combatSceneName;
    [SerializeField] private TurnManager _turnManager;

    private CombatData _pendingData;

    private void Awake()
    {
        _combatSceneName??= "CombatScene"; 
    }

    public void HandleCombatRequested(CombatData data)
    {
        _pendingData = data;
        PauseBaseGameplay(true);
        StartCoroutine(LoadCombatSceneRoutine());
    }

    private IEnumerator LoadCombatSceneRoutine()
    {
        AsyncOperation op = SceneManager.LoadSceneAsync(_combatSceneName, LoadSceneMode.Additive);
        yield return op;

        Scene combatScene = SceneManager.GetSceneByName(_combatSceneName);
        // Tìm CombatSceneManager trong scene vừa load
        foreach (var root in combatScene.GetRootGameObjects())
        {
            var mgr = root.GetComponentInChildren<CombatSceneManager>();
            if (mgr != null)
            {
                mgr.OnCombatFinished += HandleCombatFinished;
                mgr.StartCombat(_pendingData);
                yield break;
            }
        }
    }

    private void HandleCombatFinished(CombatSceneManager mgr, CombatData result)
    {
        mgr.OnCombatFinished -= HandleCombatFinished;
        StartCoroutine(UnloadCombatSceneRoutine(result));
    }

    private IEnumerator UnloadCombatSceneRoutine(CombatData result)
    {
        yield return SceneManager.UnloadSceneAsync(_combatSceneName);
        PauseBaseGameplay(false);
        CombatSignal.EndCombatScene(result);
    }

    private void PauseBaseGameplay(bool isPaused)
    {
        // Tắt/bật input map chính, hoặc set 1 flag "IsCombatPlaying" mà
        // PlayerMovement/PlayerAttackManager/TurnManager check ở Update()
        if (_turnManager != null) _turnManager.enabled = !isPaused;
    }

    private void OnEnable()
    {
        CombatSignal.CombatRequested += HandleCombatRequested;
    }
    private void OnDisable()
    {
        CombatSignal.CombatRequested -= HandleCombatRequested;
    }
}
