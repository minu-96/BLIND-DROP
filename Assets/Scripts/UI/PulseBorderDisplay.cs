using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class PulseBorderDisplay : MonoBehaviour
{
    [Header("테두리 이미지 (상/하/좌/우 순서)")]
    [SerializeField] private Image[] borderImages;

    [Header("일반 맥박")]
    [SerializeField] private float normalDuration = 0.1f;
    [SerializeField] private Color normalColor = new Color(1f, 1f, 1f, 0.7f);

    [Header("큰 박동 (4번째)")]
    [SerializeField] private float bigDuration = 0.18f;
    [SerializeField] private Color bigColor = new Color(1f, 1f, 1f, 1f);
    [SerializeField] private float bigThickness = 12f;
    [SerializeField] private float normalThickness = 6f;

    private Coroutine flashCoroutine;

    // PulseController에서 호출
    public void FlashNormal() =>
        TriggerFlash(normalColor, normalThickness, normalDuration);

    public void FlashBig() =>
        TriggerFlash(bigColor, bigThickness, bigDuration);

    private void TriggerFlash(Color color, float thickness, float duration)
    {
        if (flashCoroutine != null) StopCoroutine(flashCoroutine);
        flashCoroutine = StartCoroutine(FlashRoutine(color, thickness, duration));
    }

    private IEnumerator FlashRoutine(Color color, float thickness, float duration)
    {
        SetBorder(color, thickness);
        yield return new WaitForSeconds(duration);
        SetBorder(Color.clear, normalThickness);
        flashCoroutine = null;
    }

    private void SetBorder(Color color, float thickness)
    {
        if (borderImages == null) return;

        foreach (var img in borderImages)
        {
            if (img == null) continue;

            img.color = color;

            // 두께 조정: 상/하는 Height, 좌/우는 Width
            RectTransform rt = img.rectTransform;
            Vector2 size = rt.sizeDelta;

            // Pivot 기준으로 상/하(Pivot.y == 1 or 0)면 Height, 좌/우면 Width 조정
            if (Mathf.Approximately(rt.pivot.y, 1f) || Mathf.Approximately(rt.pivot.y, 0f))
                rt.sizeDelta = new Vector2(size.x, thickness); // 상/하
            else
                rt.sizeDelta = new Vector2(thickness, size.y); // 좌/우
        }
    }
}