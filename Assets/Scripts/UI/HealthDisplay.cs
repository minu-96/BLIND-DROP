using UnityEngine;
using UnityEngine.UI;

public class HealthDisplay : MonoBehaviour
{
    [Header("아이콘 참조")]
    [SerializeField] private Image[] baseHPIcons;   // HP_1, HP_2
    [SerializeField] private Image bonusHPIcon;     // HP_3

    // 기본 체력 색상
    private readonly Color activeColor  = new Color(1f, 1f, 1f, 1f);    // 불투명
    private readonly Color inactiveColor = new Color(1f, 1f, 1f, 0.15f); // 거의 투명
    private readonly Color bonusColor   = new Color(1f, 1f, 1f, 0.5f);  // 반투명

    public void UpdateHealth(int baseHP, int bonusHP)
    {
        // 기본 체력 아이콘 갱신
        for (int i = 0; i < baseHPIcons.Length; i++)
        {
            if (baseHPIcons[i] == null) continue;
            baseHPIcons[i].color = i < baseHP ? activeColor : inactiveColor;
        }

        // 보너스 체력 아이콘 갱신
        if (bonusHPIcon != null)
            bonusHPIcon.color = bonusHP > 0 ? bonusColor : inactiveColor;
    }
}