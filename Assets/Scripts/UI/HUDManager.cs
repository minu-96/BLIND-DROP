using UnityEngine;

public class HUDManager : MonoBehaviour
{
    [Header("HUD 컴포넌트")]
    [SerializeField] private DepthDisplay depthDisplay;
    [SerializeField] private HealthDisplay healthDisplay;
    [SerializeField] private EchoCooldownDisplay echoCooldownDisplay;

    private GameManager gameManager;
    private HealthManager healthManager;
    private EchoEmitter echoEmitter;

    void Start()
    {
        gameManager   = GameManager.Instance;
        healthManager = FindFirstObjectByType<HealthManager>();
        echoEmitter   = FindFirstObjectByType<EchoEmitter>();

        // 체력 변경 이벤트 구독
        // onHealthChanged는 Action (인자 없음) 이므로 맞춰서 연결
        if (healthManager != null)
            healthManager.onHealthChanged += OnHealthChanged;

        // 시작 시 현재 상태로 초기화
        RefreshAll();
    }

    void OnDestroy()
    {
        if (healthManager != null)
            healthManager.onHealthChanged -= OnHealthChanged;
    }

    void Update()
    {
        // 쿨타임 게이지는 매 프레임 갱신
        if (echoEmitter != null)
            echoCooldownDisplay?.UpdateCooldown(echoEmitter.CooldownProgress);
    }

    // 층수 변경 시 GameManager에서 호출
    public void OnFloorChanged(int currentFloor, int bestFloor)
    {
        depthDisplay?.UpdateDepth(currentFloor, bestFloor);
    }

    // 체력 변경 시 HealthManager 이벤트로 자동 호출 (인자 없음)
    private void OnHealthChanged()
    {
        if (healthManager == null) return;
        healthDisplay?.UpdateHealth(healthManager.baseHealth, healthManager.bonusHealth);
    }

    private void RefreshAll()
    {
        if (gameManager != null)
            depthDisplay?.UpdateDepth(gameManager.currentFloor, gameManager.bestFloor);

        if (healthManager != null)
            healthDisplay?.UpdateHealth(healthManager.baseHealth, healthManager.bonusHealth);
    }
}