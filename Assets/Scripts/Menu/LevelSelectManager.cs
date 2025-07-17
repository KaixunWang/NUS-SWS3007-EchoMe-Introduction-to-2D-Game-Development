using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class LevelSelectManager : MonoBehaviour
{
    [Header("关卡设置")]
    public string levelScenePrefix = "Level_"; // 关卡场景名称前缀
    public int totalLevels = 10; // 总关卡数

    [Header("解锁条件设置")]
    [Tooltip("每个关卡需要通过的关卡索引，-1表示默认解锁")]
    public int[] unlockRequirements; // 每个关卡需要通过的关卡索引
    [Header("UI管理")]
    public LevelSelectButtonBehaviour[] levelButtons; // 关卡按钮数组
    
    // 单例模式
    public static LevelSelectManager Instance { get; private set; }
    
    // 当前选中的关卡
    private int selectedLevel = 0;
    

    void Awake()
    {
        // 设置单例
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    void Start()
    {
        InitializeLevelButtons();
        LoadUnlockedLevels();
    }

    // 初始化关卡按钮
    void InitializeLevelButtons()
    {
        // 如果没有手动设置按钮数组，自动查找所有按钮
        if (levelButtons == null || levelButtons.Length == 0)
        {
            levelButtons = FindObjectsOfType<LevelSelectButtonBehaviour>();
        }

        // 为每个按钮设置关卡索引
        for (int i = 0; i < levelButtons.Length; i++)
        {
            if (levelButtons[i] != null)
            {
                levelButtons[i].levelIndex = i; // 从0开始，这样第0关就是第一个按钮
            }
        }

        
        Debug.Log("找到 " + levelButtons.Length + " 个关卡按钮");
    }
    

    // 加载已解锁的关卡
    void LoadUnlockedLevels()
    {
        for (int i = 0; i < levelButtons.Length; i++)
        {
            if (levelButtons[i] == null) continue;
            int stars = PlayerPrefs.GetInt($"Level_{i}_Stars", 0);
            levelButtons[i].SetStars(stars);

            // 检查解锁条件
            if (IsLevelUnlocked(i))
            {
                levelButtons[i].UnlockLevel();
            }
        }
    }

    // 检查关卡是否解锁
    public bool IsLevelUnlocked(int levelIndex)
    {
        // 如果unlockRequirements数组为空或长度不够，使用默认逻辑
        if (unlockRequirements == null || levelIndex >= unlockRequirements.Length)
        {
            // 默认逻辑：第0关默认解锁，其余关卡上一关>=1星才解锁
            return levelIndex == 0 || PlayerPrefs.GetInt($"Level_{levelIndex - 1}_Stars", 0) >= 1;
        }

        int requiredLevel = unlockRequirements[levelIndex];

        // -1表示默认解锁（比如第0关）
        if (requiredLevel == -1)
        {
            return true;
        }

        // -2表示需要前面所有关卡>=1星
        if (requiredLevel == -2)
        {
            for (int i = 0; i < levelIndex; i++)
            {
                if (PlayerPrefs.GetInt($"Level_{i}_Stars", 0) < 1)
                {
                    return false;
                }
            }
            return true;
        }

        // 检查指定关卡是否通过（>=1星）
        return PlayerPrefs.GetInt($"Level_{requiredLevel}_Stars", 0) >= 1;
    }

    // 关卡按钮被点击时的处理
    public void OnLevelButtonClicked(int levelIndex)
    {
        selectedLevel = levelIndex;
        Debug.Log("选中关卡: " + levelIndex);

        
        // 可以在这里添加确认对话框、音效等
        
        // 加载关卡场景
        LoadLevel(levelIndex);
    }
    

    // 加载关卡
    void LoadLevel(int levelIndex)
    {
        // 尝试多种场景命名格式
        string[] possibleSceneNames = {
            levelScenePrefix + levelIndex,           // "Level_0", "Level_1"
            levelScenePrefix + (levelIndex + 1),     // "Level_1", "Level_2" (如果场景从1开始命名)
            "Level" + levelIndex,                    // "Level0", "Level1"
            "Level" + (levelIndex + 1)              // "Level1", "Level2"
        };

        
        foreach (string sceneName in possibleSceneNames)
        {
            Debug.Log("尝试加载关卡: " + sceneName);
            

            try
            {
                SceneManager.LoadScene(sceneName);
                return; // 如果成功加载，直接返回
            }
            catch (System.Exception)
            {
                Debug.LogWarning("无法加载场景: " + sceneName + "。尝试下一个...");
            }
        }

        
        // 如果所有尝试都失败
        Debug.LogError("无法加载关卡 " + levelIndex + "。请确保场景已添加到Build Settings中，并检查场景命名规则。");
    }
    

    // 解锁关卡
    public void UnlockLevel(int levelIndex)
    {
        // 保存到存档系统
        PlayerPrefs.SetInt("Level_" + levelIndex + "_Unlocked", 1);
        PlayerPrefs.Save();

        // 更新按钮状态
        foreach (var button in levelButtons)
        {
            if (button != null && button.levelIndex == levelIndex)
            {
                button.UnlockLevel();
                break;
            }
        }
    }

    // 返回主菜单
    public void BackToMainMenu()
    {
        SceneManager.LoadScene("MainMenu"); // 替换为你的主菜单场景名
    }

    // 重置所有关卡进度
    public void ResetProgress()
    {
        PlayerPrefs.DeleteAll();
        Debug.Log("进度已重置");

        // 重新加载场景以更新按钮状态
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }
}