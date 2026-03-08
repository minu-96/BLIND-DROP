// Scripts/UI/OptionsManager.cs
// 옵션 패널 전체 관리
// - BGM / SFX / Pulse 볼륨 슬라이더 (AudioMixer 연동)
// - 밝기 슬라이더
// - 해상도 드롭다운
// - X버튼으로 패널 닫기

using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Audio;
using UnityEngine.Rendering.Universal;
using TMPro;

public class OptionsManager : MonoBehaviour
{
    [Header("패널 참조")]
    [SerializeField] private GameObject optionsPanel;

    [Header("AudioMixer 참조")]
    [SerializeField] private AudioMixer mainMixer;      // MainMixer 에셋 연결

    [Header("슬라이더 참조")]
    [SerializeField] private Slider bgmSlider;          // 배경음 슬라이더
    [SerializeField] private Slider sfxSlider;          // 효과음 슬라이더
    [SerializeField] private Slider pulseSlider;        // 펄스 슬라이더
    [SerializeField] private Slider brightnessSlider;   // 밝기 슬라이더

    [Header("드롭다운 참조")]
    [SerializeField] private TMP_Dropdown resolutionDropdown;

    [Header("버튼 참조")]
    [SerializeField] private Button closeButton;

    [Header("밝기 설정")]
    [SerializeField] private Light2D globalLight;
    [SerializeField] private float minBrightness = 0f;
    [SerializeField] private float maxBrightness = 1f;

    // 지원 해상도 목록
    private readonly (int width, int height)[] resolutions =
    {
        (3840, 2160),
        (2560, 1440),
        (1920, 1080),
        (1600, 900),
        (1280, 720),
        (1024, 576)
    };

    // PlayerPrefs 키 상수
    private const string KEY_BGM        = "OptionBGM";
    private const string KEY_SFX        = "OptionSFX";
    private const string KEY_PULSE      = "OptionPulse";
    private const string KEY_BRIGHTNESS = "OptionBrightness";
    private const string KEY_RESOLUTION = "OptionResolution";

    void Start()
    {
        optionsPanel.SetActive(false);
        closeButton.onClick.AddListener(CloseOptions);
        SetupResolutionDropdown();
        LoadSettings();

        // LoadSettings 이후에 이벤트 연결 (중복 발생 방지)
        bgmSlider.onValueChanged.AddListener(OnBGMChanged);
        sfxSlider.onValueChanged.AddListener(OnSFXChanged);
        pulseSlider.onValueChanged.AddListener(OnPulseChanged);
        brightnessSlider.onValueChanged.AddListener(OnBrightnessChanged);
        resolutionDropdown.onValueChanged.AddListener(OnResolutionChanged);
    }

    public void OpenOptions()
    {
        optionsPanel.SetActive(true);
    }

    private void CloseOptions()
    {
        optionsPanel.SetActive(false);
    }

    private void SetupResolutionDropdown()
    {
        resolutionDropdown.ClearOptions();
        var options = new System.Collections.Generic.List<string>();
        options.Add("Fullscreen");
        foreach (var res in resolutions)
            options.Add($"{res.width} x {res.height}");
        resolutionDropdown.AddOptions(options);
    }

    private void LoadSettings()
    {
        // BGM 볼륨 (기본값 0.8)
        float bgm = PlayerPrefs.GetFloat(KEY_BGM, 0.5f);
        bgmSlider.value = bgm;
        ApplyMixerVolume("BGMVolume", bgm);

        // SFX 볼륨 (기본값 1.0)
        float sfx = PlayerPrefs.GetFloat(KEY_SFX, 0.5f);
        sfxSlider.value = sfx;
        ApplyMixerVolume("SFXVolume", sfx);

        // Pulse 볼륨 (기본값 0.6)
        float pulse = PlayerPrefs.GetFloat(KEY_PULSE, 0.6f);
        pulseSlider.value = pulse;
        ApplyMixerVolume("PulseVolume", pulse);

        // 밝기
        float brightness = PlayerPrefs.GetFloat(KEY_BRIGHTNESS, 0.1f);
        brightnessSlider.value = brightness;
        ApplyBrightness(brightness);

        // 해상도
        int resIndex = PlayerPrefs.GetInt(KEY_RESOLUTION, 4);
        resolutionDropdown.value = resIndex;
        ApplyResolution(resIndex);
    }

    private void OnBGMChanged(float value)
    {
        ApplyMixerVolume("BGMVolume", value);
        PlayerPrefs.SetFloat(KEY_BGM, value);
        PlayerPrefs.Save();
    }

    private void OnSFXChanged(float value)
    {
        ApplyMixerVolume("SFXVolume", value);
        PlayerPrefs.SetFloat(KEY_SFX, value);
        PlayerPrefs.Save();
    }

    private void OnPulseChanged(float value)
    {
        ApplyMixerVolume("PulseVolume", value);
        PlayerPrefs.SetFloat(KEY_PULSE, value);
        PlayerPrefs.Save();
    }

    private void OnBrightnessChanged(float value)
    {
        ApplyBrightness(value);
        PlayerPrefs.SetFloat(KEY_BRIGHTNESS, value);
        PlayerPrefs.Save();
    }

    private void OnResolutionChanged(int index)
    {
        ApplyResolution(index);
        PlayerPrefs.SetInt(KEY_RESOLUTION, index);
        PlayerPrefs.Save();
    }

    // AudioMixer 볼륨 적용
    // AudioMixer는 dB 단위라 슬라이더 0~1을 로그 스케일로 변환
    // 0 → -80dB (무음), 1 → 0dB (최대)
    private void ApplyMixerVolume(string paramName, float sliderValue)
    {
        if (mainMixer == null) return;

        // 슬라이더 0이면 완전 무음 (-80dB), 아니면 로그 변환
        float dB = sliderValue > 0.001f
            ? Mathf.Log10(sliderValue) * 20f
            : -80f;

        mainMixer.SetFloat(paramName, dB);
    }

    private void ApplyBrightness(float value)
    {
        if (globalLight == null) return;
        globalLight.intensity = Mathf.Lerp(minBrightness, maxBrightness, value);
    }

    private void ApplyResolution(int index)
    {
        if (index == 0)
        {
            Screen.SetResolution(Screen.currentResolution.width,
                                 Screen.currentResolution.height, true);
        }
        else
        {
            int resIndex = index - 1;
            if (resIndex >= resolutions.Length) return;
            var res = resolutions[resIndex];
            Screen.SetResolution(res.width, res.height, false);
        }
    }
}