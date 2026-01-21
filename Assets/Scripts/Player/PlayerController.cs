using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class PlayerController : MonoBehaviour
{
    [Header("Launch Settings")]
    [SerializeField] private float minPower = 8f;   // 최소 발사 파워
    [SerializeField] private float maxPower = 18f;  // 최대 발사 파워
    [SerializeField] private float launchAngleDeg = 35f; // 발사 각도(고정)

    [Header("Debug")]
    [SerializeField] private bool resetVelocityBeforeLaunch = true; // 부활 재발사 대비

    [Header("Forward Constraint")]
    [SerializeField] private bool forceForwardOnly = true; // 전방 진행 강제
    [SerializeField] private float minForwardSpeed = 0.5f; // 최소 전방 속도(0이면 완전 정지 가능)
    [SerializeField] private bool enableGroundDamping = true;  // 바닥 감속 사용
    [SerializeField] private float groundDampingPerSec = 0.08f; // 초당 감쇠량(약하게)
    [SerializeField] private float groundMaxSpeedClamp = 999f;  // 필요시 상한(기본 무제한)

    private Rigidbody2D rb;
    private Vector2 startPos;
    private int groundContactCount = 0;

    public Vector2 StartPos => startPos;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        startPos = transform.position;
    }

    /// <summary>
    /// 게이지 값(0~1)을 받아 임펄스로 발사한다.
    /// </summary>
    public void LaunchByGauge01(float v01)
    {
        LaunchByGauge01Internal(v01, 1f);
    }

    public void LaunchByGauge01WithMultiplier(float v01, float powerMultiplier)
    {
        LaunchByGauge01Internal(v01, powerMultiplier);
    }

    private void LaunchByGauge01Internal(float v01, float powerMultiplier)
    {

        v01 = Mathf.Clamp01(v01);

        if (resetVelocityBeforeLaunch)
        {
            rb.linearVelocity = Vector2.zero; // Unity 6: linearVelocity 사용
            rb.angularVelocity = 0f;
        }

        float power = Mathf.Lerp(minPower, maxPower, v01) * powerMultiplier;

        float rad = launchAngleDeg * Mathf.Deg2Rad;
        Vector2 dir = new Vector2(Mathf.Cos(rad), Mathf.Sin(rad)).normalized;

        rb.AddForce(dir * power, ForceMode2D.Impulse);
    }

    public float GetSpeed()
    {
        return rb.linearVelocity.magnitude;
    }

    public void SetSimulated(bool simulated)
    {
        rb.simulated = simulated; // ✅ Unity 6에서도 안정적
        if (!simulated)
        {
            rb.linearVelocity = Vector2.zero;
            rb.angularVelocity = 0f;
        }
    }

    /// <summary>
    /// 외부에서 임펄스를 추가로 준다(가속/상승 등 오브젝트용)
    /// </summary>
    public void AddImpulse(Vector2 impulse)
    {
        rb.AddForce(impulse, ForceMode2D.Impulse);
    }

    private void FixedUpdate()
    {
        // (기존) 전방 진행 강제
        if (forceForwardOnly)
        {
            Vector2 v = rb.linearVelocity;
            if (v.x < minForwardSpeed)
            {
                v.x = minForwardSpeed;
                rb.linearVelocity = v;
            }
        }

        // ✅ (추가) 바닥 접지 중 약한 감속
        if (enableGroundDamping && groundContactCount > 0)
        {
            Vector2 v = rb.linearVelocity;

            // 초당 감쇠를 고정 스텝에 맞춰 적용 (지수 감쇠 느낌)
            float mul = Mathf.Clamp01(1f - groundDampingPerSec * Time.fixedDeltaTime);

            v.x *= mul;

            // 필요하면 y도 살짝 줄일 수 있지만, 일단 x만 추천
            // v.y *= mul * 0.2f;

            // 상한 클램프(선택)
            if (groundMaxSpeedClamp < 999f)
                v.x = Mathf.Min(v.x, groundMaxSpeedClamp);

            rb.linearVelocity = v;
        }
    }
    public void Freeze()
    {
        rb.simulated = false;
        rb.linearVelocity = Vector2.zero;
        rb.angularVelocity = 0f;
    }
    public void ResetRun(Vector2 startPosition)
    {
        // 물리 다시 켜기
        rb.simulated = true;

        // 위치 초기화
        transform.position = startPosition;

        // 속도/회전 초기화
        rb.linearVelocity = Vector2.zero;
        rb.angularVelocity = 0f;
    }
    public void MultiplyVelocity(float xMul, float yMul)
    {
        Vector2 v = rb.linearVelocity;
        v.x *= xMul;
        v.y *= yMul;
        rb.linearVelocity = v;
    }
    public void ReorientVelocityToAngle(float angleDeg, float minSpeed = 0f, float extraSpeed = 0f)
    {
        // 현재 속도 크기
        float curSpeed = rb.linearVelocity.magnitude;

        // 최소 속도 보장 + 추가 속도 보너스
        float targetSpeed = Mathf.Max(curSpeed, minSpeed) + extraSpeed;

        // 각도 -> 방향 벡터
        float rad = angleDeg * Mathf.Deg2Rad;
        Vector2 dir = new Vector2(Mathf.Cos(rad), Mathf.Sin(rad)).normalized;

        // ✅ 우측 진행 강제 (혹시나 cos가 음수가 되는 각도 방지)
        if (dir.x < 0f) dir.x = -dir.x;

        rb.linearVelocity = dir * targetSpeed;
    }
    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.collider.CompareTag("Ground"))
            groundContactCount++;
    }

    private void OnCollisionExit2D(Collision2D collision)
    {
        if (collision.collider.CompareTag("Ground"))
            groundContactCount = Mathf.Max(0, groundContactCount - 1);
    }
}
