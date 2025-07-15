using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Controls;

public class InputManager : MonoBehaviour
{
    public static InputManager Instance;
    public EchoMeInput _controls;
    public EchoMeInput controls { get { return _controls; } }
    // PlayerPrefs 保存键路径的 Key 名字
    private const string JumpBindingKey = "JumpBinding";
    private const string LeftBindingKey = "LeftBinding";
    private const string RightBindingKey = "RightBinding";

    // 交互键（E）检测
    public bool IsInteractPressed() => _controls != null && _controls.GamePlay.Interact.WasPressedThisFrame();
    // 召唤/中断键（G）检测
    public bool IsInterruptPressed() => _controls != null && _controls.GamePlay.Interrupt.WasPressedThisFrame();
    // R键（echo）检测
    public bool IsEchoPressed() => _controls != null && _controls.GamePlay.echo != null && _controls.GamePlay.echo.WasPressedThisFrame();

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            _controls = new EchoMeInput();
            LoadBindings();
            controls.Enable();
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void LoadBindings()
    {
        if (PlayerPrefs.HasKey(JumpBindingKey))
        {
            string path = PlayerPrefs.GetString(JumpBindingKey);
            _controls.GamePlay.Jump.ApplyBindingOverride(0, path);
        }
        if (PlayerPrefs.HasKey(LeftBindingKey))
        {
            string path = PlayerPrefs.GetString(LeftBindingKey);
            _controls.GamePlay.Left.ApplyBindingOverride(0, path);
        }
        if (PlayerPrefs.HasKey(RightBindingKey))
        {
            string path = PlayerPrefs.GetString(RightBindingKey);
            _controls.GamePlay.Right.ApplyBindingOverride(0, path);
        }
    }

    private void SaveBinding(string key, string bindingPath)
    {
        PlayerPrefs.SetString(key, bindingPath);
        PlayerPrefs.Save();
    }

    // 运行时改键接口示例
    public void RebindJump(Key newKey)
    {
        
        string path = $"<Keyboard>/{newKey.ToString().ToLower()}";
        _controls.GamePlay.Jump.ApplyBindingOverride(0, path);
        SaveBinding(JumpBindingKey, path);
        Debug.Log($"Jump key rebound to {path}");
    }

    public void RebindLeft(Key newKey)
    {
        string path = $"<Keyboard>/{newKey.ToString().ToLower()}";
        _controls.GamePlay.Left.ApplyBindingOverride(0, path);
        SaveBinding(LeftBindingKey, path);
        Debug.Log($"Left key rebound to {path}");
    }
    public void RebindJumptoW()
    {
        RebindJump(Key.W);
    }
    public void RebindJumptoSpace()
    {
        RebindJump(Key.Space);
    }
    public void RebindRight(Key newKey)
    {
        string path = $"<Keyboard>/{newKey.ToString().ToLower()}";
        _controls.GamePlay.Right.ApplyBindingOverride(0, path);
        SaveBinding(RightBindingKey, path);
        Debug.Log($"Right key rebound to {path}");
    }

    // 查询输入状态
    public bool IsJumpPressed()
    {
        return _controls.GamePlay.Jump.WasPressedThisFrame();
    }

    public bool IsLeftPressed()
    {
        return _controls.GamePlay.Left.IsPressed();
    }

    public bool IsRightPressed()
    {
        return _controls.GamePlay.Right.IsPressed();
    }
}
