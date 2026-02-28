using UnityEngine;
using System.Collections;

public class EntityAI : MonoBehaviour
{
    // 적 유형 열거형
    public enum EntityType
    {
        Wanderer,   // 배회자: 랜덤 이동
        Chaser,     // 추적자: 플레이어 추적
        Ambusher,   // 매복자: 인접 시 돌진
        Reflector   // 반사체: 음파 반응
    }

    [Header("적 설정")]
[SerializeField] public EntityType entityType;
[SerializeField] private float tileSize = 1f;
[SerializeField] private float moveTime = 0.1f;

[Header("매복자 설정")]
[SerializeField] private int ambushRange = 2;
[SerializeField] private int ambushDashRange = 3;

// Inspector 연결 없이 Start()에서 자동으로 찾아옴
private PulseController pulseController;
private CaveGenerator caveGenerator;
private Transform playerTransform;

private bool isMoving = false;
private Vector2Int reflectorDir = Vector2Int.zero;
private bool isDashing = false;

void Start()
{
    // 씬에서 자동으로 필요한 컴포넌트 찾기
    // 프리팹이라 Inspector 연결이 풀려도 런타임에 자동 연결됨
    pulseController = FindFirstObjectByType<PulseController>();
    caveGenerator = FindFirstObjectByType<CaveGenerator>();
    playerTransform = GameObject.FindWithTag("Player").transform;

    // 맥박 이벤트 구독
    pulseController.onPulse.AddListener(OnPulse);
    pulseController.onBigPulse.AddListener(OnPulse);
}
    void OnDestroy()
    {
        // 오브젝트 삭제 시 이벤트 구독 해제 (메모리 누수 방지)
        if (pulseController != null)
        {
            pulseController.onPulse.RemoveListener(OnPulse);
            pulseController.onBigPulse.RemoveListener(OnPulse);
        }
    }

    private void OnPulse()
    {
        // 이미 이동 중이면 스킵
        if (isMoving) return;

        // 유형별 이동 처리
        switch (entityType)
        {
            case EntityType.Wanderer:
                MoveWanderer();
                break;
            case EntityType.Chaser:
                MoveChaser();
                break;
            case EntityType.Ambusher:
                MoveAmbusher();
                break;
            case EntityType.Reflector:
                MoveReflector();
                break;
        }
    }

    private void MoveWanderer()
    {
        // 4방향 중 랜덤으로 이동
        // 벽이면 다른 방향 시도 (최대 4번)
        Vector2Int[] directions = {
            Vector2Int.up, Vector2Int.down,
            Vector2Int.left, Vector2Int.right
        };

        // 방향 배열 섞기
        for (int i = 0; i < directions.Length; i++)
        {
            int rand = Random.Range(i, directions.Length);
            (directions[i], directions[rand]) = (directions[rand], directions[i]);
        }

        // 이동 가능한 첫 번째 방향으로 이동
        foreach (var dir in directions)
        {
            Vector2Int targetTile = GetCurrentTile() + dir;
            if (!caveGenerator.IsWall(targetTile.x, targetTile.y))
            {
                StartCoroutine(MoveToTile(targetTile));
                return;
            }
        }
        // 모든 방향이 막혀있으면 제자리
    }

    private void MoveChaser()
    {
        // 플레이어 방향으로 한 칸 이동
        Vector2Int currentTile = GetCurrentTile();
        Vector2Int playerTile = GetPlayerTile();
        Vector2Int dir = GetDirectionToPlayer(currentTile, playerTile);

        Vector2Int targetTile = currentTile + dir;

        // 목표 방향이 벽이면 이동 안 함
        if (!caveGenerator.IsWall(targetTile.x, targetTile.y))
            StartCoroutine(MoveToTile(targetTile));
    }

    private void MoveAmbusher()
    {
        // 플레이어가 감지 범위 내에 있으면 돌진
        // 아니면 제자리 대기
        Vector2Int currentTile = GetCurrentTile();
        Vector2Int playerTile = GetPlayerTile();

        int distance = Mathf.Abs(currentTile.x - playerTile.x) +
                       Mathf.Abs(currentTile.y - playerTile.y); // 맨해튼 거리

        if (distance <= ambushRange && !isDashing)
        {
            // 감지 범위 내 → 돌진 시작
            StartCoroutine(DashToPlayer(currentTile, playerTile));
        }
        // 감지 범위 밖이면 제자리 대기
    }

    private void MoveReflector()
    {
        // 이동 방향이 없으면 제자리
        if (reflectorDir == Vector2Int.zero) return;

        Vector2Int currentTile = GetCurrentTile();
        Vector2Int targetTile = currentTile + reflectorDir;

        if (!caveGenerator.IsWall(targetTile.x, targetTile.y))
        {
            StartCoroutine(MoveToTile(targetTile));
        }
        else
        {
            // 벽에 막히면 정지
            reflectorDir = Vector2Int.zero;
        }
    }

    // 음파가 반사체에 닿았을 때 EchoRing에서 호출
    public void OnHitByEcho(Vector2 echoOrigin)
    {
        if (entityType != EntityType.Reflector) return;

        // 음파 발원지 반대 방향으로 이동 방향 설정
        Vector2 toEntity = (Vector2)transform.position - echoOrigin;

        // 더 강한 축 방향으로만 이동 (대각선 방지)
        if (Mathf.Abs(toEntity.x) >= Mathf.Abs(toEntity.y))
            reflectorDir = toEntity.x > 0 ? Vector2Int.right : Vector2Int.left;
        else
            reflectorDir = toEntity.y > 0 ? Vector2Int.up : Vector2Int.down;

        Debug.Log($"[Reflector] 음파 피격 - 이동 방향: {reflectorDir}");
    }

    private IEnumerator DashToPlayer(Vector2Int from, Vector2Int to)
    {
        isDashing = true;
        isMoving = true;

        Vector2Int currentTile = from;
        Vector2Int dir = GetDirectionToPlayer(from, to);

        // ambushDashRange 칸만큼 또는 플레이어/벽 만날 때까지 돌진
        for (int i = 0; i < ambushDashRange; i++)
        {
            Vector2Int nextTile = currentTile + dir;

            // 벽이면 돌진 중단
            if (caveGenerator.IsWall(nextTile.x, nextTile.y)) break;

            // 한 칸 이동
            yield return StartCoroutine(MoveToTile(nextTile));
            currentTile = nextTile;

            // 플레이어 타일에 도달하면 중단
            if (currentTile == GetPlayerTile()) break;
        }

        isDashing = false;
        isMoving = false;
    }

    private IEnumerator MoveToTile(Vector2Int targetTile)
    {
        isMoving = true;

        Vector3 startPos = transform.position;
        Vector3 targetPos = caveGenerator.TileToWorld(targetTile);
        float elapsed = 0f;

        // moveTime 동안 부드럽게 이동
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

    // 현재 위치를 타일 좌표로 변환
    private Vector2Int GetCurrentTile()
    {
        return new Vector2Int(
            Mathf.RoundToInt(transform.position.x / tileSize),
            Mathf.RoundToInt(transform.position.y / tileSize)
        );
    }

    // 플레이어 위치를 타일 좌표로 변환
    private Vector2Int GetPlayerTile()
    {
        return new Vector2Int(
            Mathf.RoundToInt(playerTransform.position.x / tileSize),
            Mathf.RoundToInt(playerTransform.position.y / tileSize)
        );
    }

    // 현재 타일에서 플레이어 타일 방향으로 한 칸 이동할 방향 계산
    private Vector2Int GetDirectionToPlayer(Vector2Int from, Vector2Int to)
    {
        Vector2Int diff = to - from;

        // 더 먼 축 방향으로 우선 이동 (대각선 방지)
        if (Mathf.Abs(diff.x) >= Mathf.Abs(diff.y))
            return diff.x > 0 ? Vector2Int.right : Vector2Int.left;
        else
            return diff.y > 0 ? Vector2Int.up : Vector2Int.down;
    }

    // 플레이어와 충돌 시 데미지 처리
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            HealthManager healthManager = FindFirstObjectByType<HealthManager>();
            if (healthManager != null)
                healthManager.TakeDamage(1);

            Debug.Log($"[Entity] 플레이어 충돌 - 데미지!");
        }
    }
}