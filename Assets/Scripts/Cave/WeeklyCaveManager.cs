// Scripts/Core/WeeklyCaveManager.cs
// 주간 동굴 기록 관리 전담 컴포넌트
// - 이번 주 시드 계산
// - 주간 최고 기록 저장/불러오기
// - 주가 바뀌면 기록 자동 리셋

using UnityEngine;
using System;

public class WeeklyCaveManager : MonoBehaviour
{
    public static WeeklyCaveManager Instance { get; private set; }

    // 이번 주 월요일 날짜 문자열 (예: "20260302") → 기록 키로 사용
    private string thisWeekKey;

    // 이번 주 주간 최고 기록
    public int WeeklyBestFloor { get; private set; } = 0;

    void Awake()
    {
        // 싱글톤 설정
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;

        // 이번 주 월요일 날짜 계산
        thisWeekKey = GetThisMondayString();

        // 이번 주 기록 불러오기
        LoadWeeklyRecord();
    }

    // 이번 주 월요일 날짜 문자열 반환 (예: "20260302")
    public string GetThisMondayString()
    {
        DateTime today = DateTime.Now;
        int daysFromMonday = ((int)today.DayOfWeek - (int)DayOfWeek.Monday + 7) % 7;
        DateTime thisMonday = today.AddDays(-daysFromMonday).Date;
        return thisMonday.ToString("yyyyMMdd");
    }

    // 이번 주 시드 반환 (GameManager와 동일한 계산)
    public int GetWeeklySeed()
    {
        return thisWeekKey.GetHashCode();
    }

    // 주간 기록 불러오기
    // PlayerPrefs 키를 "WeeklyBest_20260302" 형태로 주차별 분리 저장
    private void LoadWeeklyRecord()
    {
        string prefKey = $"WeeklyBest_{thisWeekKey}";
        WeeklyBestFloor = PlayerPrefs.GetInt(prefKey, 0);
        Debug.Log($"[WeeklyCave] 이번 주 키: {thisWeekKey}, 최고 기록: {WeeklyBestFloor}층");
    }

    // 주간 기록 갱신 시도 (GameManager의 OnGameOver에서 호출)
    // 갱신됐으면 true 반환
    public bool TryUpdateWeeklyRecord(int floor)
    {
        if (floor <= WeeklyBestFloor) return false;

        WeeklyBestFloor = floor;
        string prefKey = $"WeeklyBest_{thisWeekKey}";
        PlayerPrefs.SetInt(prefKey, WeeklyBestFloor);
        PlayerPrefs.Save();
        Debug.Log($"[WeeklyCave] 주간 기록 갱신! {WeeklyBestFloor}층");
        return true;
    }
}