using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class GameOverPanelUI : MonoBehaviour
{
    [Header("Refs")]
    [SerializeField] private GameFlowManager gameFlow;
    [SerializeField] private RunStatsManager runStats;

    [Header("UI")]
    [SerializeField] private GameObject panel;
    [SerializeField] private TMP_Text txtDistance;
    [SerializeField] private TMP_Text txtBest;
    [SerializeField] private Button btnRevive;
    [SerializeField] private Button btnRetry;

    private void Awake()
    {
        if (gameFlow == null) gameFlow = FindObjectOfType<GameFlowManager>();
        if (runStats == null) runStats = FindObjectOfType<RunStatsManager>();

        // 버튼 리스너 연결
        if (btnRetry != null)
            btnRetry.onClick.AddListener(OnClickRetry);
        else
            Debug.LogError("[GameOverUI] btnRetry is not assigned!");

        if (btnRevive != null)
            btnRevive.onClick.AddListener(OnClickRevive);
        else
            Debug.LogError("[GameOverUI] btnRevive is not assigned!");
    }
    public void ShowPanel(bool canRevive)
    {
        Debug.Log("[GameOverUI] ShowPanel called with canRevive=" + canRevive);

        if (panel == null)
        {
            Debug.LogError("[GameOverUI] panelRoot is NULL! Cannot show panel.");
            return;
        }

        panel.SetActive(true);
        Debug.Log("[GameOverUI] Panel SetActive(true)");

        // 기록 갱신
        int dist = (runStats != null) ? Mathf.FloorToInt(runStats.CurrentDistance) : 0;
        int best = (runStats != null) ? Mathf.FloorToInt(runStats.BestDistance) : 0;

        if (txtDistance != null) txtDistance.text = "DIST " + dist + "m";
        if (txtBest != null) txtBest.text = "BEST " + best + "m";

        // 부활 버튼 활성/비활성
        if (btnRevive != null) btnRevive.interactable = canRevive;

        Debug.Log("[GameOverUI] ShowPanel completed");
    }

    public void Hide()
    {
        if (panel != null) panel.SetActive(false);
    }


    public void UpdateUI(bool canRevive)
    {
        Debug.Log("[GameOverUI] UpdateUI called with canRevive=" + canRevive);

        // 기록 갱신
        int dist = (runStats != null) ? Mathf.FloorToInt(runStats.CurrentDistance) : 0;
        int best = (runStats != null) ? Mathf.FloorToInt(runStats.BestDistance) : 0;

        if (txtDistance != null) txtDistance.text = "DIST " + dist + "m";
        if (txtBest != null) txtBest.text = "BEST " + best + "m";

        // 부활 버튼 활성/비활성
        if (btnRevive != null) btnRevive.interactable = canRevive;
    }

    public void OnClickRevive()
    {
        Debug.Log("[GameOverUI] OnClickRevive clicked");
        if (gameFlow == null)
        {
            Debug.LogError("[GameOverUI] gameFlow is NULL!");
            return;
        }
        gameFlow.TryRevivePlaceholder();
    }

    public void OnClickRetry()
    {
        Debug.Log("[GameOverUI] OnClickRetry clicked");
        if (gameFlow == null)
        {
            Debug.LogError("[GameOverUI] gameFlow is NULL!");
            return;
        }
        gameFlow.RetryFromGameOver();
    }
}
