using UnityEngine;

public class MovingTrap : MonoBehaviour
{
    [Header("Move")]
    [SerializeField] private float moveSpeed = 8f; // n: 좌측 이동 속도

    [Header("Lifetime")]
    [SerializeField] private float despawnXMargin = 30f; // 플레이어보다 충분히 뒤로 가면 제거

    private Transform player;

    public void Init(Transform playerRef, float speed)
    {
        player = playerRef;
        moveSpeed = speed;
    }

    private void Update()
    {
        // 3) 좌측으로 이동
        transform.Translate(Vector2.left * moveSpeed * Time.deltaTime);

        // 뒤로 한참 가면 디스폰(Despawn, 제거) - 성능/정리용
        if (player != null && transform.position.x < player.position.x - despawnXMargin)
        {
            Destroy(gameObject);
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        // 4) 플레이어와 트리거 시 Trap 처리
        if (!other.CompareTag("Player")) return;

        // 플레이어 정지
        var pc = other.GetComponent<PlayerController>();
        if (pc != null)
            pc.Freeze();

        // GameOver 전환
        var flow = FindObjectOfType<GameFlowManager>();
        if (flow != null)
            flow.SetState(GameFlowManager.GameState.GameOver);

        // 트랩은 즉시 제거(중복 트리거 방지)
        Destroy(gameObject);
    }
}
