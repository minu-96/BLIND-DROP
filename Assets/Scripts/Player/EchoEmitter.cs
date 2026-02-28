using UnityEngine;
using System.Collections;

public class EchoEmitter : MonoBehaviour
{
    [Header("음파 설정")]
    [SerializeField] private float baseRadius = 5f;       // 기본 음파 범위
    [SerializeField] private float expandSpeed = 8f;      // 링이 퍼지는 속도
    [SerializeField] private float baseCooldown = 1.5f;   // 기본 쿨타임 (1~10층)

    [Header("퍼펙트 에코 설정")]
    [SerializeField] private float perfectMultiplier = 2f; // 퍼펙트 에코 시 범위 배수

    [Header("참조")]
    [SerializeField] private PlayerController playerController; // 맥박 타이밍 확인용
    [SerializeField] private GameObject echoRingPrefab;         // 음파 링 프리팹

    // 현재 쿨타임 진행 여부
    private bool isCoolingDown = false;
    private float currentCooldown;

    // 쿨타임 진행도 (UI 게이지용, 0~1)
    public float CooldownProgress { get; private set; } = 1f;

    void Start()
    {
        // 시작 시 기본 쿨타임으로 초기화
        currentCooldown = baseCooldown;
    }

    void Update()
    {
        // 마우스 좌클릭 + 쿨타임이 끝났을 때만 음파 발사
        if (Input.GetMouseButtonDown(0) && !isCoolingDown)
        {
            FireEcho();
        }

        // 쿨타임 진행도 업데이트 (UI 게이지 표시용)
        // isCoolingDown이 아닐 때는 1(가득 참) 유지
    }

    private void FireEcho()
    {
        bool isPerfect = playerController.IsPulseTime();
        float radius = isPerfect ? baseRadius * perfectMultiplier : baseRadius;

        if (isPerfect)
            Debug.Log("[Echo] 퍼펙트 에코! 범위: " + radius);
        else
            Debug.Log("[Echo] 일반 에코. 범위: " + radius);

        // 음파 링 생성
        if (echoRingPrefab != null)
        {
            GameObject ring = Instantiate(echoRingPrefab, transform.position, Quaternion.identity);
            EchoRing echoRing = ring.GetComponent<EchoRing>();
            if (echoRing != null)
                echoRing.Initialize(radius, expandSpeed, isPerfect);
        }

        // 어둠 레이어에 조명 효과 요청 (추가된 부분)
        DarknessLayer darknessLayer = FindFirstObjectByType<DarknessLayer>();
        if (darknessLayer != null)
            darknessLayer.OnEchoFired(transform.position, isPerfect);

        StartCoroutine(CooldownRoutine());
    }

    private IEnumerator CooldownRoutine()
    {
        isCoolingDown = true;
        CooldownProgress = 0f;

        float elapsed = 0f;

        // currentCooldown 시간 동안 진행도를 0→1로 채움
        while (elapsed < currentCooldown)
        {
            elapsed += Time.deltaTime;
            CooldownProgress = Mathf.Clamp01(elapsed / currentCooldown);
            yield return null;
        }

        CooldownProgress = 1f;
        isCoolingDown = false;
    }

    // GameManager에서 층수에 따라 쿨타임 조정할 때 호출
    public void SetCooldown(float newCooldown)
    {
        currentCooldown = newCooldown;
    }
}