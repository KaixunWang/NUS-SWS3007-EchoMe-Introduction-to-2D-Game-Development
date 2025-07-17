using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;
using LootLocker.Requests;

public class NameInputUI : MonoBehaviour
{
    [Header("UI References")]
    public TMP_InputField nameInputField; // 名字输入框
    public TMP_Text currentNameText; // 显示当前名字的文本
    
    void Start()
    {
        // 绑定输入框事件
        if (nameInputField != null)
        {
            nameInputField.onEndEdit.AddListener(OnNameSubmitted);
        }
        
        // 显示当前名字
        UpdateCurrentNameDisplay();
        
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
                UpdateCurrentNameDisplay();
            }
            else
            {
                Debug.LogError($"设置玩家名字失败: {response.errorData.ToString()}");
            }
            done = true;
        });
        
        yield return new WaitWhile(() => !done);
    }
    
    // 更新当前名字显示
    public void UpdateCurrentNameDisplay()
    {
        if (currentNameText != null)
        {
            string currentName = LootLockerManager.Instance.GetPlayerName();
            if (string.IsNullOrEmpty(currentName))
            {
                currentNameText.text = "未设置名字";
            }
            else
            {
                currentNameText.text = $"当前名字: {currentName}";
            }
        }
    }
    
    // 这个方法可以在名字设置成功后调用，用于刷新显示
    public void OnNameSetSuccess()
    {
        UpdateCurrentNameDisplay();
    }
} 