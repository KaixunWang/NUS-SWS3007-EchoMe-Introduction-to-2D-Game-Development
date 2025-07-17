using UnityEngine;
using System.Collections;
using LootLocker.Requests;

public class LeaderboardUpdater : MonoBehaviour
{
    [Header("References")]
    public leaderboard_score leaderboardScore;
    public LeaderboardUI leaderboardUI;
    
    [Header("Settings")]
    public bool autoUpdateOnStart = true;
    
    void Start()
    {
        // 使用全局管理器确保LootLocker已初始化
        StartCoroutine(InitializeAndUpdate());
    }
    
    // 初始化并更新排行榜
    private IEnumerator InitializeAndUpdate()
    {
        // 确保LootLocker已初始化
        yield return StartCoroutine(LootLockerManager.Instance.EnsureInitialized());
        
        // 初始化后更新排行榜分数
        if (autoUpdateOnStart)
        {
            UpdateLeaderboardScore();
        }
    }
    
    // 计算并更新通关数到排行榜
    public void UpdateLeaderboardScore()
    {
        int passedLevels = GetPassedLevelsCount();
        Debug.Log($"玩家通关数: {passedLevels}");
        
        if (leaderboardScore != null)
        {
            StartCoroutine(leaderboardScore.SubmitScoreRoutine(passedLevels));
        }
    }
    
    // 统计通关的关卡数
    int GetPassedLevelsCount()
    {
        int count = 0;
        
        // 遍历所有可能的关卡（假设最多11关）
        for (int i = 0; i <= 10; i++)
        {
            int stars = PlayerPrefs.GetInt($"Level_{i}_Stars", 0);
            if (stars > 0)
            {
                count++;
            }
        }
        
        return count;
    }
    
    // 打开排行榜UI
    public void ShowLeaderboard()
    {
        if (leaderboardUI != null)
        {
            leaderboardUI.ShowLeaderboard();
        }
    }
}