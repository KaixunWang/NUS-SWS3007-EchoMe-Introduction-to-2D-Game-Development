using System.Collections;
using UnityEngine;
using LootLocker.Requests;
using UnityEngine.UI;
using TMPro;

public class LootLockerManager : MonoBehaviour
{
    private static LootLockerManager instance;
    private bool isInitialized = false;
    private bool isInitializing = false;
    
    [Header("UI References")]
    public GameObject nameInputPanel; // 名字输入面板
    public TMP_InputField nameInputField; // 名字输入框
    public TMP_Text statusText; // 状态显示文本
    
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
    
    void Start()
    {
        // 设置UI事件
        SetupUI();
    }
    
    // 设置UI事件
    void SetupUI()
    {
        if (nameInputField != null)
        {
            // 监听EndEdit事件（失去焦点时触发）
            nameInputField.onEndEdit.AddListener(OnNameSubmitted);
        }
    }
    
    // 显示名字输入对话框
    public void ShowNameInputDialog()
    {
        if (nameInputPanel != null)
        {
            nameInputPanel.SetActive(true);
            
            // 聚焦到输入框
            if (nameInputField != null)
            {
                nameInputField.Select();
                nameInputField.ActivateInputField();
            }
            
            UpdateStatusText("请输入你的名字，按回车或点击其他地方确认");
        }
    }
    
    // 隐藏名字输入对话框
    public void HideNameInputDialog()
    {
        if (nameInputPanel != null)
        {
            nameInputPanel.SetActive(false);
        }
    }
    
    // 名字提交处理
    void OnNameSubmitted(string playerName)
    {
        // 验证名字
        if (string.IsNullOrEmpty(playerName) || playerName.Trim().Length == 0)
        {
            UpdateStatusText("名字不能为空！");
            return;
        }
        
        if (playerName.Length > 20)
        {
            UpdateStatusText("名字不能超过20个字符！");
            return;
        }
        
        // 提交名字到服务器
        StartCoroutine(SetPlayerNameRoutine(playerName.Trim()));
    }
    
    // 设置玩家名字到服务器
    public IEnumerator SetPlayerNameRoutine(string playerName)
    {
        // 确保已初始化
        yield return StartCoroutine(EnsureInitialized());
        
        UpdateStatusText("正在设置名字...");
        
        bool done = false;
        LootLockerSDKManager.SetPlayerName(playerName, (response) =>
        {
            if (response.success)
            {
                Debug.Log($"玩家名字设置成功: {playerName}");
                PlayerPrefs.SetString("PlayerName", playerName);
                PlayerPrefs.Save();
                UpdateStatusText("名字设置成功！");
                
                // 通知UI更新
                NameInputUI nameInputUI = FindObjectOfType<NameInputUI>();
                if (nameInputUI != null)
                {
                    nameInputUI.OnNameSetSuccess();
                }
                
                // 延迟隐藏对话框
                StartCoroutine(DelayedHideDialog());
            }
            else
            {
                Debug.LogError($"设置玩家名字失败: {response.errorData.ToString()}");
                UpdateStatusText($"设置失败: {response.errorData.message}");
            }
            done = true;
        });
        
        yield return new WaitWhile(() => !done);
    }
    
    // 延迟隐藏对话框
    private IEnumerator DelayedHideDialog()
    {
        yield return new WaitForSeconds(10.5f);
        HideNameInputDialog();
    }
    
    // 更新状态文本
    void UpdateStatusText(string message)
    {
        if (statusText != null)
        {
            statusText.text = message;
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