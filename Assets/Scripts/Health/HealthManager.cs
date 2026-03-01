using UnityEngine;

public class HealthManager : MonoBehaviour
{
    [Header("체력 설정")]
    [SerializeField] private int maxBaseHealth = 2;   // 기본 체력 최대값 (항상 2)
    [SerializeField] private int maxBonusHealth = 1;  // 보너스 체력 최대값 (항상 1)

    // 현재 기본 체력 (5층마다 2로 초기화)
    public int baseHealth { get; private set; }

    // 현재 보너스 체력 (파편 2개 수집 시 +1, 최대 1)
    public int bonusHealth { get; private set; }

    // 총 체력 = 기본 + 보너스 (최대 3)
    public int totalHealth => baseHealth + bonusHealth;

    // 체력 변화 시 UI에 알리는 이벤트
    public System.Action onHealthChanged;

    // 사망 시 GameManager에 알리는 이벤트
    public System.Action onDeath;

    void Start()
    {
        // 시작 시 체력 초기화
        ResetHealth();
    }

    public void ResetHealth()
    {
        // 게임 시작 시 전체 초기화
        // 기본 체력 2, 보너스 체력 0으로 시작
        baseHealth = maxBaseHealth;
        bonusHealth = 0;

        onHealthChanged?.Invoke();
        Debug.Log($"[Health] 전체 초기화 - 체력: {totalHealth}");
    }

    public void ResetBaseHealth()
    {
        // 5층마다 기본 체력만 2로 초기화
        // 보너스 체력은 유지됨
        baseHealth = maxBaseHealth;

        onHealthChanged?.Invoke();
        Debug.Log($"[Health] 기본 체력 초기화 - 체력: {totalHealth}");
    }

    public void TakeDamage(int amount = 1)
    {
        // 피해는 기본 체력부터 먼저 소모
        // 기본 체력이 0이 되면 보너스 체력 소모
        for (int i = 0; i < amount; i++)
        {
            if (baseHealth > 0)
            {
                baseHealth--;
                Debug.Log($"[Health] 피해! 기본 체력: {baseHealth}, 보너스: {bonusHealth}");
            }
            else if (bonusHealth > 0)
            {
                bonusHealth--;
                Debug.Log($"[Health] 피해! 보너스 체력: {bonusHealth}");
            }

            if (totalHealth <= 0)
            {
                // HUD 먼저 갱신 (체력 0 표시) 후 게임오버  // 변경
                onHealthChanged?.Invoke();                  // 추가
                Debug.Log("[Health] 사망!");
                onDeath?.Invoke();
                GameManager.Instance.OnGameOver();
                return;
            }
        }

        onHealthChanged?.Invoke();
    }

    public void AddBonusHealth()
    {
        // 보너스 체력 추가 (최대 1개 초과 불가)
        if (bonusHealth < maxBonusHealth)
        {
            bonusHealth++;
            Debug.Log($"[Health] 보너스 체력 추가! 총 체력: {totalHealth}");
            onHealthChanged?.Invoke();
        }
        else
        {
            Debug.Log("[Health] 보너스 체력 이미 최대");
        }
    }

    // 현재 체력 상태 확인용 (UI에서 사용)
    public bool HasBonusHealth() => bonusHealth > 0;
}
