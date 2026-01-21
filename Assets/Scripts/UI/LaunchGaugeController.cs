using UnityEngine;
using UnityEngine.UI;

public class LaunchGaugeController : MonoBehaviour
{
    [Header("UI")]
    [SerializeField] private Slider gaugeSlider; // 게이지 슬라이더

    [Header("Gauge Settings")]
    [SerializeField, Range(0.1f, 5f)] private float gaugeSpeed = 1.5f; // 왕복 속도
    [SerializeField] private bool isRunning = true; // Ready에서만 true로 켤 예정

    // 내부 상태
    private float value = 0f;   // 0~1
    private int dir = 1;        // +1 / -1

    public float CurrentValue01 => value;

    private void Awake()
    {
        if (gaugeSlider == null)
            gaugeSlider = GetComponentInChildren<Slider>();

        // 안전장치
        if (gaugeSlider != null)
        {
            gaugeSlider.minValue = 0f;
            gaugeSlider.maxValue = 1f;
            gaugeSlider.value = 0f;
        }
    }

    private void Update()
    {
        if (!isRunning) return;

        // 왕복(Ping-Pong) 게이지 값 업데이트
        value += dir * gaugeSpeed * Time.deltaTime;

        if (value >= 1f)
        {
            value = 1f;
            dir = -1;
        }
        else if (value <= 0f)
        {
            value = 0f;
            dir = 1;
        }

        if (gaugeSlider != null)
            gaugeSlider.value = value;
    }

    /// <summary>
    /// 현재 게이지 값을 샘플링한다(0~1).
    /// </summary>
    public float Sample01()
    {
        return value;
    }

    /// <summary>
    /// 게이지 동작 On/Off
    /// </summary>
    public void SetRunning(bool running)
    {
        isRunning = running;
    }

    public void ResetGauge()
    {
        value = 0f;
        dir = 1;
        if (gaugeSlider != null)
            gaugeSlider.value = value;
    }

    public void SetVisible(bool visible)
    {
        gameObject.SetActive(visible);
    }
}
