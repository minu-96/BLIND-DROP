using UnityEngine;
using UnityEngine.Events;
using System.Collections;

public class PulseController : MonoBehaviour
{
    [Header("BPM 설정")]
    [SerializeField] private float bpm = 40f;

    [Header("이벤트")]
    public UnityEvent onPulse;        // 일반 맥박
    public UnityEvent onBigPulse;     // 큰 박동 (4번째마다)

    private int pulseCount = 0;
    private Coroutine pulseCoroutine;

    void Start()
    {
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
        StartPulse(); // 새 BPM으로 재시작
    }

    public float GetInterval() => 60f / bpm;

    private IEnumerator PulseLoop()
    {
        while (true)
        {
            yield return new WaitForSeconds(60f / bpm);

            pulseCount++;

            if (pulseCount % 4 == 0)
            {
                onBigPulse?.Invoke();
                Debug.Log($"[Pulse] 큰 박동! ({pulseCount}번째)");
            }
            else
            {
                onPulse?.Invoke();
                Debug.Log($"[Pulse] 일반 맥박 ({pulseCount}번째)");
            }
        }
    }
}