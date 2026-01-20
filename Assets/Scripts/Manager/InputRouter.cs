using UnityEngine;
using System;
using UnityEngine.EventSystems;

public class InputRouter : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private GameFlowManager gameFlow; // 게임 상태 참조

    // 상태별 탭 이벤트 (다음 챕터에서 구독해서 사용)
    public event Action OnTapReady;
    public event Action OnTapFlying;
    public event Action OnTapAny;

    private void Update()
    {
        // WebGL/모바일 대응: 마우스 클릭을 터치처럼 사용
        // (모바일 브라우저에서도 보통 잘 동작)
        if (Input.GetMouseButtonDown(0))
        {
            RouteTap();
        }
    }
    private bool IsPointerOverUI()
    {
        if (EventSystem.current == null) return false;

        // 마우스/에디터
        if (EventSystem.current.IsPointerOverGameObject())
            return true;

        // 터치(모바일)
        if (Input.touchCount > 0)
        {
            var t = Input.GetTouch(0);
            if (EventSystem.current.IsPointerOverGameObject(t.fingerId))
                return true;
        }
        return false;
    }

    private void RouteTap()
    {
        if (IsPointerOverUI()) return; // ✅ UI 버튼 누를 땐 게임 입력 무시

        // 1) 탭 순간 상태 캐싱
        var stateAtTap = gameFlow.CurrentState;

        // ✅ GameOver 상태에서는 UI 버튼만 사용하도록 제한
        if (stateAtTap == GameFlowManager.GameState.GameOver)
        {
            Debug.Log("[InputRouter] Tap ignored in GameOver (use UI buttons)");
            return;
        }

        // 2) Ready/Flying 상태에서만 탭 처리
        switch (stateAtTap)
        {
            case GameFlowManager.GameState.Ready:
                Debug.Log("[InputRouter] Tap -> Ready");
                OnTapReady?.Invoke();
                break;

            case GameFlowManager.GameState.Flying:
                Debug.Log("[InputRouter] Tap -> Flying");
                OnTapFlying?.Invoke();
                break;

            default:
                Debug.Log($"[InputRouter] Tap ignored in state: {stateAtTap}");
                break;
        }
    }

}
