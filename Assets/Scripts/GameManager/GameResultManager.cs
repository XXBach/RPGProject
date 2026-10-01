using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

/// <summary>
/// Quản lý điều kiện thắng / thua của BattleScene.
///  - THẮNG: không còn Enemy nào trên sân (kể cả khi chưa tới turn 6).
///  - THUA : Key Player chết (Player có IsKeyPlayer = true).
/// Thua luôn được ưu tiên nếu Key Player và Enemy cuối cùng chết cùng lúc.
///
/// Quan trọng: kết quả chỉ được chốt khi KHÔNG còn Combat Scene đang chạy,
/// vì Combat Scene trừ máu trước khi unload và dùng WaitForSeconds (phụ thuộc timeScale).
/// </summary>
public class GameResultManager : MonoBehaviour
{
    [Header("Result Music")]
    [SerializeField] private AudioSource _musicSource;           // AudioSource riêng để phát nhạc kết quả
    [SerializeField] private AudioClip _victoryMusic;
    [SerializeField] private AudioClip _gameOverMusic;
    [SerializeField] private bool _loopResultMusic = false;
    [Tooltip("Tùy chọn: AudioSource đang phát nhạc nền của trận, sẽ được tắt khi hiện kết quả")]
    [SerializeField] private AudioSource _backgroundMusicToStop;

    [Header("References")]
    [SerializeField] private TurnManager _turnManager;

    [Header("Victory UI")]
    [SerializeField] private GameObject _victoryPanel;
    [SerializeField] private Button _victoryOkButton;

    [Header("Game Over UI")]
    [SerializeField] private GameObject _gameOverPanel;
    [SerializeField] private Button _restartButton;
    [SerializeField] private Button _exitButton;

    [Header("Scenes")]
    [Tooltip("Tên scene MainMenu - nhớ thêm vào Build Settings")]
    [SerializeField] private string _mainMenuSceneName = "MainMenu";

    private bool _isCombatActive = false;
    private bool _keyPlayerDied = false;
    private bool _enemyDied = false;
    private bool _resultShown = false;

    private void Awake()
    {
        Time.timeScale = 1f; // phòng trường hợp scene trước để lại timeScale = 0

        if (_turnManager == null) _turnManager = FindAnyObjectByType<TurnManager>();

        _victoryOkButton.onClick.AddListener(LoadMainMenu);
        _restartButton.onClick.AddListener(RestartScene);
        _exitButton.onClick.AddListener(LoadMainMenu);

        _victoryPanel.SetActive(false);
        _gameOverPanel.SetActive(false);
    }

    private void OnEnable()
    {
        CharacterSignal.CharacterDied += HandleCharacterDied;
        CombatSignal.CombatRequested += HandleCombatRequested;
        CombatSignal.CombatEnded += HandleCombatEnded;
    }

    private void OnDisable()
    {
        CharacterSignal.CharacterDied -= HandleCharacterDied;
        CombatSignal.CombatRequested -= HandleCombatRequested;
        CombatSignal.CombatEnded -= HandleCombatEnded;
    }

    private void HandleCombatRequested(CombatData data) { _isCombatActive = true; }
    private void HandleCombatEnded(CombatData data) { _isCombatActive = false; }

    private void HandleCharacterDied(ICharacter character)
    {
        if (_resultShown) return;

        if (character is Player player && player.IsKeyPlayer)
        {
            _keyPlayerDied = true;
        }
        else if (character is Enemy)
        {
            _enemyDied = true;
        }
    }

    // LateUpdate: đảm bảo mọi CharacterHealthBar.Update trong frame đã chạy xong
    // (VD AOE giết nhiều unit cùng lúc) trước khi đếm.
    private void LateUpdate()
    {
        if (_resultShown || _isCombatActive) return;

        if (_keyPlayerDied)
        {
            ShowGameOver();
        }
        else if (_enemyDied)
        {
            if (_turnManager.CountAliveEnemies() == 0) ShowVictory();
            else _enemyDied = false; // còn địch -> đợi lần chết tiếp theo
        }
    }

    private void ShowVictory()
    {
        FreezeGame();
        _victoryPanel.transform.SetAsLastSibling();
        _victoryPanel.SetActive(true);
        PlayResultMusic(_victoryMusic);      // THÊM
    }

    private void ShowGameOver()
    {
        FreezeGame();
        _gameOverPanel.transform.SetAsLastSibling();
        _gameOverPanel.SetActive(true);
        PlayResultMusic(_gameOverMusic);     // THÊM
    }

    private void FreezeGame()
    {
        _resultShown = true;
        if (_turnManager != null) _turnManager.enabled = false; // dừng chuyển lượt + dừng spawn
        Time.timeScale = 0f;                                    // dừng di chuyển / animation; UI Button vẫn bấm được
    }

    private void LoadMainMenu()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(_mainMenuSceneName);
    }

    private void RestartScene()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(gameObject.scene.name);
    }
    private void PlayResultMusic(AudioClip clip)
    {
        if (_backgroundMusicToStop != null) _backgroundMusicToStop.Stop();
        if (_musicSource == null || clip == null) return;

        _musicSource.Stop();
        _musicSource.clip = clip;
        _musicSource.loop = _loopResultMusic;
        _musicSource.Play();
    }
}