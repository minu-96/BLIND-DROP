using UnityEngine;
using UnityEngine.Rendering.Universal;
using System.Collections;
using System.Collections.Generic;

public class DarknessLayer : MonoBehaviour
{
    [Header("음파 조명 설정")]
    [SerializeField] private float baseLightRadius = 5f;      // 기본 음파 조명 반경
    [SerializeField] private float perfectLightRadius = 10f;  // 퍼펙트 에코 조명 반경
    [SerializeField] private float lightFadeTime = 0.8f;      // 조명이 사라지는 시간
    [SerializeField] private float maxLightIntensity = 1f;    // 최대 조명 밝기

    [Header("조명 프리팹")]
    [SerializeField] private GameObject echoLightPrefab;      // 음파 조명 프리팹

    // 현재 활성화된 조명 목록 (페이드아웃 관리용)
    private List<Light2D> activeLights = new List<Light2D>();

    // 음파 발사 시 EchoEmitter에서 이 메서드 호출
    public void OnEchoFired(Vector2 position, bool isPerfect)
    {
        float radius = isPerfect ? perfectLightRadius : baseLightRadius;
        StartCoroutine(SpawnEchoLight(position, radius, isPerfect));
    }

    private IEnumerator SpawnEchoLight(Vector2 position, float radius, bool isPerfect)
    {
        // 조명 오브젝트 생성
        GameObject lightObj = Instantiate(echoLightPrefab,
            new Vector3(position.x, position.y, 0), Quaternion.identity);

        Light2D light = lightObj.GetComponent<Light2D>();
        if (light == null)
        {
            Destroy(lightObj);
            yield break;
        }

        // 조명 초기 설정
        light.pointLightOuterRadius = radius;
        light.intensity = maxLightIntensity;

        // 퍼펙트 에코면 더 밝게
        if (isPerfect)
            light.intensity = maxLightIntensity * 1.5f;

        activeLights.Add(light);

        // lightFadeTime 동안 서서히 페이드아웃
        float elapsed = 0f;
        float startIntensity = light.intensity;

        while (elapsed < lightFadeTime)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / lightFadeTime;

            // 조명 밝기를 서서히 0으로
            light.intensity = Mathf.Lerp(startIntensity, 0f, t);
            yield return null;
        }

        // 조명 제거
        activeLights.Remove(light);
        Destroy(lightObj);
    }

    // 맥박 점멸 효과 (PulseController 이벤트에서 호출)
    public void OnPulseFlash(bool isBigPulse)
    {
        StartCoroutine(PulseFlash(isBigPulse));
    }

    private IEnumerator PulseFlash(bool isBigPulse)
    {
        // 화면 테두리 점멸은 UI에서 처리할 예정
        // 여기선 전체 조명 강도를 잠깐 올려주는 효과만
        float flashIntensity = isBigPulse ? 0.15f : 0.08f;
        float flashTime = 0.1f;

        // 전체 활성 조명 잠깐 밝게
        foreach (Light2D light in activeLights)
        {
            if (light != null)
                light.intensity += flashIntensity;
        }

        yield return new WaitForSeconds(flashTime);

        // 원래대로 복구
        foreach (Light2D light in activeLights)
        {
            if (light != null)
                light.intensity -= flashIntensity;
        }
    }
}