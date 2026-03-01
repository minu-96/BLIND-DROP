using UnityEngine;
using UnityEngine.UI;

public class EchoCooldownDisplay : MonoBehaviour
{
    [Header("UI 참조")]
    [SerializeField] private Image fillImage;

    [Header("색상")]
    [SerializeField] private Color readyColor    = new Color(1f, 1f, 1f, 1f);
    [SerializeField] private Color cooldownColor = new Color(1f, 1f, 1f, 0.3f);

    // HUDManager가 매 프레임 호출
    public void UpdateCooldown(float progress)
    {
        if (fillImage == null) return;

        fillImage.fillAmount = Mathf.Clamp01(progress);
        fillImage.color = Mathf.Approximately(progress, 1f) ? readyColor : cooldownColor;
    }
}