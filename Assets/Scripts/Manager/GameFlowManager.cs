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
    private Vector2 cachedStartPos;


    [Header("State")]
    [SerializeField] private GameState currentState = GameState.Ready;

    public GameState CurrentState => currentState;
    private void Awake()
    {
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

    public void SetState(GameState newState)
    {
        currentState = newState;
        Debug.Log($"[GameFlow] State -> {currentState}");

        // 상태 진입 시 UI/동작 세팅
        switch (currentState)
        {
            case GameState.Ready:
                if (player != null) player.SetSimulated(false); // ✅ Ready 정지
                if (launchGauge != null)
                {
                    launchGauge.SetVisible(true);
                    launchGauge.SetRunning(true);
                }
                if (runStats != null) runStats.BeginRun(); // ✅ 기준점 초기화
                break;

            case GameState.Flying:
                if (player != null) player.SetSimulated(true);  // ✅ Flying 물리 ON
                if (launchGauge != null)
                {
                    launchGauge.SetRunning(false);
                    launchGauge.SetVisible(false);
                }
                break;

            case GameState.GameOver:
                Debug.Log("[GameFlow] Game Over!");
                break;
        }
    }

    // Ready 상태에서 탭했을 때
    private void HandleTapReady()
    {
        if (currentState != GameState.Ready) return;

        float v01 = (launchGauge != null) ? launchGauge.Sample01() : 0f;
        Debug.Log($"[GameFlow] TapReady Sample01 = {v01:0.00}");

        SetState(GameState.Flying);          // 먼저 물리 ON
        if (player != null)
        {
            player.LaunchByGauge01(v01);
        }

        // 다음 챕터에서 여기서 Player.Launch(power) 호출할 예정
        SetState(GameState.Flying);
    }

    // Flying 상태에서 탭했을 때 (아직은 로그만)
    private void HandleTapFlying()
    {
        if (currentState != GameState.Flying) return;

        if (playerState != null)
            playerState.Toggle();
    }
    private void HandleTapAny()
    {
        if (currentState != GameState.GameOver) return;

        // Retry 실행
        if (player != null)
            player.ResetRun(cachedStartPos);

        // 2) 청크 리셋(시작부터 다시 생성)
        if (chunkSpawner != null)
            chunkSpawner.ResetChunks();

        if (movingTrapSpawner != null)
            movingTrapSpawner.ResetRun(); // ✅ 런 기준점 리셋

        // 3) Ready로 복귀(게이지 다시)
        SetState(GameState.Ready);
    }
    public void EnterGameOver()
    {
        SetState(GameState.GameOver);
    }
}
