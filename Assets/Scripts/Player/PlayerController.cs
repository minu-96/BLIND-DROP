using UnityEngine;
using System.Collections;

public class PlayerController : MonoBehaviour
{
    [Header("이동 설정")]
    [SerializeField] private float tileSize = 1f;
    [SerializeField] private float moveTime = 0.1f;
    [SerializeField] private float pulseWindow = 0.2f;

    [Header("참조")]
    [SerializeField] private PulseController pulseController;
    [SerializeField] private CaveGenerator caveGenerator;

    private bool isMoving = false;
    private bool isPulseTime = false;
    private float pulseTimer = 0f;
    private bool isInputLocked = false;

    public System.Action onRipple;

    void Start()
    {
        // 일반 맥박 이벤트만 구독 (큰 박동 제거)
        pulseController.onPulse.AddListener(OnPulseReceived);
    }

    void Update()
    {
        if (!isMoving)
            HandleMovementInput();

        UpdatePulseWindow();
    }

    public void LockInput()   { isInputLocked = true; }
    public void UnlockInput() { isInputLocked = false; }

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

        if (dir == Vector2.zero) return;

        Vector2 targetPos = (Vector2)transform.position + dir * tileSize;
        int tileX = Mathf.RoundToInt(targetPos.x / tileSize);
        int tileY = Mathf.RoundToInt(targetPos.y / tileSize);

        if (caveGenerator.IsWall(tileX, tileY))
        {
            Debug.Log("[Player] 벽 - 이동 불가");
            return;
        }

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

    private IEnumerator MoveToTile(Vector2 targetPos)
    {
        isMoving = true;
        Vector2 startPos = transform.position;
        float elapsed = 0f;

        while (elapsed < moveTime)
        {
            elapsed += Time.deltaTime;
            transform.position = Vector2.Lerp(startPos, targetPos,
                                 Mathf.Clamp01(elapsed / moveTime));
            yield return null;
        }

        transform.position = targetPos;
        isMoving = false;
    }

    private void UpdatePulseWindow()
    {
        if (isPulseTime)
        {
            pulseTimer -= Time.deltaTime;
            if (pulseTimer <= 0f)
                isPulseTime = false;
        }
    }

    private void OnPulseReceived()
    {
        // 맥박 타이밍 윈도우 열기
        isPulseTime = true;
        pulseTimer = pulseWindow;
    }

    public bool IsPulseTime() => isPulseTime;
}