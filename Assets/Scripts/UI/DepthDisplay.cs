using UnityEngine;
using TMPro;
using System.Collections;

public class DepthDisplay : MonoBehaviour
{
    [Header("텍스트 참조")]
    [SerializeField] private TextMeshProUGUI depthText;
    [SerializeField] private TextMeshProUGUI bestText;

    [Header("기록 갱신 점멸 설정")]
    [SerializeField] private float flashDuration = 0.15f;
    [SerializeField] private int flashCount = 4;

    private int lastBest = 0;
    private Coroutine flashCoroutine;

    public void UpdateDepth(int currentFloor, int bestFloor)
    {
        if (depthText != null)
            depthText.text = $"Depth: {currentFloor}";

        if (bestText != null)
        {
            bestText.text = $"Best: {bestFloor}";

            // 기록이 갱신됐을 때만 점멸
            if (bestFloor > lastBest && lastBest != 0)
            {
                if (flashCoroutine != null) StopCoroutine(flashCoroutine);
                flashCoroutine = StartCoroutine(FlashBest());
            }
            lastBest = bestFloor;
        }
    }

    private IEnumerator FlashBest()
    {
        for (int i = 0; i < flashCount; i++)
        {
            bestText.color = Color.black;
            yield return new WaitForSeconds(flashDuration);
            bestText.color = Color.white;
            yield return new WaitForSeconds(flashDuration);
        }
        bestText.color = Color.white;
    }
}