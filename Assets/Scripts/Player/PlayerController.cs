using UnityEngine;
using System.Collections;

public class PlayerController : MonoBehaviour
{


    [Header("이동 설정")]
    [SerializeField] private float tileSize = 1f;      // 한 칸의 크기 (타일 크기와 맞춰야 함)
    [SerializeField] private float moveTime = 0.1f;    // 한 칸 이동에 걸리는 시간
    [SerializeField] private float pulseWindow = 0.2f; // 맥박 타이밍 허용 범위 ±0.2초

    [Header("참조")]
    [SerializeField] private PulseController pulseController;
    [SerializeField] private CaveGenerator caveGenerator; // 벽 체크용 추가

    // 현재 이동 중인지 여부 (이동 중 추가 입력 방지용)
    private bool isMoving = false;

    // 맥박 타이밍 윈도우가 열려있는지 여부
    private bool isPulseTime = false;
    private float pulseTimer = 0f;

    // 파문 발생 시 외부(EchoEmitter, EntityAI 등)에 알리는 이벤트
    public System.Action onRipple;

    private bool isInputLocked = false;

    void Start()
    {
        // PulseController의 맥박 이벤트를 구독
        // 맥박이 발생할 때마다 아래 메서드들이 자동으로 호출됨
        pulseController.onPulse.AddListener(OnPulseReceived);
        pulseController.onBigPulse.AddListener(OnBigPulseReceived);
    }

    void Update()
    {
        // 이동 중이 아닐 때만 입력 받음 (한 칸 이동 완료 후 다음 입력)
        if (!isMoving)
        {
            HandleMovementInput();
        }

        // 맥박 윈도우 타이머 매 프레임 업데이트
        UpdatePulseWindow();
    }

    public void UnlockInput()
{
    isInputLocked = false;
}

public void LockInput()
{
    isInputLocked = true;
}

    private void HandleMovementInput()
{
    if (isInputLocked) return;

    Vector2 dir = Vector2.zero;

    if (Input.GetKeyDown(KeyCode.W) || Input.GetKeyDown(KeyCode.UpArrow))
        dir = Vector2.up;
    else if (Input.GetKeyDown(KeyCode.S) || Input.GetKeyDown(KeyCode.DownArrow))
        dir = Vector2.down;
    else if (Input.GetKeyDown(KeyCode.A) || Input.GetKeyDown(KeyCode.LeftArrow))
        dir = Vector2.left;
    else if (Input.GetKeyDown(KeyCode.D) || Input.GetKeyDown(KeyCode.RightArrow))
        dir = Vector2.right;

    if (dir != Vector2.zero)
    {
        // 이동할 목표 타일 좌표 계산
        Vector2 targetPos = (Vector2)transform.position + dir * tileSize;

        // 타일 좌표로 변환해서 벽인지 확인
        int tileX = Mathf.RoundToInt(targetPos.x / tileSize);
        int tileY = Mathf.RoundToInt(targetPos.y / tileSize);

        // 벽이면 이동 자체를 막음
        if (caveGenerator.IsWall(tileX, tileY))
        {
            Debug.Log("[Player] 벽 - 이동 불가");
            return;
        }

        // 벽이 아니면 이동 + 파문 판정
        StartCoroutine(MoveToTile(targetPos));

        if (!isPulseTime)
        {
            onRipple?.Invoke();
            Debug.Log("[Player] 파문 발생 - 타이밍 미스");
        }
        else
        {
            Debug.Log("[Player] 조용한 이동 - 타이밍 성공");
        }
    }
}

    private IEnumerator MoveToTile(Vector2 targetPos)
    {
        // 이동 시작 - 이 플래그가 true인 동안 추가 입력 차단
        isMoving = true;

        Vector2 startPos = transform.position;
        float elapsed = 0f;

        // moveTime 동안 부드럽게 목표 위치로 이동
        while (elapsed < moveTime)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / moveTime); // 0~1 사이 진행도
            transform.position = Vector2.Lerp(startPos, targetPos, t);
            yield return null;
        }

        // 정확한 위치로 고정 (부동소수점 오차 방지)
        transform.position = targetPos;

        // 이동 완료 - 다음 입력 받을 수 있음
        isMoving = false;
    }

    private void UpdatePulseWindow()
    {
        // isPulseTime이 true일 때 타이머를 감소시키다가
        // 시간이 다 되면 윈도우를 닫음
        if (isPulseTime)
        {
            pulseTimer -= Time.deltaTime;
            if (pulseTimer <= 0f)
            {
                isPulseTime = false;
            }
        }
    }

    private void OnPulseReceived()
    {
        // 일반 맥박: 타이밍 윈도우만 열어줌
        OpenPulseWindow();
    }

    private void OnBigPulseReceived()
    {
        // 큰 박동(4번째): 윈도우 열기 + 이동 여부 무관하게 강제 파문 발생
        OpenPulseWindow();
        onRipple?.Invoke();
        Debug.Log("[Player] 큰 박동 - 강제 파문!");
    }

    private void OpenPulseWindow()
    {
        // 맥박 타이밍 윈도우를 pulseWindow 시간(0.2초)만큼 열어줌
        isPulseTime = true;
        pulseTimer = pulseWindow;
    }

    // EchoEmitter에서 퍼펙트 에코 판정할 때 이 메서드로 확인
    public bool IsPulseTime() => isPulseTime;
}