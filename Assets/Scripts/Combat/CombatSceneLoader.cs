using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class CombatSceneLoader : MonoBehaviour
{
    [SerializeField] private string _combatSceneName;
    [SerializeField] private TurnManager _turnManager;
    [SerializeField] private Camera _battleCamera;
    [SerializeField] private Canvas _battleCanvas;
    private CombatData _pendingData;

    private Scene _baseScene;
    private Scene _loadedCombatScene;

    private bool _combatInProgress = false;

    private void Awake()
    {
        if (string.IsNullOrEmpty(_combatSceneName))
        {
            _combatSceneName = "CombatScene";
        }
    }

    public void HandleCombatRequested(CombatData data)
    {
        if (_combatInProgress)
        {
            Debug.LogWarning(
                "[CombatSceneLoader] Combat đang diễn ra, bỏ qua yêu cầu load trùng."
            );
            return;
        }

        _combatInProgress = true;
        _pendingData = data;

        _baseScene = gameObject.scene;

        // Pause Battle
        PauseBaseGameplay(true);
        SetSceneAudioPaused(_baseScene, true);

        // QUAN TRỌNG
        _battleCamera.enabled = false;

        Debug.Log(
            $"[Combat] Battle Camera immediately after disable: " +
            $"{_battleCamera.enabled}"
        );
        _battleCanvas.enabled = false;

        StartCoroutine(LoadCombatSceneRoutine());
    }

    private IEnumerator LoadCombatSceneRoutine()
    {
        AsyncOperation op = SceneManager.LoadSceneAsync(
            _combatSceneName,
            LoadSceneMode.Additive
        );

        yield return op;

        _loadedCombatScene =
            SceneManager.GetSceneByName(_combatSceneName);

        if (!_loadedCombatScene.IsValid() ||
            !_loadedCombatScene.isLoaded)
        {
            Debug.LogError(
                $"Không thể load Combat Scene: {_combatSceneName}"
            );

            RestoreBaseScene();
            yield break;
        }

        SceneManager.SetActiveScene(_loadedCombatScene);

        SetSceneAudioPaused(_loadedCombatScene, false);

        CombatSceneManager combatManager = null;

        foreach (var root in _loadedCombatScene.GetRootGameObjects())
        {
            combatManager =
                root.GetComponentInChildren<CombatSceneManager>(true);

            if (combatManager != null)
                break;
        }

        if (combatManager == null)
        {
            Debug.LogError(
                "Không tìm thấy CombatSceneManager."
            );

            yield return StartCoroutine(
                UnloadCombatSceneRoutine(null)
            );

            yield break;
        }

        combatManager.OnCombatFinished += HandleCombatFinished;

        combatManager.StartCombat(_pendingData);
    }

    private void HandleCombatFinished(
        CombatSceneManager mgr,
        CombatData result
    )
    {
        if (mgr != null)
        {
            mgr.OnCombatFinished -= HandleCombatFinished;
        }

        StartCoroutine(UnloadCombatSceneRoutine(result));
    }

    private IEnumerator UnloadCombatSceneRoutine(CombatData result)
    {
        // Tắt audio Combat
        SetSceneAudioPaused(
            _loadedCombatScene,
            true
        );

        // Trả Active Scene về Battle Scene
        if (_baseScene.IsValid() &&
            _baseScene.isLoaded)
        {
            SceneManager.SetActiveScene(_baseScene);
        }

        // Unload Combat
        if (_loadedCombatScene.IsValid() &&
            _loadedCombatScene.isLoaded)
        {
            yield return SceneManager.UnloadSceneAsync(
                _loadedCombatScene
            );
        }

        // BẬT LẠI BATTLE
        _battleCamera.enabled = true;
        _battleCanvas.enabled = true;

        SetSceneAudioPaused(
            _baseScene,
            false
        );

        PauseBaseGameplay(false);

        CombatSignal.EndCombatScene(result);

        _loadedCombatScene = default;
        _combatInProgress = false;
    }
    /// <summary>
    /// Bật / tắt toàn bộ Camera nằm trong một Scene.
    /// </summary>
    private void SetSceneCameras(Scene scene, bool enabled)
    {
        if (!scene.IsValid() || !scene.isLoaded)
        {
            return;
        }

        foreach (var root in scene.GetRootGameObjects())
        {
            if (root == null)
            {
                continue;
            }

            Camera[] cameras = root.GetComponentsInChildren<Camera>(
                true
            );

            foreach (var camera in cameras)
            {
                if (camera == null)
                {
                    continue;
                }

                camera.enabled = enabled;

                Debug.Log(
                    $"[CombatSceneLoader] " +
                    $"{camera.name} Camera = {enabled}"
                );
            }
        }
    }

    private void PauseBaseGameplay(bool isPaused)
    {
        if (_turnManager != null)
        {
            _turnManager.enabled = !isPaused;
        }
    }

    private void SetSceneAudioPaused(
        Scene scene,
        bool paused
    )
    {
        if (!scene.IsValid() || !scene.isLoaded)
        {
            return;
        }

        try
        {
            foreach (var root in scene.GetRootGameObjects())
            {
                if (root == null)
                {
                    continue;
                }

                AudioSource[] sources =
                    root.GetComponentsInChildren<AudioSource>(true);

                foreach (var source in sources)
                {
                    if (source == null)
                    {
                        continue;
                    }

                    if (paused)
                    {
                        source.Pause();
                    }
                    else
                    {
                        source.UnPause();
                    }
                }
            }
        }
        catch (System.Exception e)
        {
            Debug.LogWarning(
                $"[CombatSceneLoader] " +
                $"SetSceneAudioPaused lỗi nhưng bỏ qua: {e}"
            );
        }
    }

    private void RestoreBaseScene()
    {
        if (_baseScene.IsValid() && _baseScene.isLoaded)
        {
            SceneManager.SetActiveScene(_baseScene);

            SetSceneCameras(_baseScene, true);
            SetSceneAudioPaused(_baseScene, false);

            PauseBaseGameplay(false);
        }
    }

    private void SetSceneCanvases(Scene scene, bool enabled)
    {
        if (!scene.IsValid() || !scene.isLoaded)
            return;

        foreach (var root in scene.GetRootGameObjects())
        {
            if (root == null)
                continue;

            Canvas[] canvases =
                root.GetComponentsInChildren<Canvas>(true);

            foreach (var canvas in canvases)
            {
                if (canvas == null)
                    continue;

                // Chỉ xử lý Canvas Overlay
                if (canvas.renderMode == RenderMode.ScreenSpaceOverlay)
                {
                    canvas.enabled = enabled;

                    Debug.Log(
                        $"[CombatSceneLoader] " +
                        $"{canvas.name} Canvas = {enabled}"
                    );
                }
            }
        }
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