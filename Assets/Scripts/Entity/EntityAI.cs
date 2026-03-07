// Scripts/Entity/EntityAI.cs
using UnityEngine;
using System.Collections;

public class EntityAI : MonoBehaviour
{
    public enum EntityType
    {
        Wanderer,
        Chaser,
        Ambusher,
        Reflector
    }

    [Header("적 설정")]
    [SerializeField] public EntityType entityType;
    [SerializeField] private float tileSize = 1f;
    [SerializeField] private float moveTime = 0.1f;

    [Header("매복자 설정")]
    [SerializeField] private int ambushRange = 2;
    [SerializeField] private int ambushDashRange = 3;

    [Header("추격자 설정")]
    [SerializeField] private int chaseStepsOnRipple = 5; // 파문 1회당 추격 횟수

    private PulseController pulseController;
    private CaveGenerator caveGenerator;
    private Transform playerTransform;
    private PlayerController playerController;

    private bool isMoving = false;
    private Vector2Int reflectorDir = Vector2Int.zero;
    private bool isDashing = false;

    private bool isDamageCooldown = false;
    private float damageCooldownTime = 1.0f;

    // Chaser 전용: 남은 추격 횟수
    // 0이면 대기, 0 초과면 파문 반응 중
    private int remainingChaseSteps = 0;

    void Start()
    {
        pulseController    = FindFirstObjectByType<PulseController>();
        caveGenerator      = FindFirstObjectByType<CaveGenerator>();
        playerTransform    = GameObject.FindWithTag("Player").transform;
        playerController   = FindFirstObjectByType<PlayerController>();

        pulseController.onPulse.AddListener(OnPulse);

        // Chaser만 파문 이벤트 구독
        if (entityType == EntityType.Chaser && playerController != null)
            playerController.onRipple += OnRippleReceived;
    }

    void OnDestroy()
    {
        if (pulseController != null)
            pulseController.onPulse.RemoveListener(OnPulse);

        // Chaser 파문 이벤트 구독 해제
        if (entityType == EntityType.Chaser && playerController != null)
            playerController.onRipple -= OnRippleReceived;
    }

    // 파문 발생 시 호출 (PlayerController.onRipple 이벤트)
    private void OnRippleReceived()
    {
        if (entityType != EntityType.Chaser) return;

        // 파문 발생 시 추격 횟수 충전
        // 이미 추격 중이면 횟수 리셋 (새 파문이 더 강하게 반응)
        remainingChaseSteps = chaseStepsOnRipple;
        Debug.Log($"[Chaser] 파문 감지! {remainingChaseSteps}번 추격 시작");
    }

    private void OnPulse()
    {
        if (isMoving) return;

        switch (entityType)
        {
            case EntityType.Wanderer:  MoveWanderer();  break;
            case EntityType.Chaser:    MoveChaser();    break;
            case EntityType.Ambusher:  MoveAmbusher();  break;
            case EntityType.Reflector: MoveReflector(); break;
        }
    }

    private void MoveWanderer()
    {
        Vector2Int[] directions = {
            Vector2Int.up, Vector2Int.down,
            Vector2Int.left, Vector2Int.right
        };

        // 방향 셔플
        for (int i = 0; i < directions.Length; i++)
        {
            int rand = Random.Range(i, directions.Length);
            (directions[i], directions[rand]) = (directions[rand], directions[i]);
        }

        foreach (var dir in directions)
        {
            Vector2Int targetTile = GetCurrentTile() + dir;
            if (!caveGenerator.IsWall(targetTile.x, targetTile.y))
            {
                StartCoroutine(MoveToTile(targetTile));
                return;
            }
        }
    }

    private void MoveChaser()
    {
        // 남은 추격 횟수가 없으면 대기
        if (remainingChaseSteps <= 0)
        {
            Debug.Log("[Chaser] 대기 중 (파문 없음)");
            return;
        }

        // 추격 횟수 1 소모 후 플레이어 방향으로 이동
        remainingChaseSteps--;
        Debug.Log($"[Chaser] 추격 중 (남은 횟수: {remainingChaseSteps})");

        Vector2Int currentTile = GetCurrentTile();
        Vector2Int playerTile  = GetPlayerTile();
        Vector2Int dir         = GetDirectionToPlayer(currentTile, playerTile);
        Vector2Int targetTile  = currentTile + dir;

        if (!caveGenerator.IsWall(targetTile.x, targetTile.y))
            StartCoroutine(MoveToTile(targetTile));
    }

    private void MoveAmbusher()
    {
        Vector2Int currentTile = GetCurrentTile();
        Vector2Int playerTile  = GetPlayerTile();

        int distance = Mathf.Abs(currentTile.x - playerTile.x) +
                       Mathf.Abs(currentTile.y - playerTile.y);

        if (distance <= ambushRange && !isDashing)
            StartCoroutine(DashToPlayer(currentTile, playerTile));
    }

    private void MoveReflector()
    {
        if (reflectorDir == Vector2Int.zero) return;

        Vector2Int currentTile = GetCurrentTile();
        Vector2Int targetTile  = currentTile + reflectorDir;

        if (!caveGenerator.IsWall(targetTile.x, targetTile.y))
            StartCoroutine(MoveToTile(targetTile));
        else
            reflectorDir = Vector2Int.zero;
    }

    public void OnHitByEcho(Vector2 echoOrigin)
    {
        if (entityType != EntityType.Reflector) return;

        Vector2 toEntity = (Vector2)transform.position - echoOrigin;

        if (Mathf.Abs(toEntity.x) >= Mathf.Abs(toEntity.y))
            reflectorDir = toEntity.x > 0 ? Vector2Int.right : Vector2Int.left;
        else
            reflectorDir = toEntity.y > 0 ? Vector2Int.up : Vector2Int.down;

        Debug.Log($"[Reflector] 음파 피격 - 이동 방향: {reflectorDir}");
    }

    private IEnumerator DashToPlayer(Vector2Int from, Vector2Int to)
    {
        isDashing = true;
        isMoving  = true;

        Vector2Int currentTile = from;
        Vector2Int dir         = GetDirectionToPlayer(from, to);

        for (int i = 0; i < ambushDashRange; i++)
        {
            Vector2Int nextTile = currentTile + dir;
            if (caveGenerator.IsWall(nextTile.x, nextTile.y)) break;

            yield return StartCoroutine(MoveToTile(nextTile));
            currentTile = nextTile;

            if (currentTile == GetPlayerTile()) break;
        }

        isDashing = false;
        isMoving  = false;
    }

    private IEnumerator MoveToTile(Vector2Int targetTile)
    {
        isMoving = true;

        Vector3 startPos   = transform.position;
        Vector3 targetPos  = caveGenerator.TileToWorld(targetTile);
        float elapsed      = 0f;

        while (elapsed < moveTime)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / moveTime);
            transform.position = Vector3.Lerp(startPos, targetPos, t);
            yield return null;
        }

        transform.position = targetPos;
        isMoving = false;
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag("Player")) return;
        if (isDamageCooldown) return;

        HealthManager healthManager = FindFirstObjectByType<HealthManager>();
        if (healthManager != null)
            healthManager.TakeDamage(1);

        Debug.Log($"[Entity] 플레이어 충돌 - 데미지!");
        StartCoroutine(DamageCooldown());
    }

    private IEnumerator DamageCooldown()
    {
        isDamageCooldown = true;
        yield return new WaitForSeconds(damageCooldownTime);
        isDamageCooldown = false;
    }

    private Vector2Int GetCurrentTile()
    {
        return new Vector2Int(
            Mathf.RoundToInt(transform.position.x / tileSize),
            Mathf.RoundToInt(transform.position.y / tileSize)
        );
    }

    private Vector2Int GetPlayerTile()
    {
        return new Vector2Int(
            Mathf.RoundToInt(playerTransform.position.x / tileSize),
            Mathf.RoundToInt(playerTransform.position.y / tileSize)
        );
    }

    private Vector2Int GetDirectionToPlayer(Vector2Int from, Vector2Int to)
    {
        Vector2Int diff = to - from;

        if (Mathf.Abs(diff.x) >= Mathf.Abs(diff.y))
            return diff.x > 0 ? Vector2Int.right : Vector2Int.left;
        else
            return diff.y > 0 ? Vector2Int.up : Vector2Int.down;
    }
}