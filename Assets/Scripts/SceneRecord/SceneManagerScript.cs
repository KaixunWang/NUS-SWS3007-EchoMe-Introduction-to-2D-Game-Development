using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.SceneManagement;
using LootLocker.Requests;

public class SceneManagerScript : MonoBehaviour
{
    private SceneState currentState;
    private bool isRecording = false;
    public GameObject player;
    public GameObject clock;
    public GameObject win;
    public GameObject lose;
    public GameObject coinSystem; // Reference to the CoinSystemScript
    public List<GameObject> switches; // List of switch GameObjects
    public List<GameObject> pressurePlates; // List of pressure plate GameObjects
    public List<GameObject> doors; // List of door GameObjects
    public List<GameObject> boxes; // List of box GameObjects
    public List<GameObject> lifts; // List of lift GameObjects
    public List<GameObject> spikes; // List of spike GameObjects
    private int score = 0;
    public int levelGoodTime = 60;
    private int cooldown = 100;
    private bool hasSubmittedLeaderboard = false;
    // Start is called before the first frame update

    private int currentLevelIndex = 0;
    
    // 关卡排行榜键配置
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
        Debug.Assert(player != null, "Player GameObject is not assigned in the SceneManagerScript.");
        Debug.Assert(clock != null, "Clock GameObject is not assigned in the SceneManagerScript.");
        Debug.Assert(win != null, "Win GameObject is not assigned in the SceneManagerScript.");
        Debug.Assert(lose != null, "Lose GameObject is not assigned in the SceneManagerScript.");
        Debug.Assert(coinSystem != null, "CoinSystemScript GameObject is not assigned in the SceneManagerScript.");
        win.SetActive(false);
        lose.SetActive(false);
        
        // 获取当前关卡编号
        InitializeLevelInfo();
        
        // 初始化关卡排行榜配置（如果没有设置的话）
        InitializeLevelConfigs();
    }
    
    // 初始化关卡信息
    void InitializeLevelInfo()
    {
        string sceneName = UnityEngine.SceneManagement.SceneManager.GetActiveScene().name;
        if (sceneName.StartsWith("Level_"))
        {
            int.TryParse(sceneName.Substring("Level_".Length), out currentLevelIndex);
        }
    }

    // 初始化关卡排行榜配置
    void InitializeLevelConfigs()
    {
        if (levelLeaderboardConfigs.Count == 0)
        {
            // 如果没有配置，使用默认配置
            for (int i = 0; i <= 10; i++) // 假设最多10关
            {
                levelLeaderboardConfigs.Add(new LevelLeaderboardConfig
                {
                    levelIndex = i,
                    leaderboardKey = $"level{i}time" // 默认键名
                });
            }
        }
    }

    
    // Update is called once per frame
    void Update()
    {
        var playerBehaviour = player.GetComponent<PlayerBehaviour>();
        if (playerBehaviour.getState() && !isRecording)
        {
            SaveState();
        }
        if (!(playerBehaviour.getState()) && isRecording)
        {
            LoadState();
        }
        if (playerBehaviour.IsWin())
        {
            GameObject.Find("PauseObject").GetComponent<PauseMenuManager>().SetEndState(); // 设置游戏结束状态
            win.SetActive(true);
            string message = "";
            score = 1;

            if (coinSystem.GetComponent<CoinSystemScript>().GetCoinCount() == 3)
            {
                score++;
                message += "Collect Coins:3/3\n";
            } else {
                message += "Collect Coins:" + coinSystem.GetComponent<CoinSystemScript>().GetCoinCount() + "/3\n";
            }
            int time = (int)clock.GetComponent<TimerBehavior>().GetElapsedTime();
            float preciseTime = clock.GetComponent<TimerBehavior>().GetPreciseElapsedTime();
            if (time <= levelGoodTime)
            {
                score++;
                //如果当前scene是Level_3，则触发成就PassLevel3
                if (SceneManager.GetActiveScene().name == "Level_3" && clock.GetComponent<TimerBehavior>().GetElapsedTime() <= 20)
                {
                    AchievementManager.Instance.UnlockAchievement("PassLevel3_Under20");
                }
                message += "Time: " + time + "/" + levelGoodTime + "s\n";
            }
            else
            {
                message += "Time: " + time + "/" + levelGoodTime + "s\n";
            }
           
            // 提交通关时间到当前关卡排行榜（使用精确时间，包含毫秒）
                // 为避免在Update中多次提交排行榜，只在首次胜利时提交
                if (!hasSubmittedLeaderboard)
                {
                    StartCoroutine(SubmitLevelTimeToLeaderboard(preciseTime));
                    hasSubmittedLeaderboard = true;
                }
            
            
            win.GetComponent<WinScript>().SetStars(score);
            win.GetComponent<WinScript>().ShowWinPanel(message);
            clock.GetComponent<TimerBehavior>().SetTimer(false);
            // clock.SetActive(false);
            // player.SetActive(false);
            Debug.Log("You Win!");

            // ----------- 新增：保存星星数到PlayerPrefs -----------
            // 获取当前关卡编号
            int currentLevelIndex = 0;
            string sceneName = UnityEngine.SceneManagement.SceneManager.GetActiveScene().name;
            if (sceneName.StartsWith("Level_"))
            {
                int.TryParse(sceneName.Substring("Level_".Length), out currentLevelIndex);
            }
            // 只保存更高的星星数
            int oldStars = PlayerPrefs.GetInt($"Level_{currentLevelIndex}_Stars", 0);
            if (score > oldStars)
            {
                PlayerPrefs.SetInt($"Level_{currentLevelIndex}_Stars", score);
                PlayerPrefs.Save();
                Debug.Log($"保存关卡{currentLevelIndex}星星数: {score}");
            }

            // ---------------------------------------------
        }else if (playerBehaviour.IsLose())
        {
            GameObject.Find("PauseObject").GetComponent<PauseMenuManager>().SetEndState(); // 设置游戏结束状态
            lose.SetActive(true);
            clock.GetComponent<TimerBehavior>().SetTimer(false);
            // clock.SetActive(false);
            // player.SetActive(false);
            Debug.Log("You Lose!");
        }
    }

    void SaveState()
    {
        currentState = new SceneState
        {
            playerPosition = player.transform.position
            // switchState = switchState, // Assuming you have a switch state
            // pressurePlateState = pressurePlateState, // Assuming you have a pressure plate state
            // doorState = doorState, // Assuming you have a door state
            // boxPosition = boxPosition // Assuming you have a box position
        };
        foreach (var switchObj in switches)
        {
            var switchComponent = switchObj.GetComponent<Cainos.PixelArtPlatformer_Dungeon.Switch>();
            if (switchComponent != null)
            {
                currentState.switchStates.Add(switchComponent.IsOn);
                var level3 = switchObj.GetComponent<Switch_Level3>();
                if (level3 != null)
                {
                    float timer = level3.getTime();
                    float currentTime = level3.getCurrentTime();
                    currentState.switchRemainingTimes.Add(currentTime - timer);
                }
                else
                {
                    currentState.switchRemainingTimes.Add(switchComponent.GetRemainingTime());
                }
                
                Debug.Log($"Switch {switchObj.name} state: {switchComponent.IsOn}");
            }
        }
        foreach (var plateObj in pressurePlates)
        {
            var plateComponent = plateObj.GetComponent<BoardBehavior>();
            if (plateComponent != null)
            {
                currentState.pressurePlateStates.Add(plateComponent.GetBoardState());
                Debug.Log($"Pressure Plate {plateObj.name} state: {plateComponent.GetBoardState()}");
            }
        }
        foreach (var doorObj in doors)
        {
            var doorComponent = doorObj.GetComponent<Cainos.PixelArtPlatformer_Dungeon.Door>();
            if (doorComponent != null)
            {
                currentState.doorStates.Add(doorComponent.IsOpened);
                Debug.Log($"Door {doorObj.name} state: {doorComponent.IsOpened}");
            }
        }
        foreach (var boxObj in boxes)
        {
            currentState.boxPositions.Add(boxObj.transform.position);
            currentState.boxSpeed.Add(boxObj.GetComponent<Rigidbody2D>().velocity); // Assuming you want to save the speed of the box
        }
        // Debug.Log("lifts.Count: " + lifts.Count);
        foreach (var liftObj in lifts)
        {
            var platformController = liftObj.GetComponent<Bundos.MovingPlatforms.PlatformController>();
            if (platformController != null)
            {
                currentState.lifts.Add(platformController.GetStatus());
                Debug.Log("Lift " + liftObj.name + " state saved.");
            }
            else
            {
                Debug.LogWarning("Lift " + liftObj.name + " does not have a PlatformController component.");
            }
        }
        foreach (var spikeObj in spikes)
        {
            var spikeBehaviour = spikeObj.GetComponent<SpikeBehaviour>();
            if (spikeBehaviour != null)
            {
                currentState.spikes.Add(spikeBehaviour.getState());
                Debug.Log("Spike " + spikeObj.name + " state saved.");
            }
            else
            {
                Debug.LogWarning("Spike " + spikeObj.name + " does not have a SpikeBehaviour component.");
            }
        }
        isRecording = true;
        Debug.Log("Scene state saved");
    }
    void LoadState()
    {
        if (currentState != null)
        {
            player.transform.position = currentState.playerPosition;
            for (int i = 0; i < switches.Count && i < currentState.switchStates.Count; i++)
            {
                var switchComponent = switches[i].GetComponent<Cainos.PixelArtPlatformer_Dungeon.Switch>();
                if (switchComponent != null)
                {
                    if (currentState.switchStates[i])
                    {
                        if (!(switchComponent.IsOn))
                        {
                            switchComponent.TriggerSwitch();
                        }
                        switchComponent.SetRemainingTime(currentState.switchRemainingTimes[i]);
                    }
                    else
                    {
                        if (switchComponent.IsOn)
                        {
                            switchComponent.TriggerSwitch();
                        }
                        switchComponent.SetRemainingTime(0f); // Reset remaining time if switch is off
                    }
                    // else
                    // {
                    //     switchComponent.SetRemainingTime(0f); // Reset remaining time if switch is off
                    // }
                    var level3 = switches[i].GetComponent<Switch_Level3>();
                    if (level3 != null)
                    {
                        float currentTime = level3.getCurrentTime();
                        float timer = currentTime - currentState.switchRemainingTimes[i];
                        level3.setTime(timer);
                        Debug.Log($"Switch_Level3 {switches[i].name} state loaded with remaining time: {currentState.switchRemainingTimes[i]}");
                    }
                }
            }
            for (int i = 0; i < pressurePlates.Count && i < currentState.pressurePlateStates.Count; i++)
            {
                var plateComponent = pressurePlates[i].GetComponent<BoardBehavior>();
                if (plateComponent != null)
                {
                    plateComponent.SetBoardState(currentState.pressurePlateStates[i]);
                }
            }
            for (int i = 0; i < doors.Count && i < currentState.doorStates.Count; i++)
            {
                var doorComponent = doors[i].GetComponent<Cainos.PixelArtPlatformer_Dungeon.Door>();
                if (doorComponent != null)
                {
                    doorComponent.IsOpened = currentState.doorStates[i];
                    if (doors[i].tag == "gate") doors[i].GetComponent<Collider2D>().isTrigger = currentState.doorStates[i];
                }
            }
            for (int i = 0; i < boxes.Count && i < currentState.boxPositions.Count; i++)
            {
                boxes[i].transform.position = currentState.boxPositions[i];
                boxes[i].GetComponent<Rigidbody2D>().velocity = currentState.boxSpeed[i]; // Restore box speed
                // var boxComponent = boxes[i].GetComponent<BoxesBehavior>();
                // if (boxComponent != null)
                // {
                //     boxComponent.transform.position = currentState.boxPositions[i];
                // }
            }
            Debug.Log("lifts.Count: " + lifts.Count);
            Debug.Log("currentState.lifts.Count: " + currentState.lifts.Count);
            for (int i = 0; i < lifts.Count && i < currentState.lifts.Count; i++)
            {
                var platformController = lifts[i].GetComponent<Bundos.MovingPlatforms.PlatformController>();
                if (platformController != null)
                {
                    platformController.SetStatus(currentState.lifts[i]);
                    Debug.Log("Lift " + lifts[i].name + " state loaded.");
                }
            }
            for (int i = 0; i < spikes.Count && i < currentState.spikes.Count; i++)
            {
                var spikeBehaviour = spikes[i].GetComponent<SpikeBehaviour>();
                if (spikeBehaviour != null)
                {
                    spikeBehaviour.setState(currentState.spikes[i]);
                    Debug.Log("Spike " + spikes[i].name + " state loaded.");
                }
                else
                {
                    Debug.LogWarning("Spike " + spikes[i].name + " does not have a SpikeBehaviour component.");
                }
            }
            // Restore other states as needed
            isRecording = false;
            Debug.Log("Scene state loaded");
        }
        else
        {
            Debug.LogWarning("No saved state to load");
        }
    }
    public int GetScore()
    {
        return score;
    }
    
    // 提交通关时间到当前关卡排行榜
    private IEnumerator SubmitLevelTimeToLeaderboard(float elapsedTime)
    {
        // 确保LootLocker已初始化
        yield return StartCoroutine(LootLockerManager.Instance.EnsureInitialized());
        
        bool done = false;
        string playerID = PlayerPrefs.GetString("PlayerID");
        string leaderboardKey = GetCurrentLevelLeaderboardKey();
        
        // 将浮点数时间转换为毫秒整数（LootLocker只接受整数）
        int timeInMilliseconds = Mathf.RoundToInt(elapsedTime * 1000);
        
        LootLockerSDKManager.SubmitScore(playerID, timeInMilliseconds, leaderboardKey, (response) =>
        {
            if (response.success)
            {
                Debug.Log($"成功提交关卡{currentLevelIndex}通关时间: {elapsedTime:F3}秒 ({timeInMilliseconds}毫秒) 到排行榜: {leaderboardKey}");
            }
            else
            {
                Debug.LogError($"提交关卡{currentLevelIndex}通关时间失败: {response.errorData.ToString()}");
            }
            done = true;
        });
        
        yield return new WaitWhile(() => !done);
    }
    
    // 获取当前关卡对应的排行榜键
    private string GetCurrentLevelLeaderboardKey()
    {
        // 查找配置中的排行榜键
        foreach (var config in levelLeaderboardConfigs)
        {
            if (config.levelIndex == currentLevelIndex)
            {
                return config.leaderboardKey;
            }
        }
        
        // 如果没找到配置，使用默认键名
        return $"level_{currentLevelIndex}_leaderboard";
    }
    

    
    // 获取当前关卡编号
    public int GetCurrentLevelIndex()
    {
        return currentLevelIndex;
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
