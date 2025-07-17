using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;
using LootLocker.Requests;

public class LeaderboardUI : MonoBehaviour
{
    [Header("UI References")]
    public GameObject leaderboardPanel;
    public GameObject scoreEntryPrefab; // 每个玩家分数的UI预制体
    public Transform scoreContainer; // 放置分数条目的父对象
    public Button refreshButton;
    public Button closeButton;
    public TextMeshProUGUI loadingText;
    
    [Header("排行榜选择")]
    public TMP_Dropdown leaderboardDropdown; // 排行榜选择下拉框
    public TextMeshProUGUI leaderboardTitleText; // 排行榜标题文本
    
    [Header("Components")]
    public leaderboard_score leaderboardManager; // 总通关数排行榜
    
    private List<GameObject> scoreEntries = new List<GameObject>();
    private int currentLevelCount = 10; // 假设有10关，可以根据实际情况调整
    
    // 排行榜键定义
    private const string TOTAL_LEVELS_KEY = "passlevelnumber"; // 总通关数排行榜
    
    // 关卡对应的排行榜键配置
    [System.Serializable]
    public class LevelLeaderboardConfig
    {
        public int levelIndex;
        public string leaderboardKey;
    }
    
    [Header("关卡排行榜配置")]
    public List<LevelLeaderboardConfig> levelLeaderboardConfigs = new List<LevelLeaderboardConfig>();
    
    void Start()
    {
        // 绑定按钮事件
        if (refreshButton != null)
            refreshButton.onClick.AddListener(RefreshLeaderboard);
            
        if (closeButton != null)
            closeButton.onClick.AddListener(() => leaderboardPanel.SetActive(false));
            
        // 初始隐藏排行榜
        leaderboardPanel.SetActive(false);
        
        // 初始化关卡排行榜配置（如果没有设置的话）
        InitializeLevelConfigs();
        
        // 初始化下拉框
        InitializeDropdown();
    }
    
    // 初始化关卡排行榜配置
    void InitializeLevelConfigs()
    {
        if (levelLeaderboardConfigs.Count == 0)
        {
            // 如果没有配置，使用默认配置
            for (int i = 0; i <= currentLevelCount; i++)
            {
                levelLeaderboardConfigs.Add(new LevelLeaderboardConfig
                {
                    levelIndex = i,
                    leaderboardKey = $"level{i}time" // 默认键名
                });
            }
        }
    }
    
    // 初始化下拉框
    void InitializeDropdown()
    {
        if (leaderboardDropdown != null)
        {
            // 清空现有选项
            leaderboardDropdown.ClearOptions();
            
            // 添加排行榜选项
            List<string> options = new List<string>();
            options.Add("TotalLevelPassed");
            
            // 添加各关卡排行榜
            for (int i = 0; i <= currentLevelCount; i++)
            {
                options.Add($"Lv.{i} Time");
            }
            
            leaderboardDropdown.AddOptions(options);
            
            // 绑定选择事件
            leaderboardDropdown.onValueChanged.AddListener(OnLeaderboardChanged);
        }
    }
    
    // 下拉框选择改变事件
    void OnLeaderboardChanged(int index)
    {
        RefreshLeaderboard();
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
        
        // 根据下拉框选择获取对应排行榜数据
        int selectedIndex = leaderboardDropdown != null ? leaderboardDropdown.value : 0;
        
        if (selectedIndex == 0)
        {
            // 总通关数排行榜
            yield return StartCoroutine(FetchLeaderboardData(TOTAL_LEVELS_KEY, "Total Level"));
        }
        else
        {
            // 关卡排行榜
            int levelIndex = selectedIndex - 1; // 减去第一个选项
            string levelKey = GetLevelLeaderboardKey(levelIndex);
            yield return StartCoroutine(FetchLeaderboardData(levelKey, $"Lv.{levelIndex} Time"));
        }
        
        // 隐藏加载文本
        if (loadingText != null)
            loadingText.gameObject.SetActive(false);
    }
    
    // 获取排行榜数据
    private IEnumerator FetchLeaderboardData(string leaderboardKey, string title)
    {
        // 确保LootLocker已初始化
        yield return StartCoroutine(LootLockerManager.Instance.EnsureInitialized());
        
        bool done = false;
        List<Player_LevelPassed> scores = new List<Player_LevelPassed>();
        
        LootLockerSDKManager.GetScoreList(leaderboardKey, 15, 0, (response) =>
        {
            if (response.success)
            {
                Debug.Log($"成功获取排行榜数据: {leaderboardKey}");
                
                //如果排行榜数据为空，则return
                if(response.items.Length == 0){
                    Debug.Log("排行榜数据为空");
                    done = true;
                    return;
                }
                // 遍历获取的数据并解析
                foreach (var item in response.items)
                {
                    
                    int player_id = item.player.id;
                    int score = item.score;
                    string player_name = item.player.name;
                    
                    // 使用LootLockerManager获取更好的显示名称
                    string displayName = LootLockerManager.Instance.GetPlayerDisplayName(player_id, player_name);
                    
                    // 添加到列表
                    scores.Add(new Player_LevelPassed(player_id, score, displayName));
                }
                
                Debug.Log($"获取到 {scores.Count} 条排行榜数据");
            }
            else
            {
                Debug.LogError($"获取排行榜数据失败: {response.errorData.ToString()}");
            }
            done = true;
        });
        
        yield return new WaitWhile(() => !done);
        
        // 创建UI条目
        CreateScoreEntries(scores, title);
    }
    
    // 创建排行榜条目
    void CreateScoreEntries(List<Player_LevelPassed> scores, string title)
    {
        // 更新标题
        if (leaderboardTitleText != null)
            leaderboardTitleText.text = title;
        
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
                playerText.text = scores[i].player_name;
            }
            
            // 设置分数（通关数或时间）
            TextMeshProUGUI scoreText = entry.transform.Find("LevelPassed").GetComponent<TextMeshProUGUI>();
            if (scoreText != null)
            {
                if (title.Contains("Time"))
                {
                    // 显示时间格式 mm:ss.xxx
                    // level_passed现在是毫秒数
                    int totalMilliseconds = scores[i].level_passed;
                    int minutes = totalMilliseconds / (60 * 1000);
                    int seconds = (totalMilliseconds % (60 * 1000)) / 1000;
                    int milliseconds = totalMilliseconds % 1000;
                    scoreText.text = $"{minutes:00}:{seconds:00}.{milliseconds:000}";
                }
                else
                {
                    // 显示通关数
                    scoreText.text = $"{scores[i].level_passed}";
                }
            }
            
            // 前三名特殊效果
            ApplyTopThreeEffects(entry, i);
        }
    }
    
    // 应用前三名特殊效果
    void ApplyTopThreeEffects(GameObject entry, int index)
    {
        if (index < 3)
        {
            Image background = entry.GetComponent<Image>();
            if (background != null)
            {
                switch (index)
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
    
    // 提交分数到选中的排行榜
    public void SubmitScore(int score)
    {
        StartCoroutine(SubmitScoreRoutine(score));
    }
    
    private IEnumerator SubmitScoreRoutine(int score)
    {
        int selectedIndex = leaderboardDropdown != null ? leaderboardDropdown.value : 0;
        
        if (selectedIndex == 0)
        {
            // 提交到总通关数排行榜
            yield return StartCoroutine(SubmitToLeaderboard(TOTAL_LEVELS_KEY, score));
        }
        else
        {
            // 提交到关卡排行榜
            int levelIndex = selectedIndex - 1;
            string levelKey = GetLevelLeaderboardKey(levelIndex);
            yield return StartCoroutine(SubmitToLeaderboard(levelKey, score));
        }
        
        // 提交后自动刷新排行榜
        RefreshLeaderboard();
    }
    
    // 提交分数到指定排行榜
    private IEnumerator SubmitToLeaderboard(string leaderboardKey, int score)
    {
        // 确保LootLocker已初始化
        yield return StartCoroutine(LootLockerManager.Instance.EnsureInitialized());
        
        bool done = false;
        string playerID = PlayerPrefs.GetString("PlayerID");
        
        LootLockerSDKManager.SubmitScore(playerID, score, leaderboardKey, (response) =>
        {
            if (response.success)
            {
                Debug.Log($"成功提交分数到 {leaderboardKey}: {score}");
            }
            else
            {
                Debug.LogError($"提交分数失败: {response.errorData.ToString()}");
            }
            done = true;
        });
        
        yield return new WaitWhile(() => !done);
    }
    
    // 获取关卡对应的排行榜键
    private string GetLevelLeaderboardKey(int levelIndex)
    {
        // 查找配置中的排行榜键
        foreach (var config in levelLeaderboardConfigs)
        {
            if (config.levelIndex == levelIndex)
            {
                return config.leaderboardKey;
            }
        }
        
        // 如果没找到配置，使用默认键名
        return $"level{levelIndex}time";
    }
    
    // 设置关卡数量（可以在运行时调整）
    public void SetLevelCount(int count)
    {
        currentLevelCount = count;
        InitializeLevelConfigs();
        InitializeDropdown();
    }
    
    // 添加关卡排行榜配置
    public void AddLevelLeaderboardConfig(int levelIndex, string leaderboardKey)
    {
        // 检查是否已存在
        for (int i = 0; i < levelLeaderboardConfigs.Count; i++)
        {
            if (levelLeaderboardConfigs[i].levelIndex == levelIndex)
            {
                levelLeaderboardConfigs[i].leaderboardKey = leaderboardKey;
                return;
            }
        }
        
        // 如果不存在，添加新的配置
        levelLeaderboardConfigs.Add(new LevelLeaderboardConfig
        {
            levelIndex = levelIndex,
            leaderboardKey = leaderboardKey
        });
    }
}