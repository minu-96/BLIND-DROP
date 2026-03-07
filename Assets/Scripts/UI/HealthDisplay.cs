using UnityEngine;
using UnityEngine.UI;

public class HealthDisplay : MonoBehaviour
{
    [Header("아이콘 참조")]
    [SerializeField] private Image[] baseHPIcons;  // HP_1, HP_2
    [SerializeField] private Image bonusHPIcon;    // HP_3

    // 기본 체력 색상
    private readonly Color activeColor   = new Color(1f, 1f, 1f, 1f);    // 불투명 (체력 있음)
    private readonly Color inactiveColor = new Color(1f, 1f, 1f, 0.08f); // 거의 투명 (체력 없음)

    // 보너스 체력 아이콘 색상
    private readonly Color bonusFullColor = new Color(1f, 1f, 1f, 1f);   // 불투명 (파편 2개, HP 있음)
    private readonly Color bonusHalfColor = new Color(1f, 1f, 1f, 0.35f);// 반투명 (파편 1개)
    private readonly Color bonusNoneColor = new Color(1f, 1f, 1f, 0f);   // 투명 (파편 0개)

    // 기본 체력 + 보너스 체력 갱신
    public void UpdateHealth(int baseHP, int bonusHP)
    {
        // 기본 체력 아이콘 갱신
        for (int i = 0; i < baseHPIcons.Length; i++)
        {
            if (baseHPIcons[i] == null) continue;
            baseHPIcons[i].color = i < baseHP ? activeColor : inactiveColor;
        }

        // 보너스 체력 아이콘 갱신
        // bonusHP > 0이면 불투명, 아니면 투명
        // (파편 진행도는 UpdateBonusProgress에서 별도 처리)
        if (bonusHPIcon == null) return;

        if (bonusHP > 0)
            bonusHPIcon.color = bonusFullColor;
        else
            bonusHPIcon.color = bonusNoneColor;
    }

    // 파편 수집 진행도에 따라 보너스HP 아이콘 상태 갱신
    // fragmentsInCycle: 현재 2개 주기 내 수집한 파편 수 (0 또는 1)
    // hasBonusHP: 현재 보너스HP 보유 여부
    public void UpdateBonusProgress(int fragmentsInCycle, bool hasBonusHP)
    {
        if (bonusHPIcon == null) return;

        if (hasBonusHP)
        {
            // 보너스HP가 있으면 항상 불투명
            bonusHPIcon.color = bonusFullColor;
        }
        else if (fragmentsInCycle == 1)
        {
            // 보너스HP 없고 파편 1개 수집 중 → 반투명 (진행 중 표시)
            bonusHPIcon.color = bonusHalfColor;
        }
        else
        {
            // 보너스HP 없고 파편 0개 → 투명
            bonusHPIcon.color = bonusNoneColor;
        }
    }
}