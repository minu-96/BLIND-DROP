using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    [Header("참조")]
    [SerializeField] private CaveGenerator caveGenerator;
    [SerializeField] private PulseController pulseController;
    [SerializeField] private EchoEmitter echoEmitter;
    [SerializeField] private PlayerController playerController;
    [SerializeField] private HealthManager healthManager;
    [SerializeField] private Transform playerTransform;
    [SerializeField] private HUDManager hudManager;
    [SerializeField] private FloorTransition floorTransition;  // 추가

    [Header("게임 상태")]
    public int currentFloor { get; private set; } = 1;
    public int bestFloor { get; private set; } = 0;
    public bool isGameOver { get; private set; } = false;

    private int currentSeed;

    private readonly (int minFloor, float bpm)[] bpmTable =
    {
        (1,  40f),
        (11, 60f),
        (31, 80f),
        (61, 100f)
    };

    private readonly (int minFloor, float cooldown)[] cooldownTable =
    {
        (1,  1.5f),
        (11, 2.0f),
        (26, 2.5f),
        (51, 3.0f)
    };

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    void Start()
    {
        bestFloor = PlayerPrefs.GetInt("BestFloor", 0);
        StartNewGame();
    }

    public void StartNewGame()
    {
        currentFloor = 1;
        isGameOver = false;
        currentSeed = Random.Range(0, int.MaxValue);
        healthManager.ResetHealth();

        // 1층은 페이드 없이 바로 로드
        LoadFloorImmediate(currentFloor);
    }

    public void OnPlayerReachedExit()
    {
        currentFloor++;
        Debug.Log($"[GameManager] {currentFloor}층으로 이동");

        // 페이드 연출과 함께 층 이동                           // 추가
        floorTransition.PlayTransition(                        // 추가
            currentFloor,                                      // 추가
            onMidPoint: () => LoadFloorImmediate(currentFloor),// 추가 (검정 화면에서 생성)
            onComplete: () => playerController.UnlockInput()   // 추가 (페이드 인 후 입력 허용)
        );                                                     // 추가
    }

    // 페이드 연출 없이 즉시 로드 (1층 시작 / 페이드 중 호출용)
    private void LoadFloorImmediate(int floor)
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

        // 6. HUD 층수 갱신
        hudManager?.OnFloorChanged(currentFloor, bestFloor);
    }

    public void OnGameOver()
    {
        isGameOver = true;

        if (currentFloor > bestFloor)
        {
            bestFloor = currentFloor;
            PlayerPrefs.SetInt("BestFloor", bestFloor);
            PlayerPrefs.Save();
            Debug.Log($"[GameManager] 최고 기록 갱신! {bestFloor}층");
            hudManager?.OnFloorChanged(currentFloor, bestFloor);
        }

        PlayerPrefs.SetInt("LastDepth", currentFloor);
        PlayerPrefs.Save();

        Debug.Log($"[GameManager] 게임 오버 - 도달 층수: {currentFloor}층");

        SceneManager.LoadScene("SceneResult");
    }

    private float GetValueForFloor(
        (int minFloor, float value)[] table, int floor)
    {
        float result = table[0].value;

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