using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class TitleManager : MonoBehaviour
{
    [Header("버튼 참조")]
    [SerializeField] private Button normalPlayButton;
    [SerializeField] private Button weeklyPlayButton;
    [SerializeField] private Button tutorialButton;     // 튜토리얼 버튼 추가
    [SerializeField] private Button optionButton;

    [Header("옵션 참조")]
    [SerializeField] private OptionsManager optionsManager;

    void Start()
    {
        normalPlayButton.onClick.AddListener(OnNormalPlayClicked);
        weeklyPlayButton.onClick.AddListener(OnWeeklyPlayClicked);
        tutorialButton.onClick.AddListener(OnTutorialClicked);
        optionButton.onClick.AddListener(OnOptionClicked);

        // 튜토리얼 미완료 시 버튼 강조 (처음 실행 유도)
        bool tutorialDone = PlayerPrefs.GetInt("TutorialDone", 0) == 1;
        if (!tutorialDone)
            HighlightTutorialButton();
    }

    private void OnNormalPlayClicked()
    {
        PlayerPrefs.SetInt("IsWeeklyCave", 0);
        PlayerPrefs.Save();
        SceneManager.LoadScene("SceneGame");
    }

    private void OnWeeklyPlayClicked()
    {
        PlayerPrefs.SetInt("IsWeeklyCave", 1);
        PlayerPrefs.Save();
        SceneManager.LoadScene("SceneGame");
    }

    private void OnTutorialClicked()
    {
        SceneManager.LoadScene("SceneTutorial");
    }

    private void OnOptionClicked()
    {
        optionsManager.OpenOptions();
    }

    // 튜토리얼 미완료 시 버튼 텍스트에 표시
    private void HighlightTutorialButton()
    {
        var tmp = tutorialButton.GetComponentInChildren<TMPro.TextMeshProUGUI>();
        if (tmp != null)
            tmp.text = "* Tutorial";
    }
}