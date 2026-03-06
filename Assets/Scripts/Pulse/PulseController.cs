using UnityEngine;
using UnityEngine.Events;
using System.Collections;

public class PulseController : MonoBehaviour
{
    [Header("BPM 설정")]
    [SerializeField] private float bpm = 40f;

    [Header("이벤트")]
    public UnityEvent onPulse; // 일반 맥박만 사용

    private int pulseCount = 0;
    private Coroutine pulseCoroutine;
    private DarknessLayer darknessLayer;

    void Start()
    {
        darknessLayer = FindFirstObjectByType<DarknessLayer>();
        StartPulse();
    }

    public void StartPulse()
    {
        if (pulseCoroutine != null)
            StopCoroutine(pulseCoroutine);
        pulseCoroutine = StartCoroutine(PulseLoop());
    }

    public void StopPulse()
    {
        if (pulseCoroutine != null)
            StopCoroutine(pulseCoroutine);
    }

    public void SetBPM(float newBPM)
{
    bpm = newBPM;

    // BGMController가 있으면 현재 BGM 박자에 맞춰 재동기화
    // 없으면 즉시 시작
    if (BGMController.Instance != null)
    {
        double nextBeatDspTime = BGMController.Instance.GetNextBeatDspTime(newBPM);
        StartPulseAt(nextBeatDspTime);
    }
    else
    {
        StartPulse();
    }
}

    public float GetInterval() => 60f / bpm;

    private IEnumerator PulseLoop()
    {
        while (true)
        {
            yield return new WaitForSeconds(60f / bpm);
            pulseCount++;

            // 모든 박동을 일반 맥박으로 처리
            onPulse?.Invoke();
            darknessLayer?.OnPulseFlash();
            Debug.Log($"[Pulse] 맥박 ({pulseCount}번째)");
        }
    }
    // dspTime 기준으로 정확한 시점에 펄스 시작
public void StartPulseAt(double dspStartTime)
{
    if (pulseCoroutine != null)
        StopCoroutine(pulseCoroutine);
    pulseCoroutine = StartCoroutine(PulseLoopAt(dspStartTime));
}

private IEnumerator PulseLoopAt(double dspStartTime)
{
    // BGM 재생 시작 시점까지 대기
    while (AudioSettings.dspTime <= dspStartTime)
        yield return null;

    // 이후 일반 루프
    while (true)
    {
        yield return new WaitForSeconds(60f / bpm);
        pulseCount++;

        onPulse?.Invoke();
        darknessLayer?.OnPulseFlash();
    }
}
}