using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    [Header("참조")]
    [SerializeField] private Transform target; // 따라갈 대상 (Player)

    [Header("설정")]
    [SerializeField] private float smoothSpeed = 5f; // 카메라 이동 부드러움 정도
    [SerializeField] private Vector3 offset = new Vector3(0, 0, -10f); // 카메라 Z축 오프셋 (2D에서 필수)

    void LateUpdate()
    {
        // LateUpdate: 플레이어 이동이 완전히 끝난 후 카메라 이동
        // Update에서 하면 카메라가 플레이어보다 한 프레임 늦게 반응함
        if (target == null) return;

        // 목표 위치 = 플레이어 위치 + 오프셋
        Vector3 targetPos = target.position + offset;

        // Lerp로 부드럽게 따라가기
        transform.position = Vector3.Lerp(transform.position, targetPos, smoothSpeed * Time.deltaTime);
    }
}