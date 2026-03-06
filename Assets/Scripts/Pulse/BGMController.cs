// Scripts/Core/BGMController.cs
// BGM 재생 및 구간별 Pitch 조절로 박자 싱크 유지
// - 기준 BGM은 80 BPM으로 제작
// - 게임 BPM 변경 시 Pitch를 비례해서 조정 (80 BPM 기준)
// - BPM 변경 시 즉시 전환이 아닌 부드럽게 Pitch 보간

using UnityEngine;
using System.Collections;
using UnityEngine.Audio;

public class BGMController : MonoBehaviour
{
    public static BGMController Instance { get; private set; }

    [Header("BGM 설정")]
    [SerializeField] private AudioClip bgmClip;         // BGM 오디오 파일 (80 BPM 기준)
    [SerializeField] private float baseBPM = 80f;       // BGM 원본 BPM (제작 기준)
    [SerializeField] private float pitchLerpSpeed = 2f; // Pitch 전환 속도 (높을수록 빠름)

    private AudioSource audioSource;
    private float targetPitch = 1f;     // 목표 Pitch
    private Coroutine pitchCoroutine;   // Pitch 보간 코루틴

    [Header("AudioMixer 참조")]
[SerializeField] private AudioMixerGroup bgmMixerGroup; // BGM 믹서 그룹 연결

void Awake()
{
    if (Instance != null && Instance != this)
    {
        Destroy(gameObject);
        return;
    }
    Instance = this;

    audioSource = gameObject.AddComponent<AudioSource>();
    audioSource.clip = bgmClip;
    audioSource.loop = true;
    audioSource.playOnAwake = false;

    // BGM 믹서 그룹 연결
    audioSource.outputAudioMixerGroup = bgmMixerGroup;
}
void Start()
{
    audioSource.volume = PlayerPrefs.GetFloat("OptionVolume", 1f);

    // dspTime 기준으로 0.1초 후에 BGM 재생 예약
    // 약간의 여유를 두어 PulseController도 같은 시점을 기준으로 삼게 함
    double startDspTime = AudioSettings.dspTime + 0.3;
    

    // PulseController에 시작 dspTime 전달
    var pulse = FindFirstObjectByType<PulseController>();
    pulse?.StopPulse();
    pulse?.StartPulseAt(startDspTime);

    Debug.Log($"[BGM] 예약 재생 시각: {startDspTime}");
    audioSource.PlayScheduled(startDspTime);
}

    // GameManager에서 BPM 변경 시 호출
    // 현재 게임 BPM을 받아서 BGM Pitch 계산
    public void SetGameBPM(float gameBPM)
    {
        // 목표 Pitch = 현재 게임 BPM / BGM 기준 BPM
        // 예: 게임 BPM 120, 기준 80 → Pitch 1.5
        targetPitch = gameBPM / baseBPM;

        // 부드럽게 Pitch 전환
        if (pitchCoroutine != null)
            StopCoroutine(pitchCoroutine);
        pitchCoroutine = StartCoroutine(LerpPitchRoutine());

        Debug.Log($"[BGM] 게임 BPM {gameBPM} → Pitch {targetPitch}");
    }

    

    // 현재 Pitch에서 목표 Pitch로 부드럽게 보간
    private IEnumerator LerpPitchRoutine()
    {
        while (Mathf.Abs(audioSource.pitch - targetPitch) > 0.01f)
        {
            audioSource.pitch = Mathf.Lerp(
                audioSource.pitch,
                targetPitch,
                Time.deltaTime * pitchLerpSpeed
            );
            yield return null;
        }

        // 목표값으로 고정
        audioSource.pitch = targetPitch;
    }

    // 현재 BGM 재생 위치 기준으로 다음 박자까지 남은 시간 반환
    // PulseController가 첫 박자를 BGM에 맞춰 시작할 때 사용
    public float GetTimeUntilNextBeat(float gameBPM)
    {
        if (audioSource == null || !audioSource.isPlaying) return 0f;

        // BGM의 현재 박자 간격 (초)
        float bgmBPM = gameBPM * 2f;                // 게임 BPM의 2배가 BGM BPM
        float beatInterval = 60f / bgmBPM;          // BGM 한 박자 길이

        // 현재 재생 위치에서 몇 번째 박자인지 계산
        float currentTime = audioSource.time;
        float beatPosition = currentTime % beatInterval; // 현재 박자 내 위치

        // 다음 박자까지 남은 시간
        float timeUntilNextBeat = beatInterval - beatPosition;
        return timeUntilNextBeat;
    }
    // 현재 BGM 재생 위치 기준으로 다음 박자 dspTime 반환
// PulseController가 층 이동 후 싱크 맞출 때 사용
public double GetNextBeatDspTime(float gameBPM)
{
    // BGM BPM = 게임 BPM * 2
    float bgmBPM = gameBPM * 2f;
    float beatInterval = 60f / bgmBPM; // BGM 한 박자 길이 (초)

    // 현재 BGM 재생 위치에서 박자 내 위치 계산
    float currentTime = audioSource.time;
    float beatPosition = currentTime % beatInterval;

    // 다음 박자까지 남은 시간
    float timeUntilNextBeat = beatInterval - beatPosition;

    // dspTime 기준으로 반환
    return AudioSettings.dspTime + timeUntilNextBeat;
}
}