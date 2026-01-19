using UnityEngine;

public class BoostPad : TriggerObjectBase
{
    [Header("Boost")]
    [SerializeField] private float impulseX = 6f;
    [SerializeField] private float impulseY = 0f;

    protected override void OnHitGreen(PlayerController player)
    {
        player.AddImpulse(new Vector2(impulseX, impulseY));
        Debug.Log($"[BoostPad] impulse=({impulseX},{impulseY})");
    }
}
