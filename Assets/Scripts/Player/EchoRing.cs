// Scripts/Player/EchoRing.cs
using UnityEngine;
using System.Collections;

public class EchoRing : MonoBehaviour
{
    private SpriteRenderer spriteRenderer;

    // 이미 감지한 적 목록 (같은 적을 여러 번 감지 방지)
    private System.Collections.Generic.HashSet<GameObject> hitEntities
        = new System.Collections.Generic.HashSet<GameObject>();

    void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
    }

    public void Initialize(float maxRadius, float expandSpeed, bool isPerfect)
    {
        StartCoroutine(ExpandRing(maxRadius, expandSpeed, isPerfect));
    }

    private IEnumerator ExpandRing(float maxRadius, float expandSpeed, bool isPerfect)
    {
        float currentRadius = 0f;
        float startAlpha = isPerfect ? 1f : 0.7f;

        while (currentRadius < maxRadius)
        {
            currentRadius += expandSpeed * Time.deltaTime;
            currentRadius = Mathf.Min(currentRadius, maxRadius);

            float scale = currentRadius * 2f;
            transform.localScale = new Vector3(scale, scale, 1f);

            float progress = currentRadius / maxRadius;
            float alpha = Mathf.Lerp(startAlpha, 0f, progress);

            if (spriteRenderer != null)
            {
                Color c = spriteRenderer.color;
                c.a = alpha;
                spriteRenderer.color = c;
            }

            // 매 프레임 현재 반지름 범위 안의 적 감지
            DetectEntities(currentRadius);

            yield return null;
        }

        Destroy(gameObject);
    }

    // 현재 링 반지름 범위 안의 EntityAI 감지 후 OnHitByEcho 호출
    private void DetectEntities(float radius)
    {
        // Physics2D.OverlapCircle로 현재 링 위치 기준 원형 범위 안 콜라이더 감지
        Collider2D[] hits = Physics2D.OverlapCircleAll(transform.position, radius);

        foreach (var hit in hits)
        {
            // 이미 감지한 오브젝트는 스킵
            if (hitEntities.Contains(hit.gameObject)) continue;

            EntityAI entity = hit.GetComponent<EntityAI>();
            if (entity == null) continue;

            // 감지 목록에 추가 (중복 호출 방지)
            hitEntities.Add(hit.gameObject);

            // EntityAI에 음파 피격 알림
            entity.OnHitByEcho(transform.position);
            Debug.Log($"[EchoRing] 적 감지: {hit.gameObject.name}");
        }
    }
}