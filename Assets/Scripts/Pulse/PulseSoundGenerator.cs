// Scripts/Pulse/PulseSoundGenerator.cs
// 심장박동 사운드를 코드로 직접 생성해서 재생
// - 별도 오디오 파일 없이 사인파로 심장박동 느낌 구현
// - 일반 맥박: 낮고 짧은 "쿵" 소리
// - 큰 박동: 더 낮고 길고 강한 "쿵-쿵" 소리

using UnityEngine;

public class PulseSoundGenerator : MonoBehaviour
{
    [Header("일반 맥박 사운드 설정")]
    [SerializeField] private float normalFrequency = 60f;   // 소리 주파수 (Hz) - 낮을수록 묵직함
    [SerializeField] private float normalDuration = 0.12f;  // 소리 길이 (초)
    [SerializeField] private float normalVolume = 0.6f;     // 볼륨 (0~1)

    [Header("큰 박동 사운드 설정")]
    [SerializeField] private float bigFrequency = 45f;      // 더 낮은 주파수
    [SerializeField] private float bigDuration = 0.18f;     // 더 긴 소리
    [SerializeField] private float bigVolume = 1.0f;        // 더 큰 볼륨

    // 오디오 재생용 컴포넌트 (두 개로 일반/큰 박동 동시 재생 가능하게)
    private AudioSource normalAudioSource;
    private AudioSource bigAudioSource;

    // 생성된 오디오 클립 캐싱 (매번 생성 방지)
    private AudioClip normalClip;
    private AudioClip bigClip;

    void Awake()
    {
        // AudioSource 두 개 동적 추가
        normalAudioSource = gameObject.AddComponent<AudioSource>();
        bigAudioSource    = gameObject.AddComponent<AudioSource>();

        // AudioSource 기본 설정
        normalAudioSource.playOnAwake = false;
        bigAudioSource.playOnAwake    = false;

        // 사운드 클립 미리 생성 (게임 시작 시 한 번만)
        normalClip = GenerateHeartbeatClip(normalFrequency, normalDuration, normalVolume);
        bigClip    = GenerateHeartbeatClip(bigFrequency,    bigDuration,    bigVolume);

        normalAudioSource.clip = normalClip;
        bigAudioSource.clip    = bigClip;
    }

    // 일반 맥박 재생 (PulseController의 onPulse 이벤트에서 호출)
    public void PlayNormalPulse()
    {
        normalAudioSource.Play();
    }

    // 큰 박동 재생 (PulseController의 onBigPulse 이벤트에서 호출)
    public void PlayBigPulse()
    {
        bigAudioSource.Play();
    }

    // 사인파 기반 심장박동 오디오 클립 생성
    // frequency: 소리 높낮이 (낮을수록 묵직한 쿵 소리)
    // duration: 소리 길이 (초)
    // volume: 최대 볼륨
    private AudioClip GenerateHeartbeatClip(float frequency, float duration, float volume)
    {
        int sampleRate  = 44100;                          // CD 음질 기준 샘플레이트
        int sampleCount = (int)(sampleRate * duration);   // 총 샘플 수
        float[] samples = new float[sampleCount];

        for (int i = 0; i < sampleCount; i++)
        {
            float t = (float)i / sampleRate; // 현재 시간 (초)

            // 사인파로 기본 소리 생성
            float sine = Mathf.Sin(2f * Mathf.PI * frequency * t);

            // 엔벨로프: 빠르게 켜지고 천천히 꺼지는 심장박동 곡선
            // Attack(0~10%): 빠르게 켜짐
            // Decay(10~100%): 천천히 꺼짐
            float progress = (float)i / sampleCount;
            float envelope;

            if (progress < 0.1f)
                envelope = progress / 0.1f;                    // 선형 Attack
            else
                envelope = 1f - ((progress - 0.1f) / 0.9f);   // 선형 Decay

            // 엔벨로프에 EaseOut 적용 (더 자연스러운 감쇠)
            envelope = envelope * envelope;

            samples[i] = sine * envelope * volume;
        }

        // AudioClip 생성 및 샘플 데이터 적용
        AudioClip clip = AudioClip.Create("Heartbeat", sampleCount, 1, sampleRate, false);
        clip.SetData(samples, 0);
        return clip;
    }
}