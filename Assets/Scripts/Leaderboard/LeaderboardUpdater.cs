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
        // 初始化LootLocker会话
        StartGuestSession();
    }
    
    // 使用访客会话（不需要登录）
    void StartGuestSession()
    {
        LootLockerSDKManager.StartGuestSession((response) =>
        {
            if (response.success)
            {
                Debug.Log("LootLocker会话启动成功");
                Debug.Log($"玩家ID: {response.player_id}");
                
                // 保存LootLocker生成的玩家ID
                PlayerPrefs.SetString("PlayerID", response.player_id.ToString());
                PlayerPrefs.Save();
                
                // 会话启动后更新排行榜分数
                if (autoUpdateOnStart)
                {
                    UpdateLeaderboardScore();
                }
            }
            else
            {
                Debug.LogError("启动LootLocker会话失败: " + response.errorData.ToString());
            }
        });
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
        
        // 遍历所有可能的关卡（假设最多100关）
        for (int i = 0; i < 100; i++)
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