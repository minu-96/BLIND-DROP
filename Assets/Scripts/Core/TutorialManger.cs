// Scripts/Core/TutorialManager.cs
// 튜토리얼 전체 진행 관리
// 단계별로 플레이어 행동을 감지하며 자동 진행
// 0: 인사       1: 이동        2: 음파 발사
// 3: 퍼펙트에코  4: 타이밍이동  5: 파문+Chaser
// 6: 파편+보너스HP  7: Wanderer  8: Ambusher
// 9: Reflector  10: 완료

using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;
using System.Collections;

public class TutorialManager : MonoBehaviour
{
    [Header("참조")]
    [SerializeField] private PlayerController playerController;
    [SerializeField] private EchoEmitter echoEmitter;
    [SerializeField] private PulseController pulseController;
    [SerializeField] private CaveGenerator caveGenerator;
    [SerializeField] private FragmentManager fragmentManager;
    [SerializeField] private Transform playerTransform;

    [Header("UI 참조")]
    [SerializeField] private GameObject portraitBox;
    [SerializeField] private TextMeshProUGUI dialogText;
    [SerializeField] private TextMeshProUGUI hintText;
    [SerializeField] private Button confirmButton;
    [SerializeField] private Button skipButton;

    [Header("튜토리얼 설정")]
    [SerializeField] private int tutorialSeed = 12345;
    [SerializeField] private GameObject fragmentPrefab; // 파편 프리팹 (직접 연결)

    // CaveGenerator에서 받아온 튜토리얼 적 참조
private GameObject tutorialWanderer;
private GameObject tutorialChaser;
private GameObject tutorialAmbusher;
private GameObject tutorialReflector;

    // 행동 감지 플래그
    private bool timingFail       = false; // 파문 발생
    private bool perfectEchoFired = false; // 퍼펙트 에코 성공
    private int  fragmentCount    = 0;     // 수집한 파편 수

    void Awake()
    {
        playerController.LockInput();
    }

    void Start()
    {
        caveGenerator.GenerateFloor(1, tutorialSeed);
        StartCoroutine(InitAfterGenerate());
    }

    private IEnumerator InitAfterGenerate()
{
    yield return null;

    playerTransform.position = caveGenerator.TileToWorld(caveGenerator.entrancePos);

    // CaveGenerator에서 직접 참조 받아오기
    tutorialWanderer  = caveGenerator.TutorialWanderer;
    tutorialChaser    = caveGenerator.TutorialChaser;
    tutorialAmbusher  = caveGenerator.TutorialAmbusher;
    tutorialReflector = caveGenerator.TutorialReflector;

    playerController.onRipple           += OnRippleDetected;
    fragmentManager.onFragmentCollected += OnFragmentCollected;

    skipButton.onClick.AddListener(SkipTutorial);
    SetHint("");

    StartCoroutine(RunTutorial());
}

    void OnDestroy()
    {
        if (playerController != null)
            playerController.onRipple -= OnRippleDetected;
        if (fragmentManager != null)
            fragmentManager.onFragmentCollected -= OnFragmentCollected;
    }

    private void OnRippleDetected()    => timingFail = true;
    private void OnFragmentCollected() => fragmentCount++;

    // =====================
    // 튜토리얼 메인 흐름
    // =====================
    private IEnumerator RunTutorial()
    {
        // --- 단계 0: 인사 ---
        yield return ShowDialog("여기는 어둠 속 동굴이야.\n완전한 어둠 속에서 살아남아야 해.", true);
        yield return ShowDialog("기본적인 것들을 하나씩 알려줄게.", true);

        // --- 단계 1: 이동 ---
        yield return ShowDialog("먼저 움직이는 법부터.\nWASD 또는 방향키로 이동해봐.", false);
        SetHint("[ WASD ] 또는 방향키로 이동해보세요");
        playerController.UnlockInput();

        Vector3 startPos = playerTransform.position;
        while (Vector3.Distance(playerTransform.position, startPos) < 0.5f)
            yield return null;

        playerController.LockInput();
        SetHint("");
        yield return ShowDialog("좋아! 이제 어둠 속에서 길을 찾아야 해.", true);

        // --- 단계 2: 음파 발사 ---
        yield return ShowDialog("마우스 클릭으로 음파를 발사해봐.\n주변 지형이 잠깐 보일 거야.", false);
        SetHint("[ 마우스 좌클릭 ] 으로 음파를 발사해보세요");
        playerController.UnlockInput();

        // 쿨타임이 0으로 떨어지면 발사된 것
        float prevProgress = echoEmitter.CooldownProgress;
        while (true)
        {
            if (echoEmitter.CooldownProgress < 0.1f && prevProgress > 0.9f) break;
            prevProgress = echoEmitter.CooldownProgress;
            yield return null;
        }

        playerController.LockInput();
        SetHint("");
        yield return ShowDialog("음파가 닿은 곳만 잠깐 보여.\n어둠 속에서 이게 유일한 눈이야.", true);

        // --- 단계 3: 퍼펙트 에코 ---
        yield return ShowDialog("화면 테두리가 깜빡이는 거 보여?\n저게 동굴의 맥박이야.", true);
        yield return ShowDialog("맥박 타이밍에 맞춰 클릭하면\n음파 범위가 2배로 넓어져!", false);
        SetHint("테두리가 빛날 때 클릭해보세요 (퍼펙트 에코)");
        playerController.UnlockInput();

        // IsPulseTime() + 클릭 = 퍼펙트 에코 감지
        // 쿨타임 게이지가 0으로 떨어지는 순간 + 펄스 타이밍이면 성공
        perfectEchoFired = false;
        while (!perfectEchoFired)
        {
            prevProgress = echoEmitter.CooldownProgress;
            yield return null;
            // 발사 순간 감지
            if (echoEmitter.CooldownProgress < 0.1f && prevProgress > 0.9f)
            {
                if (playerController.IsPulseTime())
                {
                    perfectEchoFired = true;
                }
                else
                {
                    // 타이밍 실패 안내
                    playerController.LockInput();
                    SetHint("");
                    yield return ShowDialog("아깝다! 테두리가 빛날 때 클릭해야 해.\n다시 해봐.", false);
                    SetHint("테두리가 빛날 때 클릭해보세요 (퍼펙트 에코)");
                    playerController.UnlockInput();
                }
            }
        }

        playerController.LockInput();
        SetHint("");
        yield return ShowDialog("완벽해! 넓은 음파로 더 멀리까지 볼 수 있어.", true);

        // --- 단계 4: 맥박 타이밍 이동 ---
        yield return ShowDialog("맥박 타이밍에 맞춰 이동하면\n소음 없이 조용히 움직일 수 있어.", false);
        SetHint("테두리가 빛날 때 이동해보세요 (조용한 이동)");
        playerController.UnlockInput();

        while (true)
        {
            Vector3 beforePos = playerTransform.position;
            timingFail = false;

            // 이동할 때까지 대기
            while (Vector3.Distance(playerTransform.position, beforePos) < 0.5f)
                yield return null;

            // 한 프레임 대기 (파문 발생 여부 확인)
            yield return null;

            if (!timingFail)
            {
                break; // 타이밍 성공
            }
            else
            {
                // 타이밍 실패 - 재시도
                playerController.LockInput();
                SetHint("");
                yield return ShowDialog("타이밍을 놓쳤어.\n테두리가 빛날 때 다시 해봐.", false);
                SetHint("테두리가 빛날 때 이동해보세요 (조용한 이동)");
                playerController.UnlockInput();
            }
        }

        playerController.LockInput();
        SetHint("");
        yield return ShowDialog("타이밍을 맞추면 적이 눈치채지 못해.", true);

        // --- 단계 5: 파문 패널티 + Chaser 등장 ---
        // Chaser 활성화
        if (tutorialChaser != null) tutorialChaser.SetActive(true);

        yield return ShowDialog("반대로 타이밍을 놓치면 파문이 생겨.\n파문은 적을 자극해서 너한테 다가오게 해.", false);
        SetHint("테두리가 꺼져있을 때 이동해보세요 (파문 확인)");
        playerController.UnlockInput();
        timingFail = false;

        while (!timingFail)
            yield return null;

        yield return new WaitForSeconds(0.5f);
        playerController.LockInput();
        SetHint("");

        yield return ShowDialog("저 파문이 퍼지는 게 보여?\n추격자는 저 신호를 따라와.", true);

        // Chaser 비활성화 (다음 단계 혼란 방지)
        if (tutorialChaser != null) tutorialChaser.SetActive(false);

        // --- 단계 6: 파편 수집 + 보너스 HP ---
        // 파편 2개를 맵에 강제 배치
        SpawnTutorialFragments();

        yield return ShowDialog("동굴 곳곳에 파편이 숨겨져 있어.\n파편 2개를 모으면 보너스 체력이 생겨!", false);
        SetHint("파편 2개를 찾아 수집해보세요");
        playerController.UnlockInput();
        fragmentCount = 0;

        // 파편 2개 수집할 때까지 대기
        while (fragmentCount < 2)
            yield return null;

        playerController.LockInput();
        SetHint("");
        yield return ShowDialog("체력이 늘었어! 파편은 생존의 열쇠야.", true);

        // --- 단계 7: Wanderer 소개 ---
        if (tutorialWanderer != null) tutorialWanderer.SetActive(true);

        yield return ShowDialog("이제 적들을 소개해줄게.\n음파를 발사해서 찾아봐.", true);
        playerController.UnlockInput();
        
        yield return ShowDialog("저건 배회자야.\n맥박마다 랜덤한 방향으로 움직여.\n예측하기 어려우니 조심해.", true);

        

        if (tutorialWanderer != null) tutorialWanderer.SetActive(false);

        // --- 단계 8: Ambusher 소개 ---
        if (tutorialAmbusher != null) tutorialAmbusher.SetActive(true);

        

        yield return ShowDialog("저건 매복자야.\n평소엔 가만히 있다가\n네가 가까이 가면 갑자기 돌진해!", true);
        yield return ShowDialog("음파로 위치를 파악하고\n절대 가까이 가지 마.", true);

        if (tutorialAmbusher != null) tutorialAmbusher.SetActive(false);

        // --- 단계 9: Reflector 소개 ---
        if (tutorialAmbusher != null) tutorialReflector.SetActive(true);

        yield return ShowDialog("저건 반사체야.\n음파가 닿으면 반대 방향으로 이동 시작해.\n음파를 함부로 쏘면 안 돼!", true);
        yield return ShowDialog("음파로 위치만 파악하고\n이동 방향을 예측해서 피해야 해.", true);
        playerController.LockInput();

        if (tutorialAmbusher != null) tutorialReflector.SetActive(false);

        // --- 단계 10: 완료 ---
        yield return ShowDialog("이제 준비됐어!\n계단을 찾아서 최대한 깊이 내려가봐.", true);
        yield return ShowDialog("행운을 빌어.", true);

        PlayerPrefs.SetInt("TutorialDone", 1);
        PlayerPrefs.SetInt("IsWeeklyCave", 0);
        PlayerPrefs.Save();
        SceneManager.LoadScene("SceneGame");
    }

    // 튜토리얼 전용 파편 2개 강제 스폰
    // 플레이어 근처 바닥 타일에 배치 (찾기 쉽게)
    private void SpawnTutorialFragments()
    {
        // FragmentPickup 프리팹을 CaveGenerator에서 받아올 수 없으니
        // CaveGenerator의 SpawnFragments를 직접 호출하는 대신
        // 맵에서 바닥 타일 2개를 찾아 FragmentPickup 오브젝트를 찾아서 활성화
        // → 이미 CaveGenerator가 생성한 Fragment가 있으면 활성화
        // → 없으면 별도 프리팹 사용

        // TutorialFragmentSpawner에 프리팹 연결해서 사용
        if (fragmentPrefab == null) return;

        for (int i = 0; i < 2; i++)
        {
            // 입구 근처 바닥 타일 찾기 (플레이어가 쉽게 찾을 수 있도록)
            Vector2Int tile = GetFloorTileNearEntrance(i * 2);
            if (tile == Vector2Int.zero) continue;

            Vector3 pos = caveGenerator.TileToWorld(tile);
            Instantiate(fragmentPrefab, pos, Quaternion.identity);
        }
    }

    // 입구 근처 바닥 타일 찾기 (offset으로 위치 분산)
    private Vector2Int GetFloorTileNearEntrance(int offset)
    {
        Vector2Int entrance = caveGenerator.entrancePos;

        for (int d = offset; d < offset + 5; d++)
        {
            for (int dx = -d; dx <= d; dx++)
            {
                for (int dy = -d; dy <= d; dy++)
                {
                    int x = entrance.x + dx;
                    int y = entrance.y + dy;
                    if (!caveGenerator.IsWall(x, y) &&
                        new Vector2Int(x, y) != entrance)
                        return new Vector2Int(x, y);
                }
            }
        }
        return Vector2Int.zero;
    }

    // =====================
    // UI 헬퍼
    // =====================
    private IEnumerator ShowDialog(string message, bool waitForConfirm)
    {
        portraitBox.SetActive(true);
        confirmButton.gameObject.SetActive(waitForConfirm);

        // 타이핑 효과
        dialogText.text = "";
        foreach (char c in message)
        {
            dialogText.text += c;
            yield return new WaitForSeconds(0.04f);
        }

        if (waitForConfirm)
        {
            bool confirmed = false;
            confirmButton.onClick.AddListener(() => confirmed = true);
            while (!confirmed) yield return null;
            confirmButton.onClick.RemoveAllListeners();
        }
        else
        {
            yield return new WaitForSeconds(1.5f);
        }

        HideDialog();
    }

    private void HideDialog() => portraitBox.SetActive(false);
    private void SetHint(string msg)
    {
        if (hintText != null) hintText.text = msg;
    }

    private void SkipTutorial()
    {
        PlayerPrefs.SetInt("TutorialDone", 1);
        PlayerPrefs.SetInt("IsWeeklyCave", 0);
        PlayerPrefs.Save();
        SceneManager.LoadScene("SceneGame");
    }
}