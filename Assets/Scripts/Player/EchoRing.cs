// Scripts/Player/EchoRing.cs
// 음파 링 시각 효과
// - 퍼지면서 알파가 서서히 줄어들고 최대 범위 도달 시 사라짐
// - 퍼펙트 에코 시 더 밝고 선명하게 표현

using UnityEngine;
using System.Collections;

public class EchoRing : MonoBehaviour
{
    private SpriteRenderer spriteRenderer;

    void Awake()
    {
        // SpriteRenderer 캐싱 (매 프레임 GetComponent 호출 방지)
        spriteRenderer = GetComponent<SpriteRenderer>();
    }

    // EchoEmitter에서 Instantiate 후 바로 호출
    public void Initialize(float maxRadius, float expandSpeed, bool isPerfect)
    {
        StartCoroutine(ExpandRing(maxRadius, expandSpeed, isPerfect));
    }

    private IEnumerator ExpandRing(float maxRadius, float expandSpeed, bool isPerfect)
    {
        float currentRadius = 0f;

        // 퍼펙트 에코면 시작 알파를 더 강하게
        float startAlpha = isPerfect ? 1f : 0.7f;

        while (currentRadius < maxRadius)
        {
            currentRadius += expandSpeed * Time.deltaTime;
            currentRadius = Mathf.Min(currentRadius, maxRadius); // 최대값 초과 방지

            // 링 크기 적용 (지름 = 반지름 * 2)
            float scale = currentRadius * 2f;
            transform.localScale = new Vector3(scale, scale, 1f);

            // 확장 진행도에 따라 알파 감소 (퍼질수록 흐려짐)
            float progress = currentRadius / maxRadius; // 0 → 1
            float alpha = Mathf.Lerp(startAlpha, 0f, progress);

            // SpriteRenderer에 알파 적용
            if (spriteRenderer != null)
            {
                Color c = spriteRenderer.color;
                c.a = alpha;
                spriteRenderer.color = c;
            }

            yield return null;
        }

        // 최대 범위 도달 → 즉시 삭제
        Destroy(gameObject);
    }
}