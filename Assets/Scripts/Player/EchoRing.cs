using UnityEngine;
using System.Collections;

public class EchoRing : MonoBehaviour
{
    // EchoEmitter에서 Instantiate 후 바로 이 메서드 호출해서 초기화
    public void Initialize(float maxRadius, float expandSpeed, bool isPerfect)
    {
        StartCoroutine(ExpandRing(maxRadius, expandSpeed, isPerfect));
    }

    private IEnumerator ExpandRing(float maxRadius, float expandSpeed, bool isPerfect)
    {
        float currentRadius = 0f;

        // 링이 최대 범위까지 퍼지는 동안 반복
        while (currentRadius < maxRadius)
        {
            currentRadius += expandSpeed * Time.deltaTime;

            // 링 크기 적용 (원형 스케일로 표현)
            float scale = currentRadius * 2f; // 지름 = 반지름 * 2
            transform.localScale = new Vector3(scale, scale, 1f);

            // 퍼펙트 에코면 알파값을 더 강하게 표현 (추후 SpriteRenderer와 연결)
            float alpha = isPerfect ? 1f : 0.7f;
            // TODO: SpriteRenderer 알파 적용 (링 프리팹 완성 후 연결)

            yield return null;
        }

        // 최대 범위 도달 후 오브젝트 삭제
        Destroy(gameObject);
    }
}