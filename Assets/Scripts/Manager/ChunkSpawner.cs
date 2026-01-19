using System.Collections.Generic;
using UnityEngine;

public class ChunkSpawner : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Transform player;
    [SerializeField] private Transform chunkRoot;

    [Header("Chunk Pools")]
    [SerializeField] private List<GameObject> onboardingChunks = new(); // 고정 순서
    [SerializeField] private List<GameObject> easyChunks = new();       // 랜덤(초중반)
    [SerializeField] private List<GameObject> hardChunks = new();       // 랜덤(후반, 선택)

    [Header("Spawn Settings")]
    [SerializeField] private float chunkLength = 25f;      // ✅ 청크 길이
    [SerializeField] private float startSpawnX = -25f;     // 시작 겹침 방지
    [SerializeField] private float chunkOriginY = -3f;     // 전체 Y 오프셋
    [SerializeField] private int initialChunks = 7;        // 시작 깔기
    [SerializeField] private int keepChunksBehind = 3;     // 뒤 유지
    [SerializeField] private float spawnAheadOffset = 45f; // pop-in 방지(25 기준 1.8 청크)

    [Header("Tier Switch")]
    [SerializeField] private float onboardingDistance = 100f; // 온보딩 끝나는 거리
    [SerializeField] private float hardTierDistance = 250f;   // 하드 풀 전환 거리(선택)

    private float nextSpawnX;
    private readonly Queue<GameObject> spawned = new();

    private Vector2 runStartPos;

    private void Awake()
    {
        if (chunkRoot == null) chunkRoot = transform;

        if (player == null)
        {
            var p = GameObject.FindWithTag("Player");
            if (p != null) player = p.transform;
        }
    }

    private void Start()
    {
        nextSpawnX = startSpawnX;

        if (player != null)
            runStartPos = player.position;

        for (int i = 0; i < initialChunks; i++)
            SpawnNextChunk();
    }

    private void Update()
    {
        if (player == null) return;

        if (player.position.x + spawnAheadOffset >= nextSpawnX)
        {
            SpawnNextChunk();
            CleanupOldChunks();
        }
    }

    private void SpawnNextChunk()
    {
        GameObject prefab = PickChunkPrefab();
        if (prefab == null) return;

        Vector3 pos = new Vector3(nextSpawnX, chunkOriginY, 0f);
        GameObject go = Instantiate(prefab, pos, Quaternion.identity, chunkRoot);
        spawned.Enqueue(go);

        nextSpawnX += chunkLength;
    }

    private GameObject PickChunkPrefab()
    {
        float dist = (player != null) ? (player.position.x - runStartPos.x) : 0f;

        // 1) 온보딩: 고정 순서로 뽑고, 다 쓰면 easy로
        if (dist < onboardingDistance && onboardingChunks.Count > 0)
        {
            int idx = spawned.Count < onboardingChunks.Count ? spawned.Count : onboardingChunks.Count - 1;
            return onboardingChunks[idx];
        }

        // 2) 하드 티어(선택)
        if (dist >= hardTierDistance && hardChunks.Count > 0)
        {
            return hardChunks[Random.Range(0, hardChunks.Count)];
        }

        // 3) 기본 easy
        if (easyChunks.Count > 0)
        {
            return easyChunks[Random.Range(0, easyChunks.Count)];
        }

        // 안전망
        if (onboardingChunks.Count > 0)
            return onboardingChunks[Random.Range(0, onboardingChunks.Count)];

        return null;
    }

    private void CleanupOldChunks()
    {
        int maxKeep = keepChunksBehind + 4; // 완충 버퍼

        while (spawned.Count > maxKeep)
        {
            var old = spawned.Dequeue();
            if (old != null) Destroy(old);
        }
    }

    /// <summary>
    /// 런 재시작 시(리트라이) 호출: 청크 리셋
    /// </summary>
    public void ResetChunks()
    {
        // 1) 기존 청크 전부 제거
        while (spawned.Count > 0)
        {
            var go = spawned.Dequeue();
            if (go != null) Destroy(go);
        }

        // 2) 스폰 좌표 초기화
        nextSpawnX = startSpawnX;

        // 3) 거리 기준점(온보딩 거리 계산용) 초기화
        if (player != null)
            runStartPos = player.position;

        // 4) 초기 청크 다시 깔기
        for (int i = 0; i < initialChunks; i++)
            SpawnNextChunk();
    }
}
