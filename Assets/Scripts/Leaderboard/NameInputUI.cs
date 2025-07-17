using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;
using LootLocker.Requests;

public class NameInputUI : MonoBehaviour
{
    [Header("UI References")]
    public TMP_InputField nameInputField; // 名字输入框
    
    void Start()
    {
        // 绑定输入框事件
        if (nameInputField != null)
        {
            nameInputField.onEndEdit.AddListener(OnNameSubmitted);
        }
        
        // 设置输入框的当前值
        if (nameInputField != null)
        {
            string currentName = LootLockerManager.Instance.GetPlayerName();
            if (!string.IsNullOrEmpty(currentName))
            {
                nameInputField.text = currentName;
            }
        }
    }
    
    // 名字提交处理
    void OnNameSubmitted(string playerName)
    {
        // 验证名字
        if (string.IsNullOrEmpty(playerName) || playerName.Trim().Length == 0)
        {
            return; // 如果为空，不处理
        }
        
        if (playerName.Length > 20)
        {
            Debug.LogWarning("名字不能超过20个字符！");
            return;
        }
        
        // 提交名字到服务器
        StartCoroutine(SetPlayerNameRoutine(playerName.Trim()));
    }
    
    // 设置玩家名字到服务器
    private IEnumerator SetPlayerNameRoutine(string playerName)
    {
        // 确保已初始化
        yield return StartCoroutine(LootLockerManager.Instance.EnsureInitialized());
        
        bool done = false;
        LootLockerSDKManager.SetPlayerName(playerName, (response) =>
        {
            if (response.success)
            {
                Debug.Log($"玩家名字设置成功: {playerName}");
                PlayerPrefs.SetString("PlayerName", playerName);
                PlayerPrefs.Save();
            }
            else
            {
                Debug.LogError($"设置玩家名字失败: {response.errorData.ToString()}");
            }
            done = true;
        });
        
        yield return new WaitWhile(() => !done);
    }
    
    // 获取当前玩家名字
    public string GetCurrentPlayerName()
    {
        return LootLockerManager.Instance.GetPlayerName();
    }
    
    // 手动设置输入框的值
    public void SetInputFieldValue(string value)
    {
        if (nameInputField != null)
        {
            nameInputField.text = value;
        }
    }
} 