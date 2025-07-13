using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class Achievement
{
    public string id;
    public string name;
    public string description;
    public string category;
    public bool isUnlocked;
    public Sprite icon;
    public int scoreReward;        // 积分奖励
    public DateTime unlockedTime;
    
    public Achievement(string id, string name, string description, string category, int scoreReward = 100)
    {
        this.id = id;
        this.name = name;
        this.description = description;
        this.category = category;
        this.scoreReward = scoreReward;
        this.isUnlocked = false;
    }
}

public class AchievementManager : MonoBehaviour
{
    public static AchievementManager Instance;
    
    [Header("Achievement Settings")]
    public List<Achievement> achievements = new List<Achievement>();
    private Dictionary<string, Achievement> achievementDict = new Dictionary<string, Achievement>();
    
    // 事件系统
    public static event Action<Achievement> OnAchievementUnlocked;
    public static event Action<int> OnTotalScoreChanged;

    private void Awake()
    {
        // 单例模式
        // PlayerPrefs.DeleteAll();
        // PlayerPrefs.Save();
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            Debug.Log("AchievementManager 初始化开始");
            InitializeAchievements();
            LoadAchievements();
            Debug.Log($"AchievementManager 初始化完成，共有 {achievements.Count} 个成就");
        }
        else
        {
            Destroy(gameObject);
        }
    }
    
    private void InitializeAchievements()
    {
        // 清空字典但保留Inspector中配置的成就列表
        achievementDict.Clear();
        
        // 只使用Inspector中配置的成就，不添加默认成就
        Debug.Log($"使用Inspector配置的成就，共 {achievements.Count} 个");
        
        // 构建字典以便快速查找
        foreach (var achievement in achievements)
        {
            achievementDict[achievement.id] = achievement;
            Debug.Log($"加载成就: {achievement.name}, 分类: {achievement.category}");
        }
    }
    
    private void AddAchievement(string id, string name, string description, string category, int scoreReward = 100)
    {
        Achievement newAchievement = new Achievement(id, name, description, category, scoreReward);
        achievements.Add(newAchievement);
    }
    
    // 直接解锁成就（简化版）
    public void UnlockAchievement(string achievementId)
    {
        if (achievementDict.ContainsKey(achievementId))
        {
            Achievement achievement = achievementDict[achievementId];
            
            if (achievement.isUnlocked) return;
            
            achievement.isUnlocked = true;
            achievement.unlockedTime = DateTime.Now;
            
            // 触发解锁事件
            OnAchievementUnlocked?.Invoke(achievement);
            
            // 触发积分变化事件
            OnTotalScoreChanged?.Invoke(GetTotalScore());
            
            // 保存数据
            SaveAchievements();
            
            Debug.Log($"成就解锁: {achievement.name} (+{achievement.scoreReward}积分)");
        }
    }
    
    // 获取总积分
    public int GetTotalScore()
    {
        int totalScore = 0;
        foreach (var achievement in achievements)
        {
            if (achievement.isUnlocked)
            {
                totalScore += achievement.scoreReward;
            }
        }
        return totalScore;
    }

    //获取coinCount
    public int GetCoinCount()
    {
        return PlayerPrefs.GetInt("CoinCount");
    }
    
    // 获取指定分类的成就
    public List<Achievement> GetAchievementsByCategory(string category)
    {
        return achievements.FindAll(a => a.category == category);
    }
    
    // 获取所有分类
    public List<string> GetAllCategories()
    {
        HashSet<string> categories = new HashSet<string>();
        foreach (var achievement in achievements)
        {
            categories.Add(achievement.category);
        }
        return new List<string>(categories);
    }
    
    // 获取成就完成度（按解锁数量计算）
    public float GetCategoryProgress(string category)
    {
        var categoryAchievements = GetAchievementsByCategory(category);
        if (categoryAchievements.Count == 0) return 0f;
        
        int unlockedCount = categoryAchievements.FindAll(a => a.isUnlocked).Count;
        return (float)unlockedCount / categoryAchievements.Count;
    }
    
    // 保存成就数据
    private void SaveAchievements()
    {
        string json = JsonUtility.ToJson(new AchievementData(achievements));
        PlayerPrefs.SetString("AchievementData", json);
        PlayerPrefs.Save();
    }
    
    // 加载成就数据
    private void LoadAchievements()
    {
        if (PlayerPrefs.HasKey("AchievementData"))
        {
            string json = PlayerPrefs.GetString("AchievementData");
            AchievementData data = JsonUtility.FromJson<AchievementData>(json);
            
            // 恢复成就状态
            foreach (var savedAchievement in data.achievements)
            {
                if (achievementDict.ContainsKey(savedAchievement.id))
                {
                    Achievement achievement = achievementDict[savedAchievement.id];
                    achievement.isUnlocked = savedAchievement.isUnlocked;
                    achievement.unlockedTime = savedAchievement.unlockedTime;
                }
            }
        }
    }
    
    //检查coinCount是否达到新的里程碑
    public void CheckCoinMilestones()
    {
        int coinCount = GetCoinCount();
        if (coinCount >= 10)
            UnlockAchievement("Coin1");
        if (coinCount >= 20)
            UnlockAchievement("Coin2");
        if (coinCount >= 50)
            UnlockAchievement("Coin3");
        
    }
    // 便捷方法 - 游戏事件触发（简化版）
    public void OnEnemyKilled()
    {
        UnlockAchievement("first_kill");
        // 可以添加击杀计数逻辑来解锁kill_100
    }
    
    public void OnLevelCompleted(int levelNumber)
    {
        UnlockAchievement("first_level");
        if (levelNumber >= 10)
            UnlockAchievement("level_10");
    }
    
    public void OnCoinsCollected(float totalCoins)
    {
        if (totalCoins >= 1000)
            UnlockAchievement("collect_coins");
    }
    
    public void OnPlayTimeReached(float totalHours)
    {
        if (totalHours >= 10)
            UnlockAchievement("play_time");
    }
}

// 用于JSON序列化的数据类
[Serializable]
public class AchievementData
{
    public List<Achievement> achievements;
    
    public AchievementData(List<Achievement> achievements)
    {
        this.achievements = achievements;
    }
}