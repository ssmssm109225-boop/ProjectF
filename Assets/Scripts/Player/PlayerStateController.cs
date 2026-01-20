using UnityEngine;
using System;

[RequireComponent(typeof(SpriteRenderer))]
public class PlayerStateController : MonoBehaviour
{
    public enum PlayerState
    {
        Green, // 충돌 판정 O
        Red    // 통과
    }

    // ✅ 상태 변경 이벤트: Red->Green, Green->Red 전환 감지
    public event Action<PlayerState, PlayerState> OnStateChanged;

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

    // ���̾� �ε��� ĳ��(����/����)
    private int playerLayer;
    private int interactableLayer;
    private int trapLayer;

    private void Awake()
    {
        sr = GetComponent<SpriteRenderer>();

        playerLayer = LayerMask.NameToLayer(playerLayerName);
        interactableLayer = LayerMask.NameToLayer(interactableLayerName);
        trapLayer = LayerMask.NameToLayer(trapLayerName);

        // ���̾ ������ -1�� ����. ������ �ܰ迡�� �Ǽ� ���� �α�.
        if (playerLayer < 0) Debug.LogError($"[PlayerState] Layer not found: {playerLayerName}");
        if (interactableLayer < 0) Debug.LogError($"[PlayerState] Layer not found: {interactableLayerName}");
        if (trapLayer < 0) Debug.LogError($"[PlayerState] Layer not found: {trapLayerName}");

        ApplyVisual();
        ApplyCollisionRules(); // ���� ���� �ݿ�
    }

    public void Toggle()
    {
        PlayerState prevState = currentState;
        currentState = (currentState == PlayerState.Green) ? PlayerState.Red : PlayerState.Green;
        
        ApplyVisual();
        ApplyCollisionRules();
        
        // ✅ 상태 변경 이벤트 발생
        OnStateChanged?.Invoke(currentState, prevState);
        
        Debug.Log($"[PlayerState] {prevState} -> {currentState}");
    }

    private void ApplyVisual()
    {
        if (sr == null) return;
        sr.color = (currentState == PlayerState.Green) ? greenColor : redColor;
    }

    /// <summary>
    /// ���¿� ���� Player vs (Interactable/Trap) �浹�� On/Off �Ѵ�.
    /// Ground�� �ǵ帮�� �ʴ´�.
    /// </summary>
    private void ApplyCollisionRules()
    {
        if (playerLayer < 0) return;

        bool shouldIgnore = (currentState == PlayerState.Red);

        // Red�� ���: �浹 ����(true)
        // Green�̸� ��ȣ�ۿ�: �浹 ���(false)
        if (interactableLayer >= 0)
            Physics2D.IgnoreLayerCollision(playerLayer, interactableLayer, shouldIgnore);

        if (trapLayer >= 0)
            Physics2D.IgnoreLayerCollision(playerLayer, trapLayer, shouldIgnore);
    }
}
