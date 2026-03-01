// Scripts/UI/TitleAnimator.cs
// 타이틀 화면 테두리 맥박 연출
// - 빠르게 켜지고 천천히 꺼지는 심장박동 느낌

using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class TitleAnimator : MonoBehaviour
{
    [Header("테두리 Image 참조 (4개)")]
    [SerializeField] private Image borderTop;
    [SerializeField] private Image borderBottom;
    [SerializeField] private Image borderLeft;
    [SerializeField] private Image borderRight;

    [Header("박동 설정")]
    [SerializeField] private float pulseInterval = 1.5f;    // 박동 간격 (초)
    [SerializeField] private float fadeInDuration = 0.05f;  // 켜지는 시간 (매우 빠르게)
    [SerializeField] private float fadeOutDuration = 0.6f;  // 꺼지는 시간 (천천히)

    private Image[] borders;

    void Start()
    {
        borders = new Image[] { borderTop, borderBottom, borderLeft, borderRight };
        SetBorderAlpha(0f);
        StartCoroutine(PulseBorderRoutine());
    }

    private IEnumerator PulseBorderRoutine()
    {
        while (true)
        {
            // 다음 박동까지 대기
            yield return new WaitForSeconds(pulseInterval);

            // 빠르게 켜짐
            yield return StartCoroutine(FadeBorder(0f, 1f, fadeInDuration));

            // 천천히 꺼짐 (심장박동 잔향 느낌)
            yield return StartCoroutine(FadeBorder(1f, 0f, fadeOutDuration));
        }
    }

    // 테두리 Alpha를 from → to로 부드럽게 전환하는 코루틴
    private IEnumerator FadeBorder(float from, float to, float duration)
    {
        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / duration);

            // 꺼질 때는 EaseOut 커브 적용 → 처음엔 빠르게, 끝엔 느리게 사라짐
            float alpha = (to < from)
                ? Mathf.Lerp(from, to, t * t * (3f - 2f * t)) // SmoothStep
                : Mathf.Lerp(from, to, t);                     // 켜질 땐 선형

            SetBorderAlpha(alpha);
            yield return null;
        }

        SetBorderAlpha(to);
    }

    private void SetBorderAlpha(float alpha)
    {
        foreach (var border in borders)
        {
            if (border == null) continue;
            Color c = border.color;
            c.a = alpha;
            border.color = c;
        }
    }
}