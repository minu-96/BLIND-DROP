using UnityEngine;
using System.Collections;

public class StairTile : MonoBehaviour
{
    // 층 로드 직후 계단 감지 방지 쿨타임
    private bool isReady = false;

    private void Start()
    {
        // 0.5초 후부터 감지 시작
        StartCoroutine(ReadyDelay());
    }

    private IEnumerator ReadyDelay()
    {
        yield return new WaitForSeconds(0.5f);
        isReady = true;
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        // 준비 안 됐으면 무시
        if (!isReady) return;

        if (other.CompareTag("Player"))
        {
            Debug.Log("[Stair] 다음 층으로 이동!");
            GameManager.Instance.OnPlayerReachedExit();
        }
    }
}