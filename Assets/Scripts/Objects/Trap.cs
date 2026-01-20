using UnityEngine;

public class Trap : TriggerObjectBase
{
    [Header("Trap")]
    [SerializeField] private bool freezePlayerOnHit = true;

    protected override void OnHitGreen(PlayerController player)
    {
        // 중복 충돌 방지: 이미 GameOver 상태면 무시
        if (GameFlowManager.Instance != null && 
            GameFlowManager.Instance.CurrentState == GameFlowManager.GameState.GameOver)
        {
            Debug.Log("[Trap] Already in GameOver state, ignoring collision");
            return;
        }

        // 1) 플레이어 동결
        if (freezePlayerOnHit)
        {
            player.Freeze();
            var rb = player.GetComponent<Rigidbody2D>();
            if (rb != null) rb.simulated = false;
        }
        else
        {
            player.Freeze();
        }

        // 2) GameOver 진입 (싱글톤 사용)
        if (GameFlowManager.Instance != null)
        {
            Debug.Log("[Trap] Entering GameOver via Instance");
            GameFlowManager.Instance.EnterGameOver();
        }
        else
        {
            Debug.LogError("[Trap] GameFlowManager.Instance is NULL!");
        }

        Debug.Log("[Trap] Hit -> GameOver");
    }
}
