using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;

public class ResultManager : MonoBehaviour
{
    [Header("UI 참조")]
    [SerializeField] private TextMeshProUGUI depthText;
    [SerializeField] private TextMeshProUGUI bestText;
    [SerializeField] private Button restartButton;

    void Start()
    {
        // PlayerPrefs에서 기록 불러오기
        int lastDepth = PlayerPrefs.GetInt("LastDepth", 0);
        int bestDepth = PlayerPrefs.GetInt("BestFloor", 0);

        depthText.text = $"DEPTH: {lastDepth}";
        bestText.text  = $"BEST: {bestDepth}";

        // 재시작 버튼 이벤트 연결
        restartButton.onClick.AddListener(OnRestartClicked);
    }

    private void OnRestartClicked()
    {
        SceneManager.LoadScene("SceneGame");
    }
}