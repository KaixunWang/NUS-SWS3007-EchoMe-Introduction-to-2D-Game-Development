using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class LeaderboardUI : MonoBehaviour
{
    [Header("UI References")]
    public GameObject leaderboardPanel;
    public GameObject scoreEntryPrefab; // 每个玩家分数的UI预制体
    public Transform scoreContainer; // 放置分数条目的父对象
    public Button refreshButton;
    public Button closeButton;
    public TextMeshProUGUI loadingText;
    
    [Header("Components")]
    public leaderboard_score leaderboardManager;
    
    private List<GameObject> scoreEntries = new List<GameObject>();
    
    void Start()
    {
        // 绑定按钮事件
        if (refreshButton != null)
            refreshButton.onClick.AddListener(RefreshLeaderboard);
            
        if (closeButton != null)
            closeButton.onClick.AddListener(() => leaderboardPanel.SetActive(false));
            
        // 初始隐藏排行榜
        leaderboardPanel.SetActive(false);
    }
    
    // 显示排行榜
    public void ShowLeaderboard()
    {
        leaderboardPanel.SetActive(true);
        RefreshLeaderboard();
    }
    
    // 刷新排行榜数据
    public void RefreshLeaderboard()
    {
        StartCoroutine(RefreshLeaderboardRoutine());
    }
    
    private IEnumerator RefreshLeaderboardRoutine()
    {
        // 显示加载中
        if (loadingText != null)
        {
            loadingText.gameObject.SetActive(true);
            loadingText.text = "Loading...";
        }
        
        // 清空旧的UI条目
        foreach (var entry in scoreEntries)
        {
            Destroy(entry);
        }
        scoreEntries.Clear();
        
        // 获取最新数据
        yield return StartCoroutine(leaderboardManager.FetchTopHighScoresRoutine());
        
        // 获取分数列表
        List<Player_LevelPassed> scores = leaderboardManager.GetScores();
        
        // 创建UI条目
        for (int i = 0; i < scores.Count; i++)
        {
            GameObject entry = Instantiate(scoreEntryPrefab, scoreContainer);
            scoreEntries.Add(entry);
            
            // 设置排名
            TextMeshProUGUI rankText = entry.transform.Find("Rank").GetComponent<TextMeshProUGUI>();
            if (rankText != null)
                rankText.text = (i + 1).ToString();
            
            // 设置玩家ID
            TextMeshProUGUI playerText = entry.transform.Find("ID").GetComponent<TextMeshProUGUI>();
            if (playerText != null)
            {
                // 使用LootLockerManager获取更好的显示名称
                string displayName = LootLockerManager.Instance.GetPlayerDisplayName(scores[i].player_id, scores[i].player_name);
                playerText.text = displayName;
            }
            
            // 设置通关数
            TextMeshProUGUI scoreText = entry.transform.Find("LevelPassed").GetComponent<TextMeshProUGUI>();
            if (scoreText != null)
                scoreText.text = $"{scores[i].level_passed}";
            
            // 如果是前三名，可以添加特殊效果
            if (i < 3)
            {
                Image background = entry.GetComponent<Image>();
                if (background != null)
                {
                    switch (i)
                    {
                        case 0: // 金牌
                            background.color = new Color(1f, 0.84f, 0f, 0.3f);
                            break;
                        case 1: // 银牌
                            background.color = new Color(0.75f, 0.75f, 0.75f, 0.3f);
                            break;
                        case 2: // 铜牌
                            background.color = new Color(0.8f, 0.5f, 0.2f, 0.3f);
                            break;
                    }
                }
            }
        }
        
        // 隐藏加载文本
        if (loadingText != null)
            loadingText.gameObject.SetActive(false);
    }
    
    // 提交当前玩家的分数
    public void SubmitCurrentPlayerScore(int levelsPassed)
    {
        StartCoroutine(SubmitScoreRoutine(levelsPassed));
    }
    
    private IEnumerator SubmitScoreRoutine(int score)
    {
        yield return StartCoroutine(leaderboardManager.SubmitScoreRoutine(score));
        
        // 提交后自动刷新排行榜
        RefreshLeaderboard();
    }
}