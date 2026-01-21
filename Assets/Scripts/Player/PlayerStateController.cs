using UnityEngine;
using System;

[RequireComponent(typeof(SpriteRenderer))]
public class PlayerStateController : MonoBehaviour
{
    public enum PlayerState
    {
        Green, // 於╇弻 ?愳爼 O
        Red,   // ?店臣
        Yellow // 부활 무적
    }

    // ???來儨 氤€瓴??措菠?? Red->Green, Green->Red ?勴櫂 臧愳?
    public event Action<PlayerState, PlayerState> OnStateChanged;

    [Header("State")]
    [SerializeField] private PlayerState currentState = PlayerState.Green;
    public PlayerState CurrentState => currentState;

    [Header("Visual")]
    [SerializeField] private Color greenColor = new Color(0.2f, 1f, 0.4f, 1f);
    [SerializeField] private Color redColor = new Color(1f, 0.2f, 0.2f, 1f);
    [SerializeField] private Color yellowColor = new Color(1f, 0.9f, 0.2f, 1f);

    [Header("Collision Toggle (Layer)")]
    [SerializeField] private string playerLayerName = "Player";
    [SerializeField] private string interactableLayerName = "Interactable";
    [SerializeField] private string trapLayerName = "Trap";

    private SpriteRenderer sr;

    // 锟斤拷锟教撅拷 锟轿碉拷锟斤拷 某锟斤拷(锟斤拷锟斤拷/锟斤拷锟斤拷)
    private int playerLayer;
    private int interactableLayer;
    private int trapLayer;

    private void Awake()
    {
        sr = GetComponent<SpriteRenderer>();

        playerLayer = LayerMask.NameToLayer(playerLayerName);
        interactableLayer = LayerMask.NameToLayer(interactableLayerName);
        trapLayer = LayerMask.NameToLayer(trapLayerName);

        // 锟斤拷锟教绢啊 锟斤拷锟斤拷锟斤拷 -1锟斤拷 锟斤拷锟斤拷. 锟斤拷锟斤拷锟斤拷 锟杰拌俊锟斤拷 锟角硷拷 锟斤拷锟斤拷 锟轿憋拷.
        if (playerLayer < 0) Debug.LogError($"[PlayerState] Layer not found: {playerLayerName}");
        if (interactableLayer < 0) Debug.LogError($"[PlayerState] Layer not found: {interactableLayerName}");
        if (trapLayer < 0) Debug.LogError($"[PlayerState] Layer not found: {trapLayerName}");

        ApplyVisual();
        ApplyCollisionRules(); // 锟斤拷锟斤拷 锟斤拷锟斤拷 锟捷匡拷
    }

    public void Toggle()
    {
        SetState((currentState == PlayerState.Green) ? PlayerState.Red : PlayerState.Green);
    }

    public void SetState(PlayerState newState)
    {
        if (currentState == newState)
            return;

        PlayerState prevState = currentState;
        currentState = (currentState == PlayerState.Green) ? PlayerState.Red : PlayerState.Green;

        currentState = newState;

        ApplyVisual();
        ApplyCollisionRules();


        // ???來儨 氤€瓴??措菠??氚滌儩
        OnStateChanged?.Invoke(currentState, prevState);


        Debug.Log($"[PlayerState] {prevState} -> {currentState}");
    }

    private void ApplyVisual()
    {
        if (sr == null) return;
        switch (currentState)
        {
            case PlayerState.Green:
                sr.color = greenColor;
                break;
            case PlayerState.Yellow:
                sr.color = yellowColor;
                break;
            default:
                sr.color = redColor;
                break;
        }
    }

    /// <summary>
    /// 锟斤拷锟铰匡拷 锟斤拷锟斤拷 Player vs (Interactable/Trap) 锟芥倒锟斤拷 On/Off 锟窖达拷.
    /// Ground锟斤拷 锟角靛府锟斤拷 锟绞绰达拷.
    /// </summary>
    private void ApplyCollisionRules()
    {
        if (playerLayer < 0) return;

        bool shouldIgnore = (currentState == PlayerState.Red || currentState == PlayerState.Yellow);

        // Red锟斤拷 锟斤拷锟? 锟芥倒 锟斤拷锟斤拷(true)
        // Green锟教革拷 锟斤拷龋锟桔匡拷: 锟芥倒 锟斤拷锟?false)
        if (interactableLayer >= 0)
            Physics2D.IgnoreLayerCollision(playerLayer, interactableLayer, shouldIgnore);

        if (trapLayer >= 0)
            Physics2D.IgnoreLayerCollision(playerLayer, trapLayer, shouldIgnore);
    }
    public void ResetToGreen()
    {
        if (currentState == PlayerState.Green)
        {
            ApplyVisual();
            ApplyCollisionRules();
            return;
        }

        PlayerState prevState = currentState;
        currentState = PlayerState.Green;

        ApplyVisual();
        ApplyCollisionRules();

        OnStateChanged?.Invoke(currentState, prevState);
        Debug.Log($"[PlayerState] {prevState} -> {currentState} (reset)");
    }

    public Color GetColorForState(PlayerState state)
    {
        switch (state)
        {
            case PlayerState.Green:
                return greenColor;
            case PlayerState.Yellow:
                return yellowColor;
            default:
                return redColor;
        }
    }
}
