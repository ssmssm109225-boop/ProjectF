using UnityEngine;

public class Trap : TriggerObjectBase
{
    [Header("Trap")]
    [SerializeField] private bool freezePlayerOnHit = true; // true면 rb.simulated=false까지

    protected override void OnHitGreen(PlayerController player)
    {
        // 1) 플레이어 정지
        if (freezePlayerOnHit)
        {
            player.Freeze();
            // 선택: 완전 정지(물리 OFF)
            var rb = player.GetComponent<Rigidbody2D>();
            if (rb != null) rb.simulated = false;
        }
        else
        {
            player.Freeze();
        }

        // 2) 게임오버
        var flow = FindObjectOfType<GameFlowManager>();
        if (flow != null)
            flow.EnterGameOver();

        Debug.Log("[Trap] Hit -> GameOver");
    }
}
