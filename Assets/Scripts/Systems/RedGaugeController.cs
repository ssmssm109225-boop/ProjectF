using UnityEngine;

/// <summary>
/// Red 상태 지속 제한 게이지 시스템 (3초 스태미나)
/// 
/// 규칙:
/// - maxTime: 3초 (최대 게이지)
/// - Red 상태: 초당 drainPerSec만큼 감소
/// - Green 상태: 초당 recoverPerSec만큼 회복
/// - Red→Green 전환 순간: 즉시 snapRecoverSeconds(1초)치 회복 (1회만)
/// - Flying 상태에서만 동작
/// - timeLeft <= 0이면 즉시 GameOver
/// </summary>
public class RedGaugeController : MonoBehaviour
{
    [Header("Gauge Settings")]
    [SerializeField] private float maxTime = 3f;              // 최대 게이지 시간 (초)
    [SerializeField] private float drainPerSec = 1f;          // Red 상태일 때 초당 감소량
    [SerializeField] private float recoverPerSec = 0.5f;      // Green 상태일 때 초당 회복량
    [SerializeField] private float snapRecoverSeconds = 1f;   // Red→Green 전환 시 즉시 회복량 (초)

    [Header("References")]
    [SerializeField] private PlayerStateController playerState;
    [SerializeField] private GameFlowManager gameFlow;

    // 현재 게이지 값
    private float timeLeft;

    // Red→Green 전환 시 스냅 회복 적용 여부 (중복 방지)
    private bool snapRecoveryApplied = false;
    private PlayerStateController.PlayerState prevState;

    public float MaxTime => maxTime;
    public float TimeLeft => timeLeft;

    /// <summary>
    /// 게이지를 정규화된 값(0~1)으로 반환
    /// UI에서 FillAmount나 Slider 값으로 사용
    /// </summary>
    public float Normalized01 => Mathf.Clamp01(timeLeft / maxTime);

    private void OnEnable()
    {
        // PlayerStateController 이벤트 구독
        if (playerState != null)
        {
            playerState.OnStateChanged += HandleStateChanged;
        }
    }

    private void OnDisable()
    {
        // 이벤트 구독 해제 (중복 구독 방지)
        if (playerState != null)
        {
            playerState.OnStateChanged -= HandleStateChanged;
        }
    }

    private void Start()
    {
        // 게이지 초기화 (최대값)
        ResetGauge();

        if (playerState != null)
            prevState = playerState.CurrentState;
    }

    /// <summary>
    /// 게이지를 최대값으로 리셋
    /// Ready 상태로 돌아올 때 호출
    /// </summary>
    public void ResetGauge()
    {
        timeLeft = maxTime;
        snapRecoveryApplied = false;
        Debug.Log("[RedGauge] Gauge reset to max: " + maxTime);
    }

    /// <summary>
    /// ✅ GameOver 상태 진입 시: 게이지 계산 중단
    /// </summary>
    public void OnGameOverState()
    {
        // 게이지 계산을 멈춤 (Update에서 gameFlow.CurrentState 체크하므로 자동 중단)
        Debug.Log("[RedGauge] GameOver state - gauge calculation stopped");
    }

    /// <summary>
    /// ✅ Ready 상태로 진입 시: 게이지 초기화
    /// </summary>
    public void OnReadyState()
    {
        ResetGauge();
    }

    private void Update()
    {
        // Flying 상태에서만 게이지 동작
        if (gameFlow == null || gameFlow.CurrentState != GameFlowManager.GameState.Flying)
            return;

        // 현재 상태 확인
        if (playerState == null)
            return;

        var currentState = playerState.CurrentState;

        // Red 상태: 감소
        if (currentState == PlayerStateController.PlayerState.Red)
        {
            timeLeft -= drainPerSec * Time.deltaTime;

            // 게이지가 0 이하가 되면 GameOver
            if (timeLeft <= 0f)
            {
                timeLeft = 0f;
                Debug.Log("[RedGauge] Time expired! Entering GameOver");
                
                if (gameFlow != null)
                    gameFlow.EnterGameOver();
            }
        }
        // Green 상태: 회복
        else if (currentState == PlayerStateController.PlayerState.Green)
        {
            timeLeft += recoverPerSec * Time.deltaTime;

            // 최대값을 넘지 않도록 클램프
            if (timeLeft > maxTime)
                timeLeft = maxTime;
        }
    }

    /// <summary>
    /// PlayerStateController의 상태 변경 이벤트 핸들러
    /// Red→Green 전환 순간에 스냅 회복 1회 적용
    /// </summary>
    private void HandleStateChanged(PlayerStateController.PlayerState newState, PlayerStateController.PlayerState prevState)
    {
        // Red → Green 전환 시에만 스냅 회복 적용
        if (prevState == PlayerStateController.PlayerState.Red && 
            newState == PlayerStateController.PlayerState.Green)
        {
            if (!snapRecoveryApplied)
            {
                // 스냅 회복: recoverPerSec * snapRecoverSeconds만큼 즉시 회복
                float snapRecoverAmount = recoverPerSec * snapRecoverSeconds;
                timeLeft += snapRecoverAmount;

                // 최대값을 넘지 않도록 클램프
                if (timeLeft > maxTime)
                    timeLeft = maxTime;

                snapRecoveryApplied = true;
                Debug.Log("[RedGauge] Snap recovery applied! +" + snapRecoverAmount.ToString("F2") + "s, total: " + timeLeft.ToString("F2"));
            }
        }
        // Green → Red 전환 시 스냅 회복 플래그 리셋
        else if (prevState == PlayerStateController.PlayerState.Green && 
                 newState == PlayerStateController.PlayerState.Red)
        {
            snapRecoveryApplied = false;
            Debug.Log("[RedGauge] Entered Red state. Snap recovery reset for next Green transition");
        }

        this.prevState = newState;
    }
}
