using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
public class MainMenuManager : MonoBehaviour
{
    [SerializeField] private Button _playButton;
    [SerializeField] private Button _optionButton;
    [SerializeField] private Button _quitButton;

    private void Start()
    {
        _playButton.onClick.AddListener(OnPlayButtonClicked);
        _optionButton.onClick.AddListener(OnOptionButtonClicked);
        _quitButton.onClick.AddListener(OnQuitButtonClicked);
    }
    private void OnPlayButtonClicked()
    {
        SceneManager.LoadScene("BattleScene");
    }
    private void OnOptionButtonClicked()
    {

    }
    private void OnQuitButtonClicked()
    {
        Application.Quit();
    }
}
