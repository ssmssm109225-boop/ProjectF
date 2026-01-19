using UnityEngine;

[RequireComponent(typeof(SpriteRenderer))]
public class PlayerStateController : MonoBehaviour
{
    public enum PlayerState
    {
        Green, // 상호작용
        Red    // 통과
    }

    [Header("State")]
    [SerializeField] private PlayerState currentState = PlayerState.Green;
    public PlayerState CurrentState => currentState;

    [Header("Visual")]
    [SerializeField] private Color greenColor = new Color(0.2f, 1f, 0.4f, 1f);
    [SerializeField] private Color redColor = new Color(1f, 0.2f, 0.2f, 1f);

    [Header("Collision Toggle (Layer)")]
    [SerializeField] private string playerLayerName = "Player";
    [SerializeField] private string interactableLayerName = "Interactable";
    [SerializeField] private string trapLayerName = "Trap";

    private SpriteRenderer sr;

    // 레이어 인덱스 캐싱(성능/안정)
    private int playerLayer;
    private int interactableLayer;
    private int trapLayer;

    private void Awake()
    {
        sr = GetComponent<SpriteRenderer>();

        playerLayer = LayerMask.NameToLayer(playerLayerName);
        interactableLayer = LayerMask.NameToLayer(interactableLayerName);
        trapLayer = LayerMask.NameToLayer(trapLayerName);

        // 레이어가 없으면 -1이 나옴. 프로토 단계에서 실수 잡기용 로그.
        if (playerLayer < 0) Debug.LogError($"[PlayerState] Layer not found: {playerLayerName}");
        if (interactableLayer < 0) Debug.LogError($"[PlayerState] Layer not found: {interactableLayerName}");
        if (trapLayer < 0) Debug.LogError($"[PlayerState] Layer not found: {trapLayerName}");

        ApplyVisual();
        ApplyCollisionRules(); // 시작 상태 반영
    }

    public void Toggle()
    {
        currentState = (currentState == PlayerState.Green) ? PlayerState.Red : PlayerState.Green;
        ApplyVisual();
        ApplyCollisionRules();
        Debug.Log($"[PlayerState] -> {currentState}");
    }

    private void ApplyVisual()
    {
        if (sr == null) return;
        sr.color = (currentState == PlayerState.Green) ? greenColor : redColor;
    }

    /// <summary>
    /// 상태에 따라 Player vs (Interactable/Trap) 충돌을 On/Off 한다.
    /// Ground는 건드리지 않는다.
    /// </summary>
    private void ApplyCollisionRules()
    {
        if (playerLayer < 0) return;

        bool shouldIgnore = (currentState == PlayerState.Red);

        // Red면 통과: 충돌 무시(true)
        // Green이면 상호작용: 충돌 허용(false)
        if (interactableLayer >= 0)
            Physics2D.IgnoreLayerCollision(playerLayer, interactableLayer, shouldIgnore);

        if (trapLayer >= 0)
            Physics2D.IgnoreLayerCollision(playerLayer, trapLayer, shouldIgnore);
    }
}
