// Scripts/UI/ResultManager.cs
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;
using System.Collections;

public class ResultManager : MonoBehaviour
{
    [Header("텍스트 참조")]
    [SerializeField] private TextMeshProUGUI gameOverText;
    [SerializeField] private TextMeshProUGUI depthText;
    [SerializeField] private TextMeshProUGUI bestText;

    [Header("주간 동굴 전용 UI")]
    [SerializeField] private GameObject weeklyGroup;         // 주간 기록 표시 그룹 (비활성화 기본값)
    [SerializeField] private TextMeshProUGUI weeklyBestText; // "WEEKLY BEST  n" 텍스트

    [Header("버튼 참조")]
    [SerializeField] private Button restartButton;

    [Header("테두리 Image 참조 (4개)")]
    [SerializeField] private Image borderTop;
    [SerializeField] private Image borderBottom;
    [SerializeField] private Image borderLeft;
    [SerializeField] private Image borderRight;

    [Header("박동 설정")]
    [SerializeField] private float pulseInterval = 1.5f;
    [SerializeField] private float fadeInDuration = 0.05f;
    [SerializeField] private float fadeOutDuration = 0.6f;

    [Header("최고기록 갱신 점멸")]
    [SerializeField] private float bestFlashInterval = 0.4f;
    [SerializeField] private int bestFlashCount = 4;

    private Image[] borders;

    void Start()
    {
        // --- 기록 불러오기 ---
        int lastDepth  = PlayerPrefs.GetInt("LastDepth", 0);
        int bestDepth  = PlayerPrefs.GetInt("BestFloor", 0);
        bool isWeekly  = PlayerPrefs.GetInt("LastIsWeekly", 0) == 1;

        depthText.text = $"Depth: {lastDepth}";
        bestText.text  = $"Best: {bestDepth}";

        // 최고기록 갱신 여부 확인
        bool isNewRecord = lastDepth == bestDepth && lastDepth > 0;

        // --- 주간 동굴 전용 UI ---
        if (weeklyGroup != null)
        {
            weeklyGroup.SetActive(isWeekly); // 주간 모드일 때만 표시

            if (isWeekly && weeklyBestText != null)
            {
                // WeeklyCaveManager가 씬에 없을 수 있으므로 PlayerPrefs에서 직접 읽기
                string weekKey = new WeeklyCaveManager().GetThisMondayString(); // 날짜 계산만 재활용
                int weeklyBest = PlayerPrefs.GetInt($"WeeklyBest_{weekKey}", 0);
                weeklyBestText.text = $"Weekly Best  {weeklyBest}";

                // 주간 기록 갱신 시에도 점멸
                bool isNewWeeklyRecord = lastDepth == weeklyBest && lastDepth > 0;
                if (isNewWeeklyRecord)
                    StartCoroutine(FlashTextRoutine(weeklyBestText));
            }
        }

        // --- 버튼 ---
        restartButton.onClick.AddListener(OnRestartClicked);

        // --- 테두리 박동 ---
        borders = new Image[] { borderTop, borderBottom, borderLeft, borderRight };
        SetBorderAlpha(0f);
        StartCoroutine(PulseBorderRoutine());

        // --- 일반 최고기록 갱신 점멸 ---
        if (isNewRecord)
            StartCoroutine(FlashTextRoutine(bestText));
    }

    private IEnumerator PulseBorderRoutine()
    {
        while (true)
        {
            yield return new WaitForSeconds(pulseInterval);
            yield return StartCoroutine(FadeBorder(0f, 1f, fadeInDuration));
            yield return StartCoroutine(FadeBorder(1f, 0f, fadeOutDuration));
        }
    }

    private IEnumerator FadeBorder(float from, float to, float duration)
    {
        float elapsed = 0f;
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / duration);
            float alpha = (to < from)
                ? Mathf.Lerp(from, to, t * t * (3f - 2f * t))
                : Mathf.Lerp(from, to, t);
            SetBorderAlpha(alpha);
            yield return null;
        }
        SetBorderAlpha(to);
    }

    // 텍스트 점멸 코루틴 (bestText / weeklyBestText 공용)
    private IEnumerator FlashTextRoutine(TextMeshProUGUI target)
    {
        for (int i = 0; i < bestFlashCount; i++)
        {
            SetTextAlpha(target, 0f);
            yield return new WaitForSeconds(bestFlashInterval);
            SetTextAlpha(target, 1f);
            yield return new WaitForSeconds(bestFlashInterval);
        }
    }

    private void SetBorderAlpha(float alpha)
    {
        foreach (var border in borders)
        {
            if (border == null) continue;
            Color c = border.color;
            c.a = alpha;
            border.color = c;
        }
    }

    private void SetTextAlpha(TextMeshProUGUI tmp, float alpha)
    {
        if (tmp == null) return;
        Color c = tmp.color;
        c.a = alpha;
        tmp.color = c;
    }

    private void OnRestartClicked()
    {
        SceneManager.LoadScene("SceneTitle");
    }
}