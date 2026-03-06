// Scripts/UI/TitlePulseSound.cs
// 타이틀/결과 씬에서 PulseController 없이 독립적으로 펄스 사운드 재생
// TitleAnimator의 점멸 주기와 동일한 interval로 맞춰서 사용

using UnityEngine;
using System.Collections;
using UnityEngine.Audio;

public class TitlePulseSound : MonoBehaviour
{
    [Header("박동 설정")]
    [SerializeField] private float interval = 1.5f;  // TitleAnimator의 pulseInterval과 동일하게
    [SerializeField] private float frequency = 60f;
    [SerializeField] private float duration = 0.12f;
    [SerializeField] private float volume = 0.4f;    // 인게임보다 약하게

    private AudioSource audioSource;

[Header("AudioMixer 참조")]
[SerializeField] private AudioMixerGroup pulseMixerGroup;

void Awake()
{
    audioSource = gameObject.AddComponent<AudioSource>();
    audioSource.playOnAwake = false;

    // Pulse 믹서 그룹 연결
    audioSource.outputAudioMixerGroup = pulseMixerGroup;

    audioSource.clip = GenerateHeartbeatClip();
}

void Start()
{
    // AudioListener.volume 대신 믹서로 관리하므로 볼륨 직접 설정 불필요
    audioSource.volume = 1f;
}

    public void PlayPulse()
    {
            audioSource.Play();
    }

    private AudioClip GenerateHeartbeatClip()
    {
        int sampleRate  = 44100;
        int sampleCount = (int)(sampleRate * duration);
        float[] samples = new float[sampleCount];

        for (int i = 0; i < sampleCount; i++)
        {
            float t        = (float)i / sampleRate;
            float sine     = Mathf.Sin(2f * Mathf.PI * frequency * t);
            float progress = (float)i / sampleCount;
            float envelope;

            if (progress < 0.1f)
                envelope = progress / 0.1f;
            else
                envelope = 1f - ((progress - 0.1f) / 0.9f);

            envelope   = envelope * envelope;
            samples[i] = sine * envelope * volume;
        }

        AudioClip clip = AudioClip.Create("TitleHeartbeat", sampleCount, 1, sampleRate, false);
        clip.SetData(samples, 0);
        return clip;
    }
}
