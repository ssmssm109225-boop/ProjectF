using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Red 게이지 UI 표현 (상시 표시)
/// 
/// 위치: 상단 중앙 (Top-Center)
/// 형태: 가로 Progress Bar (Image.fillAmount 또는 Slider)
/// 색상: Green 상태 = 초록색, Red 상태 = 빨강색
/// </summary>
public class RedGaugeUI : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private RedGaugeController redGauge;
    [SerializeField] private PlayerStateController playerState;

    [Header("UI - Fill Image")]
    [SerializeField] private Image gaugeBar;  // Progress bar (fillAmount 방식)

    [Header("Colors")]
    [SerializeField] private Color greenColor = new Color(0.2f, 0.8f, 0.2f);  // 초록색
    [SerializeField] private Color redColor = new Color(0.9f, 0.2f, 0.2f);    // 빨강색

    [Header("Blink Settings (Optional)")]
    [SerializeField] private bool enableBlinkInRed = false;      // Red 상태에서 깜빡임
    [SerializeField] private float blinkSpeed = 0.5f;            // 깜빡임 속도

    private bool isBlinking = false;

    private void OnEnable()
    {
        // PlayerState 변경 이벤트 구독
        if (playerState != null)
        {
            playerState.OnStateChanged += UpdateColor;
        }
    }

    private void OnDisable()
    {
        // 이벤트 구독 해제
        if (playerState != null)
        {
            playerState.OnStateChanged -= UpdateColor;
        }
    }

    private void Start()
    {
        // 초기 설정
        if (gaugeBar == null)
        {
            Debug.LogError("[RedGaugeUI] gaugeBar Image not assigned!");
            return;
        }

        if (redGauge == null)
        {
            redGauge = FindObjectOfType<RedGaugeController>();
        }

        if (playerState == null)
        {
            playerState = FindObjectOfType<PlayerStateController>();
        }

        // 초기 색상 설정
        if (playerState != null)
            UpdateColor(playerState.CurrentState, playerState.CurrentState);
    }

    private void Update()
    {
        // 게이지 바 업데이트 (상시 표시)
        if (redGauge != null && gaugeBar != null)
        {
            gaugeBar.fillAmount = redGauge.Normalized01;
        }

        // Red 상태에서 깜빡임 (선택)
        if (enableBlinkInRed && isBlinking && gaugeBar != null)
        {
            HandleBlink();
        }
    }

    /// <summary>
    /// PlayerState 변경에 따른 색상 업데이트
    /// </summary>
    private void UpdateColor(PlayerStateController.PlayerState newState, PlayerStateController.PlayerState prevState)
    {
        if (gaugeBar == null)
            return;

        if (newState == PlayerStateController.PlayerState.Green)
        {
            gaugeBar.color = greenColor;
            isBlinking = false;
            Debug.Log("[RedGaugeUI] Gauge color -> Green");
        }
        else if (newState == PlayerStateController.PlayerState.Red)
        {
            gaugeBar.color = redColor;
            isBlinking = enableBlinkInRed;
            Debug.Log("[RedGaugeUI] Gauge color -> Red");
        }
    }

    /// <summary>
    /// Red 상태에서 깜빡임 효과 (알파값 변조)
    /// </summary>
    private void HandleBlink()
    {
        float blink = Mathf.Sin(Time.time * blinkSpeed * Mathf.PI) * 0.5f + 0.5f;  // 0~1 사이의 사인파
        Color c = gaugeBar.color;
        c.a = blink;
        gaugeBar.color = c;
    }
}
