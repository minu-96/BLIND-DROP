// Scripts/UI/OptionsManager.cs
// 옵션 패널 전체 관리
// - 사운드 볼륨 슬라이더
// - 밝기 슬라이더 (Global Light2D Intensity 조정)
// - 해상도 드롭다운
// - X버튼으로 패널 닫기
// - PlayerPrefs로 설정값 저장/불러오기

using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Rendering.Universal;
using TMPro;

public class OptionsManager : MonoBehaviour
{
    [Header("패널 참조")]
    [SerializeField] private GameObject optionsPanel;   // 옵션 패널 루트 오브젝트

    [Header("슬라이더 참조")]
    [SerializeField] private Slider volumeSlider;       // 사운드 볼륨 슬라이더
    [SerializeField] private Slider brightnessSlider;   // 밝기 슬라이더

    [Header("드롭다운 참조")]
    [SerializeField] private TMP_Dropdown resolutionDropdown; // 해상도 드롭다운

    [Header("버튼 참조")]
    [SerializeField] private Button closeButton;        // X 버튼

    [Header("밝기 설정")]
    [SerializeField] private Light2D globalLight;       // Global Light2D 참조
    [SerializeField] private float minBrightness = 0f;  // 밝기 최솟값
    [SerializeField] private float maxBrightness = 1f;  // 밝기 최댓값

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
    private const string KEY_VOLUME     = "OptionVolume";
    private const string KEY_BRIGHTNESS = "OptionBrightness";
    private const string KEY_RESOLUTION = "OptionResolution";
    private const string KEY_FULLSCREEN = "OptionFullscreen";

    void Start()
    {
        // 시작 시 패널 비활성화
        optionsPanel.SetActive(false);

        // 버튼 이벤트 연결
        closeButton.onClick.AddListener(CloseOptions);

        // 해상도 드롭다운 옵션 구성
        SetupResolutionDropdown();

        // 저장된 설정값 불러와서 UI에 반영
        LoadSettings();

        // 슬라이더/드롭다운 변경 이벤트 연결
        // (LoadSettings 이후에 연결해야 불러올 때 이벤트가 중복 발생하지 않음)
        volumeSlider.onValueChanged.AddListener(OnVolumeChanged);
        brightnessSlider.onValueChanged.AddListener(OnBrightnessChanged);
        resolutionDropdown.onValueChanged.AddListener(OnResolutionChanged);
    }

    // 옵션 패널 열기 (TitleManager에서 호출)
    public void OpenOptions()
    {
        optionsPanel.SetActive(true);
    }

    // X버튼 클릭 시 패널 닫기
    private void CloseOptions()
    {
        optionsPanel.SetActive(false);
    }

    // 해상도 드롭다운 옵션 목록 구성
    private void SetupResolutionDropdown()
    {
        resolutionDropdown.ClearOptions();

        var options = new System.Collections.Generic.List<string>();

        // 풀스크린 옵션을 첫 번째로 추가
        options.Add("Fullscreen");

        // 해상도 목록 추가
        foreach (var res in resolutions)
            options.Add($"{res.width} x {res.height}");

        resolutionDropdown.AddOptions(options);
    }

    // 저장된 설정 불러오기 + UI 반영 + 실제 적용
    private void LoadSettings()
    {
        // 볼륨 (기본값 1.0)
        float volume = PlayerPrefs.GetFloat(KEY_VOLUME, 1f);
        volumeSlider.value = volume;
        ApplyVolume(volume);

        // 밝기 (기본값 0.5)
        float brightness = PlayerPrefs.GetFloat(KEY_BRIGHTNESS, 0.5f);
        brightnessSlider.value = brightness;
        ApplyBrightness(brightness);

        // 해상도 (기본값 0 = Fullscreen)
        int resIndex = PlayerPrefs.GetInt(KEY_RESOLUTION, 0);
        resolutionDropdown.value = resIndex;
        ApplyResolution(resIndex);
    }

    // 볼륨 슬라이더 변경 시
    private void OnVolumeChanged(float value)
    {
        ApplyVolume(value);
        PlayerPrefs.SetFloat(KEY_VOLUME, value);
        PlayerPrefs.Save();
    }

    // 밝기 슬라이더 변경 시
    private void OnBrightnessChanged(float value)
    {
        ApplyBrightness(value);
        PlayerPrefs.SetFloat(KEY_BRIGHTNESS, value);
        PlayerPrefs.Save();
    }

    // 해상도 드롭다운 변경 시
    private void OnResolutionChanged(int index)
    {
        ApplyResolution(index);
        PlayerPrefs.SetInt(KEY_RESOLUTION, index);
        PlayerPrefs.Save();
    }

    // 볼륨 실제 적용
    private void ApplyVolume(float value)
    {
        // AudioListener.volume으로 전체 볼륨 조정
        // 사운드 에셋 추가 시 AudioMixer로 교체 가능
        AudioListener.volume = value;
    }

    // 밝기 실제 적용 (Global Light2D Intensity 조정)
    private void ApplyBrightness(float value)
    {
        if (globalLight == null) return;
        // 슬라이더 0~1 값을 minBrightness~maxBrightness 범위로 변환
        globalLight.intensity = Mathf.Lerp(minBrightness, maxBrightness, value);
    }

    // 해상도 실제 적용
    private void ApplyResolution(int index)
    {
        if (index == 0)
        {
            // 풀스크린
            Screen.SetResolution(Screen.currentResolution.width,
                                 Screen.currentResolution.height, true);
        }
        else
        {
            // 인덱스 1부터 resolutions 배열 기준 (0번은 Fullscreen이므로 -1)
            int resIndex = index - 1;
            if (resIndex >= resolutions.Length) return;

            var res = resolutions[resIndex];
            Screen.SetResolution(res.width, res.height, false);
        }
    }
}