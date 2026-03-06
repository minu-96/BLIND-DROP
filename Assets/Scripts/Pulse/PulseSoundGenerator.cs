using UnityEngine;
using UnityEngine.Audio;

public class PulseSoundGenerator : MonoBehaviour
{
    [Header("맥박 사운드 설정")]
    [SerializeField] private float frequency = 60f;     // 소리 주파수
    [SerializeField] private float duration = 0.12f;    // 소리 길이
    [SerializeField] private float volume = 0.6f;       // 볼륨

    [Header("AudioMixer 참조")]
[SerializeField] private AudioMixerGroup pulseMixerGroup; // Pulse 믹서 그룹



    private AudioSource audioSource;
    private AudioClip clip;

   void Awake()
{
    audioSource = gameObject.AddComponent<AudioSource>();
    audioSource.playOnAwake = false;

    // Pulse 믹서 그룹 연결
    audioSource.outputAudioMixerGroup = pulseMixerGroup;

    clip = GenerateHeartbeatClip(frequency, duration, volume);
    audioSource.clip = clip;
}

    // PulseController의 onPulse 이벤트에서 호출
    public void PlayPulse()
    {
        audioSource.Play();
    }

    private AudioClip GenerateHeartbeatClip(float frequency, float duration, float volume)
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

            envelope  = envelope * envelope;
            samples[i] = sine * envelope * volume;
        }

        AudioClip clip = AudioClip.Create("Heartbeat", sampleCount, 1, sampleRate, false);
        clip.SetData(samples, 0);
        return clip;
    }
}