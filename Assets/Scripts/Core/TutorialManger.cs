// Scripts/Core/TutorialManager.cs
// 튜토리얼 단계별 진행 관리
// - 포터 대사로 지시 → 플레이어 행동 감지 → 다음 단계 자동 진행
// 단계 순서:
// 0. 인사 + 상황 설명 (확인 버튼)
// 1. WASD 이동 가르치기 (이동하면 자동 진행)
// 2. 음파 발사 가르치기 (클릭하면 자동 진행)
// 3. 맥박 타이밍 이동 가르치기 (타이밍 성공하면 자동 진행)
// 4. 파문 패널티 가르치기 (타이밍 실패하면 자동 진행)
// 5. 완료 + SceneGame으로 이동

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
    [SerializeField] private Transform playerTransform;

    [Header("UI 참조")]
    [SerializeField] private GameObject portraitBox;        // 포터 대사 박스
    [SerializeField] private TextMeshProUGUI dialogText;    // 포터 대사 텍스트
    [SerializeField] private TextMeshProUGUI hintText;      // 화면 중앙 힌트
    [SerializeField] private Button confirmButton;          // 대사 박스 확인 버튼
    [SerializeField] private Button skipButton;             // 스킵 버튼

    [Header("튜토리얼 설정")]
    [SerializeField] private int tutorialSeed = 12345;      // 고정 맵 시드

    // 현재 튜토리얼 단계
    private int currentStep = 0;

    // 각 단계별 행동 감지 플래그
    private bool playerMoved       = false; // 이동 감지
    private bool echoFired         = false; // 음파 발사 감지
    private bool timingSuccess     = false; // 맥박 타이밍 성공 감지
    private bool timingFail        = false; // 파문 발생 감지

    void Awake()
{
    // 입력 차단은 가장 먼저
    playerController.LockInput();
}

void Start()
{
    // GenerateFloor는 모든 오브젝트 Start() 이후 실행 보장
    caveGenerator.GenerateFloor(1, tutorialSeed);

    // 한 프레임 대기 후 플레이어 배치 (맵 생성 완전히 끝난 후)
    StartCoroutine(InitAfterGenerate());
}

private IEnumerator InitAfterGenerate()
{
    // 한 프레임 대기
    yield return null;

    // 맵 생성 후 플레이어 배치
    playerTransform.position = caveGenerator.TileToWorld(caveGenerator.entrancePos);

    // 스킵 버튼 연결
    skipButton.onClick.AddListener(SkipTutorial);

    // 파문 감지 이벤트 구독
    playerController.onRipple += OnRippleDetected;

    // 힌트 텍스트 숨김
    SetHint("");

    // 튜토리얼 시작
    StartCoroutine(RunTutorial());
}

    void OnDestroy()
    {
        // 이벤트 구독 해제
        if (playerController != null)
            playerController.onRipple -= OnRippleDetected;
    }

    // 파문 발생 감지 콜백
    private void OnRippleDetected()
    {
        timingFail = true;
    }

    // 튜토리얼 전체 흐름 코루틴
    private IEnumerator RunTutorial()
    {
        // --- 단계 0: 인사 ---
        yield return StartCoroutine(ShowDialog(
            "..여기는 어둠 속 동굴이야.\n완전한 어둠 속에서 살아남아야 해.",
            waitForConfirm: true
        ));

        yield return StartCoroutine(ShowDialog(
            "먼저 움직이는 법부터 배워보자.",
            waitForConfirm: true
        ));

        // --- 단계 1: 이동 ---
        HideDialog();
        SetHint("[ WASD ] 또는 방향키로 이동해보세요");
        playerController.UnlockInput();

        // 플레이어가 이동할 때까지 대기
        Vector3 startPos = playerTransform.position;
        while (Vector3.Distance(playerTransform.position, startPos) < 0.5f)
            yield return null;

        playerMoved = true;
        playerController.LockInput();
        SetHint("");

        yield return StartCoroutine(ShowDialog(
            "좋아! 이동할 수 있어.\n하지만 이 어둠 속에서 길을 어떻게 찾지?",
            waitForConfirm: true
        ));

        // --- 단계 2: 음파 발사 ---
        yield return StartCoroutine(ShowDialog(
            "마우스 클릭으로 음파를 발사해봐.\n주변 지형이 잠깐 보일 거야.",
            waitForConfirm: false
        ));

        SetHint("[ 마우스 좌클릭 ] 으로 음파를 발사해보세요");
        playerController.UnlockInput();

        // 음파 발사 감지 (EchoEmitter의 쿨타임 게이지가 0이 되면 발사된 것)
        float prevProgress = echoEmitter.CooldownProgress;
        while (true)
        {
            // CooldownProgress가 1→0으로 떨어지면 발사된 것
            if (echoEmitter.CooldownProgress < 0.1f && prevProgress > 0.9f)
            {
                echoFired = true;
                break;
            }
            prevProgress = echoEmitter.CooldownProgress;
            yield return null;
        }

        playerController.LockInput();
        SetHint("");

        yield return StartCoroutine(ShowDialog(
            "음파가 닿은 곳만 잠깐 보여.\n어둠 속에서 이게 유일한 눈이야.",
            waitForConfirm: true
        ));

        // --- 단계 3: 맥박 타이밍 이동 ---
        yield return StartCoroutine(ShowDialog(
            "화면 테두리가 깜빡이는 거 보여?\n저게 동굴의 맥박이야.",
            waitForConfirm: true
        ));

        yield return StartCoroutine(ShowDialog(
            "맥박 타이밍에 맞춰 이동하면\n소음 없이 조용히 움직일 수 있어.",
            waitForConfirm: false
        ));

        SetHint("테두리가 빛날 때 이동해보세요 (조용한 이동 성공)");
        playerController.UnlockInput();

        // 타이밍 성공 감지 - onRipple이 발생하지 않고 이동하면 성공
        Vector3 beforePos = playerTransform.position;
        timingFail = false;

        while (true)
        {
            // 위치가 바뀌고 파문이 없으면 타이밍 성공
            if (Vector3.Distance(playerTransform.position, beforePos) > 0.5f && !timingFail)
            {
                timingSuccess = true;
                break;
            }
            // 파문이 발생하면 다시 시도
            if (timingFail)
            {
                timingFail = false;
                beforePos = playerTransform.position;
            }
            yield return null;
        }

        playerController.LockInput();
        SetHint("");

        yield return StartCoroutine(ShowDialog(
            "완벽해! 타이밍을 맞추면 적이 눈치채지 못해.",
            waitForConfirm: true
        ));

        // --- 단계 4: 파문 패널티 ---
        yield return StartCoroutine(ShowDialog(
            "반대로 타이밍을 놓치면 파문이 생겨.\n파문은 적을 자극해서 너한테 다가오게 해.",
            waitForConfirm: false
        ));

        SetHint("테두리가 꺼져있을 때 이동해보세요 (파문 발생 확인)");
        playerController.UnlockInput();
        timingFail = false;

        // 파문 발생할 때까지 대기
        while (!timingFail)
            yield return null;

        playerController.LockInput();
        SetHint("");

        yield return StartCoroutine(ShowDialog(
            "저 파문이 퍼지는 게 보여?\n적들은 저 신호를 따라와.",
            waitForConfirm: true
        ));

        // --- 단계 5: 완료 ---
        yield return StartCoroutine(ShowDialog(
            "이제 준비됐어. 최대한 깊이 내려가봐.\n행운을 빌어.",
            waitForConfirm: true
        ));

        // 튜토리얼 완료 플래그 저장 후 게임 씬으로
        PlayerPrefs.SetInt("TutorialDone", 1);
        PlayerPrefs.SetInt("IsWeeklyCave", 0);
        PlayerPrefs.Save();
        SceneManager.LoadScene("SceneGame");
    }

    // 대사 박스 표시 코루틴
    // waitForConfirm: true면 확인 버튼 누를 때까지 대기
    //                 false면 2초 후 자동으로 넘어감
    private IEnumerator ShowDialog(string message, bool waitForConfirm)
    {
        portraitBox.SetActive(true);
        confirmButton.gameObject.SetActive(waitForConfirm);

        // 타이핑 효과: 한 글자씩 표시
        dialogText.text = "";
        foreach (char c in message)
        {
            dialogText.text += c;
            yield return new WaitForSeconds(0.04f); // 타이핑 속도
        }

        if (waitForConfirm)
        {
            // 확인 버튼 누를 때까지 대기
            bool confirmed = false;
            confirmButton.onClick.AddListener(() => confirmed = true);
            while (!confirmed) yield return null;
            confirmButton.onClick.RemoveAllListeners();
        }
        else
        {
            // 1.5초 후 자동 진행
            yield return new WaitForSeconds(1.5f);
        }
    }

    // 대사 박스 숨기기
    private void HideDialog()
    {
        portraitBox.SetActive(false);
    }

    // 힌트 텍스트 설정
    private void SetHint(string message)
    {
        if (hintText == null) return;
        hintText.text = message;
    }

    // 스킵 버튼 → 바로 SceneGame으로
    private void SkipTutorial()
    {
        PlayerPrefs.SetInt("TutorialDone", 1);
        PlayerPrefs.SetInt("IsWeeklyCave", 0);
        PlayerPrefs.Save();
        SceneManager.LoadScene("SceneGame");
    }
}