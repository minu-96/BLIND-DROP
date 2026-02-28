using UnityEngine;
using System.Collections.Generic;

public class FragmentManager : MonoBehaviour
{
    [Header("참조")]
    [SerializeField] private HealthManager healthManager; // 보너스 체력 추가용

    // 현재 플레이 내 수집한 파편 수 (2개마다 보너스 체력 +1)
    private int currentPlayFragments = 0;

    // 전체 누적 수집 파편 수 (음파 모양 해금용, 저장됨)
    private int totalFragments = 0;

    // 음파 모양 해금 조건 테이블
    // Key: 해금에 필요한 누적 파편 수, Value: 해금되는 음파 이름
    private readonly (int required, string echoName)[] unlockTable =
    {
        (0,  "기본"),      // 시작부터 사용 가능
        (3,  "각진 메아리"), // 25층 이하에서 파편 3개 수집
        (5,  "별빛 에코"),  // 50층 이하에서 파편 5개 수집
        (10, "무한 나선"),  // 100층 이하에서 파편 10개 수집
    };

    // 현재 해금된 음파 목록 (UI 표시용)
    public List<string> unlockedEchos { get; private set; } = new List<string>();

    // 파편 수집 시 UI에 알리는 이벤트
    public System.Action onFragmentCollected;

    // 음파 해금 시 UI에 알리는 이벤트 (해금된 음파 이름 전달)
    public System.Action<string> onEchoUnlocked;

    void Start()
    {
        // 저장된 누적 파편 수와 해금 목록 불러오기
        totalFragments = PlayerPrefs.GetInt("TotalFragments", 0);
        LoadUnlockedEchos();

        Debug.Log($"[Fragment] 누적 파편: {totalFragments}개, 해금 음파: {unlockedEchos.Count}개");
    }

    public void StartNewPlay()
    {
        // 새 플레이 시작 시 현재 플레이 파편 수만 초기화
        // 누적 파편과 해금 목록은 유지
        currentPlayFragments = 0;
        Debug.Log("[Fragment] 새 플레이 시작 - 현재 파편 초기화");
    }

    public void CollectFragment()
    {
        // 파편 1개 수집
        currentPlayFragments++;
        totalFragments++;

        Debug.Log($"[Fragment] 파편 수집! 현재 플레이: {currentPlayFragments}개, 누적: {totalFragments}개");

        // 현재 플레이에서 2개 수집마다 보너스 체력 +1
        if (currentPlayFragments % 2 == 0)
        {
            healthManager.AddBonusHealth();
            Debug.Log("[Fragment] 파편 2개 달성 - 보너스 체력 추가!");
        }

        // 누적 파편으로 음파 모양 해금 체크
        CheckEchoUnlock();

        // 누적 파편 수 저장
        PlayerPrefs.SetInt("TotalFragments", totalFragments);
        PlayerPrefs.Save();

        onFragmentCollected?.Invoke();
    }

    private void CheckEchoUnlock()
    {
        // 누적 파편 수 기준으로 해금 조건 확인
        foreach (var entry in unlockTable)
        {
            // 이미 해금된 음파는 스킵
            if (unlockedEchos.Contains(entry.echoName)) continue;

            // 조건 충족 시 해금
            if (totalFragments >= entry.required)
            {
                unlockedEchos.Add(entry.echoName);

                // 해금 목록 저장
                SaveUnlockedEchos();

                Debug.Log($"[Fragment] 음파 해금! {entry.echoName}");
                onEchoUnlocked?.Invoke(entry.echoName);
            }
        }
    }

    private void SaveUnlockedEchos()
    {
        // 해금된 음파 목록을 쉼표로 구분해서 저장
        // 예: "기본,각진 메아리,별빛 에코"
        string saved = string.Join(",", unlockedEchos);
        PlayerPrefs.SetString("UnlockedEchos", saved);
        PlayerPrefs.Save();
    }

    private void LoadUnlockedEchos()
    {
        // 저장된 해금 목록 불러오기
        unlockedEchos.Clear();

        string saved = PlayerPrefs.GetString("UnlockedEchos", "기본");

        // 쉼표로 분리해서 목록 복원
        string[] echos = saved.Split(',');
        foreach (string echo in echos)
        {
            if (!string.IsNullOrEmpty(echo))
                unlockedEchos.Add(echo);
        }

        // 기본 음파는 항상 해금 상태
        if (!unlockedEchos.Contains("기본"))
            unlockedEchos.Add("기본");
    }

    // 외부에서 현재 플레이 파편 수 확인용 (UI용)
    public int GetCurrentPlayFragments() => currentPlayFragments;

    // 외부에서 누적 파편 수 확인용 (UI용)
    public int GetTotalFragments() => totalFragments;
}