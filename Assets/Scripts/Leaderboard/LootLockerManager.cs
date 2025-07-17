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
        
        // 尝试使用保存的玩家ID进行会话恢复
        string savedPlayerID = PlayerPrefs.GetString("PlayerID", "");
        if (!string.IsNullOrEmpty(savedPlayerID))
        {
            yield return StartCoroutine(TryRestoreSession(savedPlayerID));
        }
        
        // 如果恢复失败或没有保存的ID，创建新的游客会话
        if (!isInitialized)
        {
            yield return StartCoroutine(CreateNewGuestSession());
        }
        
        isInitializing = false;
    }
    
    // 尝试恢复现有会话
    private IEnumerator TryRestoreSession(string playerID)
    {
        bool done = false;
        LootLockerSDKManager.StartGuestSession(playerID, (response) =>
        {
            if (response.success)
            {
                Debug.Log("LootLocker会话恢复成功");
                Debug.Log($"玩家ID: {response.player_id}");
                
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
                Debug.LogWarning("会话恢复失败，将创建新会话: " + response.errorData.ToString());
                // 清除无效的玩家ID
                PlayerPrefs.DeleteKey("PlayerID");
                PlayerPrefs.Save();
            }
            done = true;
        });
        
        yield return new WaitWhile(() => !done);
    }
    
    // 创建新的游客会话
    private IEnumerator CreateNewGuestSession()
    {
        bool done = false;
        LootLockerSDKManager.StartGuestSession((response) =>
        {
            if (response.success)
            {
                Debug.Log("LootLocker新会话创建成功");
                Debug.Log($"新玩家ID: {response.player_id}");
                
                // 保存新的玩家ID
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
                Debug.LogError("创建LootLocker会话失败: " + response.errorData.ToString());
            }
            done = true;
        });
        
        yield return new WaitWhile(() => !done);
    }
    
    // 检查是否已初始化
    public bool IsInitialized()
    {
        return isInitialized && LootLockerSDKManager.CheckInitialized();
    }
} 