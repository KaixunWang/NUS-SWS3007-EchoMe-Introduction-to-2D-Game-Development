using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem;

public class KeyBindingManager : MonoBehaviour
{
    [Header("按钮引用")]
    public Button bindWButton;
    public Button bindSpaceButton;
    
    [Header("Input Action引用")]
    public InputActionReference jumpActionRef;
    
    private InputAction jumpAction;
    private InputActionRebindingExtensions.RebindingOperation rebindOperation;
    
    void Start()
    {
        // 获取jump action
        jumpAction = jumpActionRef.action;
        
        // 确保action已启用
        if (!jumpAction.enabled)
        {
            jumpAction.Enable();
        }
        if (jumpAction == null||bindWButton==null||bindSpaceButton==null)return;
        // 绑定按钮事件
        bindWButton.onClick.AddListener(() => StartRebind(0)); // 重新绑定第一个绑定
        bindSpaceButton.onClick.AddListener(() => StartRebind(0)); // 重新绑定第一个绑定
        
        Debug.Log("KeyBindingManager初始化完成");
    }
    
    void StartRebind(int bindingIndex)
    {
        if (jumpAction == null||bindWButton==null||bindSpaceButton==null)return;

        // 取消之前的重新绑定操作
        rebindOperation?.Cancel();
        
        // 禁用action以避免冲突
        jumpAction.Disable();
        
        // 开始交互式重新绑定
        rebindOperation = jumpAction.PerformInteractiveRebinding(bindingIndex)
            .OnComplete(operation =>
            {
                Debug.Log($"重新绑定完成: {operation.selectedControl?.path}");
                jumpAction.Enable();
                rebindOperation = null;
                
                // 保存绑定设置
                SaveBindingOverrides();
            })
            .OnCancel(operation =>
            {
                Debug.Log("重新绑定取消");
                jumpAction.Enable();
                rebindOperation = null;
            });
        
        Debug.Log("开始重新绑定，请按下新的按键...");
        rebindOperation.Start();
    }
    
    void SaveBindingOverrides()
    {
        if (jumpAction == null||bindWButton==null||bindSpaceButton==null)return;
        // 保存绑定覆盖到PlayerPrefs
        var overrides = new InputBindingOverrideList();
        overrides.GetOverrides(jumpAction);
        
        string json = JsonUtility.ToJson(overrides);
        PlayerPrefs.SetString("JumpActionOverrides", json);
        PlayerPrefs.Save();
        
        Debug.Log("绑定设置已保存");
    }
    
    void LoadBindingOverrides()
    {
        if (jumpAction == null||bindWButton==null||bindSpaceButton==null)return;
        // 从PlayerPrefs加载绑定覆盖
        if (PlayerPrefs.HasKey("JumpActionOverrides"))
        {
            string json = PlayerPrefs.GetString("JumpActionOverrides");
            var overrides = JsonUtility.FromJson<InputBindingOverrideList>(json);
            overrides.ApplyOverrides(jumpAction);
            Debug.Log("绑定设置已加载");
        }
    }
    
    void OnEnable()
    {
        // 组件启用时加载保存的绑定
        if (jumpAction != null)
        {
            LoadBindingOverrides();
        }
    }
    
    void OnDisable()
    {
        // 清理重新绑定操作
        rebindOperation?.Cancel();
        rebindOperation?.Dispose();
    }
}

// 用于序列化绑定覆盖的辅助类
[System.Serializable]
public class InputBindingOverrideList
{
    public string[] overrides = new string[0];
    
    public void GetOverrides(InputAction action)
    {
        overrides = new string[action.bindings.Count];
        for (int i = 0; i < action.bindings.Count; i++)
        {
            overrides[i] = action.bindings[i].overridePath ?? action.bindings[i].path;
        }
    }
    
    public void ApplyOverrides(InputAction action)
    {
        for (int i = 0; i < overrides.Length && i < action.bindings.Count; i++)
        {
            if (!string.IsNullOrEmpty(overrides[i]))
            {
                action.ApplyBindingOverride(i, overrides[i]);
            }
        }
    }
}