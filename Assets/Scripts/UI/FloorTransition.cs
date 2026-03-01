using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;

public class FloorTransition : MonoBehaviour
{
    [Header("UI 참조")]
    [SerializeField] private Image bgImage;
    [SerializeField] private TextMeshProUGUI floorText;

    [Header("연출 설정")]
    [SerializeField] private float fadeOutTime = 0.3f;  // 어두워지는 시간
    [SerializeField] private float holdTime    = 0.4f;  // 검정 화면 유지 시간
    [SerializeField] private float fadeInTime  = 0.3f;  // 밝아지는 시간

    // 전환 완료 시 GameManager에 알리는 콜백
    private System.Action onMidPoint;  // 검정 화면일 때 (동굴 생성 타이밍)
    private System.Action onComplete;  // 페이드 인 완료 후 (입력 허용 타이밍)

    // ── 공개 API ──────────────────────────────────────
    public void PlayTransition(int floor, System.Action onMidPoint, System.Action onComplete)
{
    // 연출 시작 시 입력 즉시 차단
    GameManager.Instance.GetComponent<PlayerController>();
    FindFirstObjectByType<PlayerController>().LockInput(); // 추가

    this.onMidPoint = onMidPoint;
    this.onComplete = onComplete;
    StartCoroutine(TransitionRoutine(floor));
}

    // ── 연출 코루틴 ───────────────────────────────────
    private IEnumerator TransitionRoutine(int floor)
    {
        // 1. 페이드 아웃 (화면이 어두워짐)
        yield return StartCoroutine(Fade(0f, 1f, fadeOutTime));

        // 2. 검정 화면에서 동굴 생성 (플레이어는 못 봄)
        floorText.text = $"DEPTH  {floor}";
        SetTextAlpha(1f);
        onMidPoint?.Invoke();

        // 3. 검정 화면 잠깐 유지
        yield return new WaitForSeconds(holdTime);

        // 4. 페이드 인 (화면이 밝아짐)
        SetTextAlpha(0f);
        yield return StartCoroutine(Fade(1f, 0f, fadeInTime));

        // 5. 완료 → 입력 허용
        onComplete?.Invoke();
    }

    private IEnumerator Fade(float from, float to, float duration)
    {
        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float alpha = Mathf.Lerp(from, to, elapsed / duration);
            SetBgAlpha(alpha);
            yield return null;
        }

        SetBgAlpha(to);
    }

    private void SetBgAlpha(float alpha)
    {
        Color c = bgImage.color;
        c.a = alpha;
        bgImage.color = c;
    }

    private void SetTextAlpha(float alpha)
    {
        Color c = floorText.color;
        c.a = alpha;
        floorText.color = c;
    }
}