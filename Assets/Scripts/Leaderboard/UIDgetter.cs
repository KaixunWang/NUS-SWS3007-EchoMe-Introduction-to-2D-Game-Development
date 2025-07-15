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

        StartCoroutine(InitUID());
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

    IEnumerator InitUID()
    {
        // 如果之前已经保存过，就直接用
        if (PlayerPrefs.HasKey("PlayerID"))
        {
            playerID = PlayerPrefs.GetString("PlayerID");
            if (tmpText != null) tmpText.text = "UID: " + playerID; // 赋值到TMP
            yield break;
        }

        // 否则初始化 LootLocker 并获取 UID
        bool done = false;

        LootLockerSDKManager.StartGuestSession((response) =>
        {
            if (response.success)
            {
                playerID = response.player_id.ToString();
                PlayerPrefs.SetString("PlayerID", playerID);
                PlayerPrefs.Save();
                Debug.Log("Initialized and saved UID: " + playerID);
                if (tmpText != null) tmpText.text = "UID: " + playerID; // 赋值到TMP
            }
            else
            {
                Debug.LogError("Failed to start session: " + response.errorData.message);
            }
            done = true;
        });

        yield return new WaitWhile(() => !done);
    }
}
