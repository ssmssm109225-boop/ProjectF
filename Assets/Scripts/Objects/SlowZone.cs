using UnityEngine;

public class SlowZone : TriggerObjectBase
{
    [Header("Slow")]
    [Range(0.1f, 1f)]
    [SerializeField] private float xMultiplier = 0.6f; // x 속도 배율(0.6이면 40% 감속)

    [Range(0.1f, 1f)]
    [SerializeField] private float yMultiplier = 1.0f; // 기본은 y 유지

    protected override void OnHitGreen(PlayerController player)
    {
        player.MultiplyVelocity(xMultiplier, yMultiplier);
        Debug.Log($"[SlowZone] mul=({xMultiplier},{yMultiplier})");
    }
}
