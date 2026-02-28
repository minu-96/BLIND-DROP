using UnityEngine;

public class FragmentPickup : MonoBehaviour
{
    // 플레이어가 파편에 닿으면 수집 처리
    private void OnTriggerEnter2D(Collider2D other)
    {
        // 충돌한 오브젝트가 Player 태그인지 확인
        if (other.CompareTag("Player"))
        {
            // FragmentManager에 수집 알림
            FragmentManager fragmentManager = FindFirstObjectByType<FragmentManager>();
            if (fragmentManager != null)
                fragmentManager.CollectFragment();

            // 파편 오브젝트 삭제
            Destroy(gameObject);

            Debug.Log("[Fragment] 파편 획득!");
        }
    }
}