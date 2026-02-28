using UnityEngine;

public class GameManager : MonoBehaviour
{
    // 싱글톤: 어디서든 GameManager.Instance로 접근 가능
    public static GameManager Instance { get; private set; }

    [Header("참조")]
    [SerializeField] private CaveGenerator caveGenerator;       // 동굴 생성기
    [SerializeField] private PulseController pulseController;   // 맥박 컨트롤러
    [SerializeField] private EchoEmitter echoEmitter;           // 음파 발사기
    [SerializeField] private PlayerController playerController; // 플레이어
    [SerializeField] private HealthManager healthManager;       // 체력 관리
    [SerializeField] private Transform playerTransform;         // 플레이어 위치 설정용

    [Header("게임 상태")]
    public int currentFloor { get; private set; } = 1;   // 현재 층수
    public int bestFloor { get; private set; } = 0;      // 개인 최고 기록
    public bool isGameOver { get; private set; } = false;

    // 현재 플레이의 랜덤 시드 (매 플레이마다 다른 동굴)
    private int currentSeed;

    // 층수에 따른 BPM 설정 (GDD 기준)
    // Key: 최소 층수, Value: BPM
    private readonly (int minFloor, float bpm)[] bpmTable =
    {
        (1,  40f),
        (11, 60f),
        (31, 80f),
        (61, 100f)
    };

    // 층수에 따른 음파 쿨타임 설정 (GDD 기준)
    private readonly (int minFloor, float cooldown)[] cooldownTable =
    {
        (1,  1.5f),
        (11, 2.0f),
        (26, 2.5f),
        (51, 3.0f)
    };

    void Awake()
    {
        // 싱글톤 초기화: 이미 Instance가 있으면 중복 제거
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    void Start()
    {
        // 저장된 최고 기록 불러오기
        bestFloor = PlayerPrefs.GetInt("BestFloor", 0);

        // 새 게임 시작
        StartNewGame();
    }

    public void StartNewGame()
    {
        // 게임 상태 초기화
        currentFloor = 1;
        isGameOver = false;

        // 매 플레이마다 다른 시드 생성 (랜덤 동굴)
        currentSeed = Random.Range(0, int.MaxValue);

        // 체력 초기화
        healthManager.ResetHealth();

        // 1층 생성 및 시작
        LoadFloor(currentFloor);
    }

    public void LoadFloor(int floor)
    {
        // 1. 동굴 생성
        caveGenerator.GenerateFloor(floor, currentSeed);

        // 2. 플레이어를 입구 위치에 배치
        playerTransform.position = caveGenerator.TileToWorld(caveGenerator.entrancePos);

        // 3. 층수에 맞는 BPM 적용
        float newBPM = GetValueForFloor(bpmTable, floor);
        pulseController.SetBPM(newBPM);
        Debug.Log($"[GameManager] {floor}층 시작 - BPM: {newBPM}");

        // 4. 층수에 맞는 음파 쿨타임 적용
        float newCooldown = GetValueForFloor(cooldownTable, floor);
        echoEmitter.SetCooldown(newCooldown);
        Debug.Log($"[GameManager] 음파 쿨타임: {newCooldown}초");

        // 5. 5층마다 기본 체력 초기화
        if (floor % 5 == 0)
        {
            healthManager.ResetBaseHealth();
            Debug.Log($"[GameManager] {floor}층 - 기본 체력 초기화");
        }
    }

    // 계단에 닿았을 때 호출 (StairTile 또는 PlayerController에서 호출)
    public void OnPlayerReachedExit()
    {
        currentFloor++;
        Debug.Log($"[GameManager] {currentFloor}층으로 이동");
        LoadFloor(currentFloor);
    }

    // 플레이어 사망 시 호출 (HealthManager에서 호출)
    public void OnGameOver()
    {
        isGameOver = true;

        // 최고 기록 갱신
        if (currentFloor > bestFloor)
        {
            bestFloor = currentFloor;
            PlayerPrefs.SetInt("BestFloor", bestFloor);
            PlayerPrefs.Save();
            Debug.Log($"[GameManager] 최고 기록 갱신! {bestFloor}층");
        }

        Debug.Log($"[GameManager] 게임 오버 - 도달 층수: {currentFloor}층");

        // TODO: SceneResult로 전환 (UI 완성 후 연결)
    }

    // 테이블에서 현재 층수에 맞는 값 찾기
    // 층수가 높을수록 더 높은 구간의 값 적용
    private float GetValueForFloor(
        (int minFloor, float value)[] table, int floor)
    {
        float result = table[0].value; // 기본값은 첫 번째 항목

        foreach (var entry in table)
        {
            if (floor >= entry.minFloor)
                result = entry.value;
            else
                break;
        }

        return result;
    }
}