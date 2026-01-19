using UnityEngine;

[RequireComponent(typeof(Collider2D))]
public abstract class TriggerObjectBase : MonoBehaviour
{
    protected virtual void Reset()
    {
        // ✅ 모든 오브젝트를 트리거로 통일
        var col = GetComponent<Collider2D>();
        col.isTrigger = true;
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        // Player만 처리
        if (!other.CompareTag("Player")) return;

        var player = other.GetComponent<PlayerController>();
        if (player == null) return;

        // Green에서만 상호작용 (Red는 효과 없음)
        var state = other.GetComponent<PlayerStateController>();
        if (state != null && state.CurrentState != PlayerStateController.PlayerState.Green)
            return;

        OnHitGreen(player);
    }

    protected abstract void OnHitGreen(PlayerController player);
}
