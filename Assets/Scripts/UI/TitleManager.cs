// Scripts/UI/TitleManager.cs
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class TitleManager : MonoBehaviour
{
    [Header("버튼 참조")]
    [SerializeField] private Button normalPlayButton;   // 일반 플레이 버튼
    [SerializeField] private Button weeklyPlayButton;   // 주간 동굴 버튼
    [SerializeField] private Button optionButton;       // 옵션 버튼 추가

    [Header("옵션 참조")]
    [SerializeField] private OptionsManager optionsManager; // 옵션 패널 관리자

    void Start()
    {
        // 버튼 클릭 이벤트 연결
        normalPlayButton.onClick.AddListener(OnNormalPlayClicked);
        weeklyPlayButton.onClick.AddListener(OnWeeklyPlayClicked);
        optionButton.onClick.AddListener(OnOptionClicked); // 옵션 버튼 이벤트 연결
    }

    // 일반 플레이 버튼 클릭
    private void OnNormalPlayClicked()
    {
        // 주간 동굴 모드 OFF로 저장 후 게임 씬 이동
        PlayerPrefs.SetInt("IsWeeklyCave", 0);
        PlayerPrefs.Save();
        SceneManager.LoadScene("SceneGame");
    }

    // 주간 동굴 버튼 클릭
    private void OnWeeklyPlayClicked()
    {
        // 주간 동굴 모드 ON으로 저장 후 게임 씬 이동
        PlayerPrefs.SetInt("IsWeeklyCave", 1);
        PlayerPrefs.Save();
        SceneManager.LoadScene("SceneGame");
    }
    // 옵션 버튼 클릭 시 옵션 패널 열기
    private void OnOptionClicked()
    {
        optionsManager.OpenOptions();
    }
}