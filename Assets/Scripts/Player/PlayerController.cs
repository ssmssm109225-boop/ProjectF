using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class PlayerController : MonoBehaviour
{
    [Header("Launch Settings")]
    [SerializeField] private float minPower = 8f;   // 최소 발사 파워
    [SerializeField] private float maxPower = 18f;  // 최대 발사 파워
    [SerializeField] private float launchAngleDeg = 35f; // 발사 각도(고정)

    [Header("Revive Launch Settings")]
    [SerializeField] private float reviveMinPower = 8f;   // 부활 최소 발사 파워
    [SerializeField] private float reviveMaxPower = 18f;  // 부활 최대 발사 파워

    [Header("Debug")]
    [SerializeField] private bool resetVelocityBeforeLaunch = true; // 부활 재발사 대비

    [Header("Forward Constraint")]
    [SerializeField] private bool forceForwardOnly = true; // 전방 진행 강제
    [SerializeField] private float minForwardSpeed = 0.5f; // 최소 전방 속도(0이면 완전 정지 가능)
    [SerializeField] private bool enableGroundDamping = true;  // 바닥 감속 사용
    [SerializeField] private float groundDampingPerSec = 0.08f; // 초당 감쇠량(약하게)
    [SerializeField] private float groundMaxSpeedClamp = 999f;  // 필요시 상한(기본 무제한)

    [Header("Trail (Speed Stages)")]
    [SerializeField] private TrailRenderer trail;
    [SerializeField] private PlayerStateController playerState;
    [SerializeField] private float trailStage2Speed = 4f;
    [SerializeField] private float trailStage3Speed = 8f;
    [SerializeField] private float trailStage1Time = 0.05f;
    [SerializeField] private float trailStage2Time = 0.15f;
    [SerializeField] private float trailStage3Time = 0.3f;
    [SerializeField] private float trailStage1Width = 0.08f;
    [SerializeField] private float trailStage2Width = 0.14f;
    [SerializeField] private float trailStage3Width = 0.22f;
    [SerializeField] private Color trailStage1Color = new Color(0.6f, 1f, 0.7f, 0.7f);
    [SerializeField] private Color trailStage2Color = new Color(0.2f, 1f, 0.4f, 0.85f);
    [SerializeField] private Color trailStage3Color = new Color(0.2f, 1f, 0.4f, 1f);
    [SerializeField] private float trailLerpSpeed = 6f;

    private Rigidbody2D rb;
    private Vector2 startPos;
    private int groundContactCount = 0;
    private int currentTrailStage = -1;
    private PlayerStateController.PlayerState cachedPlayerState = (PlayerStateController.PlayerState)(-1);
    private static Material trailMaterialCache;

    public Vector2 StartPos => startPos;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        startPos = transform.position;
        CacheTrailIfNeeded();
    }

    private void OnEnable()
    {
        if (playerState != null)
            playerState.OnStateChanged += HandleStateChanged;
    }

    private void OnDisable()
    {
        if (playerState != null)
            playerState.OnStateChanged -= HandleStateChanged;
    }

    private void Reset()
    {
        rb = GetComponent<Rigidbody2D>();
        CacheTrailIfNeeded();
    }

    private void OnValidate()
    {
        CacheTrailIfNeeded();
    }

    private void CacheTrailIfNeeded()
    {
        if (trail == null)
            trail = GetComponentInChildren<TrailRenderer>();
        if (playerState == null)
            playerState = GetComponent<PlayerStateController>();
        EnsureTrailMaterial();
    }

    private void EnsureTrailMaterial()
    {
        if (trail == null)
            return;

        if (trail.sharedMaterial != null && trail.sharedMaterial.shader != null)
        {
            if (trail.sharedMaterial.shader.name != "Standard")
                return;
        }

        if (trailMaterialCache == null)
        {
            Shader shader = Shader.Find("Sprites/Default");
            if (shader == null)
                shader = Shader.Find("Particles/Standard Unlit");

            if (shader == null)
                return;

            trailMaterialCache = new Material(shader);
            trailMaterialCache.name = "Trail_Unlit_Runtime";
            trailMaterialCache.hideFlags = HideFlags.DontSave;
        }

        trail.sharedMaterial = trailMaterialCache;
    }

    /// <summary>
    /// 게이지 값(0~1)을 받아 임펄스로 발사한다.
    /// </summary>
    public void LaunchByGauge01(float v01)
    {
        LaunchByGauge01Internal(v01, 1f, minPower, maxPower);
    }

    public void LaunchByGauge01WithMultiplier(float v01, float powerMultiplier)
    {
        LaunchByGauge01Internal(v01, powerMultiplier, minPower, maxPower);
    }

    public void LaunchByGauge01ReviveWithMultiplier(float v01, float powerMultiplier)
    {
        LaunchByGauge01Internal(v01, powerMultiplier, reviveMinPower, reviveMaxPower);
    }

    private void LaunchByGauge01Internal(float v01, float powerMultiplier, float minP, float maxP)
    {

        v01 = Mathf.Clamp01(v01);

        if (resetVelocityBeforeLaunch)
        {
            rb.linearVelocity = Vector2.zero; // Unity 6: linearVelocity 사용
            rb.angularVelocity = 0f;
        }

        float power = Mathf.Lerp(minP, maxP, v01) * powerMultiplier;

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

        UpdateTrailBySpeed();
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

    private void UpdateTrailBySpeed()
    {
        if (trail == null)
            return;

        float speed = rb.linearVelocity.magnitude;
        int stage;
        if (speed < trailStage2Speed) stage = 0;
        else if (speed < trailStage3Speed) stage = 1;
        else stage = 2;

        Color baseColor = GetBaseTrailColor();
        bool stateChanged = cachedPlayerState != (playerState != null ? playerState.CurrentState : cachedPlayerState);

        float targetTime = (stage == 0) ? trailStage1Time : (stage == 1 ? trailStage2Time : trailStage3Time);
        float targetWidth = (stage == 0) ? trailStage1Width : (stage == 1 ? trailStage2Width : trailStage3Width);
        float targetAlpha = (stage == 0) ? trailStage1Color.a : (stage == 1 ? trailStage2Color.a : trailStage3Color.a);

        trail.time = Mathf.MoveTowards(trail.time, targetTime, trailLerpSpeed * Time.fixedDeltaTime);
        trail.widthMultiplier = Mathf.MoveTowards(trail.widthMultiplier, targetWidth, trailLerpSpeed * Time.fixedDeltaTime);

        if (stage != currentTrailStage || stateChanged)
        {
            currentTrailStage = stage;
            cachedPlayerState = (playerState != null) ? playerState.CurrentState : cachedPlayerState;

            Color finalColor = new Color(baseColor.r, baseColor.g, baseColor.b, targetAlpha);
            trail.startColor = finalColor;
            trail.endColor = new Color(finalColor.r, finalColor.g, finalColor.b, 0f);
        }
    }

    private void HandleStateChanged(PlayerStateController.PlayerState newState, PlayerStateController.PlayerState prevState)
    {
        cachedPlayerState = newState;
        if (trail == null)
            return;

        Color baseColor = GetBaseTrailColor();
        float targetAlpha = (currentTrailStage == 0) ? trailStage1Color.a : (currentTrailStage == 1 ? trailStage2Color.a : trailStage3Color.a);
        Color finalColor = new Color(baseColor.r, baseColor.g, baseColor.b, targetAlpha);
        trail.startColor = finalColor;
        trail.endColor = new Color(finalColor.r, finalColor.g, finalColor.b, 0f);
    }

    private Color GetBaseTrailColor()
    {
        if (playerState != null)
            return playerState.GetColorForState(playerState.CurrentState);

        return trailStage2Color;
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
