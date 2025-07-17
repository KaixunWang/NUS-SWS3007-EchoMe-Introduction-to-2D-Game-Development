using System.Collections;
using UnityEngine;
using LootLocker.Requests;

public class LootLockerManager : MonoBehaviour
{
    private static LootLockerManager instance;
    private bool isInitialized = false;
    private bool isInitializing = false;
    
    public static LootLockerManager Instance
    {
        get
        {
            if (instance == null)
            {
                instance = FindObjectOfType<LootLockerManager>();
                if (instance == null)
                {
                    GameObject go = new GameObject("LootLockerManager");
                    instance = go.AddComponent<LootLockerManager>();
                    DontDestroyOnLoad(go);
                }
            }
            return instance;
        }
    }
    
    void Awake()
    {

        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else if (instance != this)
        {
            Destroy(gameObject);
        }
    }
    
    // 获取当前玩家名字
    public string GetPlayerName()
    {
        return PlayerPrefs.GetString("PlayerName", "");
    }
    
    // 获取玩家显示名称（优先显示名字，没有名字则显示ID）
    public string GetPlayerDisplayName(int playerId, string playerName = "")
    {
        // 如果传入了玩家名字且不为空，使用传入的名字
        if (!string.IsNullOrEmpty(playerName))
        {
            return playerName;
        }
        
        // 如果是当前玩家，尝试获取本地保存的名字
        string currentPlayerID = PlayerPrefs.GetString("PlayerID", "");
        if (currentPlayerID == playerId.ToString())
        {
            string savedName = GetPlayerName();
            if (!string.IsNullOrEmpty(savedName))
            {
                return savedName;
            }
        }
        
        // 如果都没有，返回玩家ID
        return $"玩家{playerId}";
    }
    
    // 确保LootLocker已初始化
    public IEnumerator EnsureInitialized()
    {
        if (isInitialized)
        {
            yield break;
        }
        
        if (isInitializing)
        {
            // 如果正在初始化，等待完成
            yield return new WaitWhile(() => isInitializing);
            yield break;
        }
        
        isInitializing = true;
        
        // 检查是否已经初始化
        if (LootLockerSDKManager.CheckInitialized())
        {
            isInitialized = true;
            isInitializing = false;
            yield break;
        }
        
        // 使用设备唯一标识符进行游客登录
        bool done = false;
        LootLockerSDKManager.StartGuestSession(SystemInfo.deviceUniqueIdentifier, (response) =>
        {
            if (response.success)
            {
                Debug.Log("LootLocker会话启动成功");
                Debug.Log($"玩家ID: {response.player_id}");
                
                // 保存玩家ID
                PlayerPrefs.SetString("PlayerID", response.player_id.ToString());
                PlayerPrefs.Save();
                
                // 更新UIDgetter显示
                UIDgetter uidGetter = FindObjectOfType<UIDgetter>();
                if (uidGetter != null)
                {
                    uidGetter.UpdatePlayerID(response.player_id.ToString());
                }
                
                isInitialized = true;
            }
            else
            {
                Debug.LogError("启动LootLocker会话失败: " + response.errorData.ToString());
            }
            done = true;
        });
        
        yield return new WaitWhile(() => !done);
        isInitializing = false;
    }
    
    // 检查是否已初始化
    public bool IsInitialized()
    {
        return isInitialized && LootLockerSDKManager.CheckInitialized();
    }
} 