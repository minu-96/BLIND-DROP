using UnityEngine;

public class StairTile : MonoBehaviour
{
    private void OnTriggerEnter2D(Collider2D other)
    {
        // 플레이어가 계단에 닿으면 다음 층으로 이동
        if (other.CompareTag("Player"))
        {
            Debug.Log("[Stair] 다음 층으로 이동!");
            GameManager.Instance.OnPlayerReachedExit();
        }
    }
}