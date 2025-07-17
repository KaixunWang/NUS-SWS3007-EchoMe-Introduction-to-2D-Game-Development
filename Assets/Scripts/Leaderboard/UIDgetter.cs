using System.Collections;
using UnityEngine;
using LootLocker.Requests;
using TMPro;

public class UIDgetter : MonoBehaviour
{
    public string playerID; // 公共字段可供外部访问
    public TMP_Text tmpText; // 新增TMP_Text字段
    private static UIDgetter instance;

    void Awake()
    {
        // 实现单例模式 + 防止重复创建
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
            return;
        }

        InitUID();
    }

    void Start()
    {
        if (tmpText == null)
        {
            // 没有绑定TMP，直接返回
            return;
        }
        // 赋值
        tmpText.text = "UID: " + playerID;
    }

    void InitUID()
    {
        // 如果之前已经保存过，就直接用
        if (PlayerPrefs.HasKey("PlayerID"))
        {
            playerID = PlayerPrefs.GetString("PlayerID");
            if (tmpText != null) tmpText.text = "UID: " + playerID; // 赋值到TMP
        }
        else
        {
            // 如果没有保存过ID，显示默认值
            playerID = "未登录";
            if (tmpText != null) tmpText.text = "UID: " + playerID;
        }
    }
    
    // 更新显示的ID（当其他地方登录成功后调用）
    public void UpdatePlayerID(string newID)
    {
        playerID = newID;
        if (tmpText != null) tmpText.text = "UID: " + playerID;
    }
}
