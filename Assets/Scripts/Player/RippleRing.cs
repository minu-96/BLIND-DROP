// Scripts/Player/RippleRing.cs
// 파문 시각 효과
// - EchoRing과 동일한 구조지만 검정 색상
// - 퍼지면서 알파가 서서히 줄어들다 사라짐

using UnityEngine;
using System.Collections;

public class RippleRing : MonoBehaviour
{
    private SpriteRenderer spriteRenderer;

    void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();

        // 검정 색상으로 설정 (EchoRing은 흰색, RippleRing은 검정)
        if (spriteRenderer != null)
            spriteRenderer.color = new Color(1f, 1f, 1f, 1f); // 흰 배경 위 검정 효과
    }

    public void Initialize(float maxRadius, float expandSpeed)
    {
        StartCoroutine(ExpandRing(maxRadius, expandSpeed));
    }

    private IEnumerator ExpandRing(float maxRadius, float expandSpeed)
    {
        float currentRadius = 0f;

        while (currentRadius < maxRadius)
        {
            currentRadius += expandSpeed * Time.deltaTime;
            currentRadius = Mathf.Min(currentRadius, maxRadius);

            float scale = currentRadius * 2f;
            transform.localScale = new Vector3(scale, scale, 1f);

            // 퍼질수록 흐려짐
            float progress = currentRadius / maxRadius;
            float alpha = Mathf.Lerp(0.8f, 0f, progress);

            if (spriteRenderer != null)
            {
                Color c = spriteRenderer.color;
                c.a = alpha;
                spriteRenderer.color = c;
            }

            yield return null;
        }

        Destroy(gameObject);
    }
}