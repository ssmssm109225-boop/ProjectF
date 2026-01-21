using UnityEngine;
using System.Collections;
using UnityEngine;

public class GameFlowManager : MonoBehaviour
{
    public enum GameState
    {
        Ready,
        Flying,
        GameOver,
        Result
    }

    [Header("References")]
    [SerializeField] private InputRouter inputRouter;              // 입력 라우터
    [SerializeField] private LaunchGaugeController launchGauge;     // 발사 게이지
    [SerializeField] private PlayerController player;
    [SerializeField] private PlayerStateController playerState;
    [SerializeField] private Transform playerStartPoint; // 선택: 시작 위치 오브젝트
    [SerializeField] private ChunkSpawner chunkSpawner;
    [SerializeField] private RunStatsManager runStats;
    [SerializeField] private HardMovingTrapSpawner movingTrapSpawner;
    [SerializeField] private RedGaugeController redGauge;  // ✅ Red 게이지 추가
    private Vector2 cachedStartPos;


    [Header("State")]
    [SerializeField] private GameState currentState = GameState.Ready;

    [Header("UI")]
    [SerializeField] private GameOverPanelUI gameOverUI;

    [Header("Revive (Prototype)")]
    [SerializeField] private bool allowReviveOnce = true;
    [SerializeField] private float reviveInvincibleDuration = 3f;
    [SerializeField] private ParticleSystem reviveInvincibleVfx;
    [SerializeField] private AudioSource reviveInvincibleSfx;
    private bool usedReviveThisRun = false;
    private bool isReviveFlowActive = false;
    private bool isReviveInvincibleActive = false;
    private Coroutine reviveInvincibleCoroutine;
    [SerializeField] private float reviveLaunchImpulseMultiplier = 3f;
    private float lastLaunchGauge01 = 0f;

    public GameState CurrentState => currentState;

    // ✅ 싱글톤 패턴: 다른 스크립트에서 쉽게 접근
    public static GameFlowManager Instance { get; private set; }

    private void Awake()
    {
        // ✅ 싱글톤 초기화
        if (Instance != null && Instance != this)
        {
            Debug.LogError("[GameFlowManager] Multiple instances detected!");
            Destroy(gameObject);
            return;
        }
        Instance = this;

        Debug.Log("[GameFlowManager] Singleton initialized");

        // 시작 위치는 1) StartPoint가 있으면 그걸, 2) 없으면 플레이어 현재 위치
        if (playerStartPoint != null) cachedStartPos = playerStartPoint.position;
        else if (player != null) cachedStartPos = player.transform.position;
    }


    private void OnEnable()
    {
        // 이벤트 구독(Subscribe)
        if (inputRouter != null)
        {
            inputRouter.OnTapReady += HandleTapReady;
            inputRouter.OnTapFlying += HandleTapFlying;
            inputRouter.OnTapAny += HandleTapAny;
        }
    }

    private void OnDisable()
    {
        // 이벤트 해제(Unsubscribe) - Unity 6에서 특히 중요(중복 구독 방지)
        if (inputRouter != null)
        {
            inputRouter.OnTapReady -= HandleTapReady;
            inputRouter.OnTapFlying -= HandleTapFlying;
            inputRouter.OnTapAny -= HandleTapAny;
        }
    }

    private void Start()
    {
        SetState(GameState.Ready);
    }

    public void SetState(GameState newState, bool resetReviveFlag = true)
    {
        currentState = newState;
        Debug.Log($"[GameFlow] State -> {currentState}");

        // 상태 진입 시 UI/동작 세팅
        switch (currentState)
        {
            case GameState.Ready:
                if (resetReviveFlag)
                    usedReviveThisRun = false; // ✅ 런 시작 시 부활 가능 초기화
                if (player != null)
                {
                    player.SetSimulated(false);  // ✅ 물리 비활성화 (정지 상태)
                    Debug.Log("[GameFlow] Player simulated OFF");
                }
                if (redGauge != null)
                {
                    redGauge.OnReadyState();  // ✅ 게이지 리셋 (maxTime으로)
                }
                if (gameOverUI != null) gameOverUI.Hide();
                if (launchGauge != null)
                {
                    launchGauge.SetVisible(true);
                    launchGauge.SetRunning(true);
                    launchGauge.ResetGauge();
                }
                break;

            case GameState.Flying:
                if (player != null)
                {
                    player.SetSimulated(true);  // ✅ 물리 활성화 (발사)
                    Debug.Log("[GameFlow] Player simulated ON");
                }
                if (gameOverUI != null) gameOverUI.Hide();
                if (launchGauge != null) { launchGauge.SetVisible(false); launchGauge.SetRunning(false); }
                break;

            case GameState.GameOver:
                isReviveFlowActive = false;
                if (isReviveInvincibleActive)
                    EndReviveInvincibility();
                if (launchGauge != null) { launchGauge.SetVisible(false); launchGauge.SetRunning(false); }

                // ✅ GameOver 패널 표시 + 부활 가능 여부 전달
                bool canRevive = allowReviveOnce && !usedReviveThisRun;
                if (gameOverUI != null)
                {
                    Debug.Log("[GameFlow] Calling ShowPanel with canRevive=" + canRevive);
                    gameOverUI.ShowPanel(canRevive);
                }
                else
                {
                    Debug.LogError("[GameFlow] gameOverUI is NULL! Cannot show GameOver panel.");
                }
                break;
        }
    }

    // Ready 상태에서 탭했을 때
    private void HandleTapReady()
    {
        if (currentState != GameState.Ready) return;

        float v01 = (launchGauge != null) ? launchGauge.Sample01() : 0f;
        lastLaunchGauge01 = v01;

        SetState(GameState.Flying);          // 먼저 물리 ON
        if (player != null)
        {
            player.LaunchByGauge01(v01);
        }

        // 다음 챕터에서 여기서 Player.Launch(power) 호출할 예정
        SetState(GameState.Flying);

        if (isReviveFlowActive)
        {
            isReviveFlowActive = false;
            StartReviveInvincibleCountdown();
        }
    }

    // Flying 상태에서 탭했을 때 (아직은 로그만)
    private void HandleTapFlying()
    {
        if (currentState != GameState.Flying) return;

        if (isReviveInvincibleActive)
            return;

        if (playerState != null)
            playerState.Toggle();
    }
    private void HandleTapAny()
    {
        // ✅ GameOver 상태에서는 탭으로 아무것도 하지 않음 (UI 버튼만 사용)
    }
    public void EnterGameOver()
    {
        SetState(GameState.GameOver);
    }

    /// <summary>
    /// GameOver 패널의 Retry 버튼에서 호출
    /// </summary>
    public void RetryFromGameOver()
    {
        if (currentState != GameState.GameOver) return;

        Debug.Log("[GameFlow] RetryFromGameOver called");

        // 1) 플레이어 리셋
        if (player != null)
            player.ResetRun(cachedStartPos);

        // 2) 청크 리셋
        if (chunkSpawner != null)
            chunkSpawner.ResetChunks();

        // 3) 뮤빙 트랩 리셋
        if (movingTrapSpawner != null)
            movingTrapSpawner.ResetRun();

        // 4) Ready 상태로 복귀
        SetState(GameState.Ready);
    }

    /// <summary>
    /// (프로토) 부활: 나중에 리워드 광고 성공 콜백에서 이걸 호출하도록 바꾸면 됨
    /// </summary>
    public void TryRevivePlaceholder()
    {
        if (currentState != GameState.GameOver) return;
        if (!allowReviveOnce || usedReviveThisRun) return;

        usedReviveThisRun = true;
        isReviveFlowActive = true;
        BeginReviveInvincibility();

        // Trap에서 rb.simulated=false로 꺼놨으면 다시 켜기
        if (player != null)
        {
            // 정지 지점에서 재개
            // (Freeze를 했다면 simulated을 켜야 함)
            var rb = player.GetComponent<Rigidbody2D>();
            if (rb != null) rb.simulated = true;
        }
        // ✅ 그 자리에서 발사 게이지 재소환
        SetState(GameState.Ready, resetReviveFlag: false);
    }
    private void BeginReviveInvincibility()
    {
        isReviveInvincibleActive = true;

        if (reviveInvincibleCoroutine != null)
        {
            StopCoroutine(reviveInvincibleCoroutine);
            reviveInvincibleCoroutine = null;
        }

        if (redGauge != null)
            redGauge.SetGaugePaused(true);

        if (playerState != null)
            playerState.SetState(PlayerStateController.PlayerState.Red);

        if (reviveInvincibleVfx != null)
            reviveInvincibleVfx.Play();

        if (reviveInvincibleSfx != null)
            reviveInvincibleSfx.Play();
    }

    private void StartReviveInvincibleCountdown()
    {
        if (reviveInvincibleCoroutine != null)
            StopCoroutine(reviveInvincibleCoroutine);

        reviveInvincibleCoroutine = StartCoroutine(EndReviveInvincibleAfterDelay());
    }

    private IEnumerator EndReviveInvincibleAfterDelay()
    {
        yield return new WaitForSeconds(reviveInvincibleDuration);
        EndReviveInvincibility();
    }

    private void EndReviveInvincibility()
    {
        isReviveInvincibleActive = false;

        if (redGauge != null)
            redGauge.SetGaugePaused(false);

        if (playerState != null)
            playerState.SetState(PlayerStateController.PlayerState.Red);

        if (reviveInvincibleVfx != null)
            reviveInvincibleVfx.Stop();

        if (reviveInvincibleSfx != null)
            reviveInvincibleSfx.Stop();

        if (reviveInvincibleCoroutine != null)
        {
            StopCoroutine(reviveInvincibleCoroutine);
            reviveInvincibleCoroutine = null;
        }
    }
}
