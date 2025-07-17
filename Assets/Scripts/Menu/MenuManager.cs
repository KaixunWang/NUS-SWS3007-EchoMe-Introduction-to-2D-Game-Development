using UnityEngine;
using UnityEngine.SceneManagement;
using LootLocker.Requests;
using System.Collections;
using System.Collections.Generic;
using TMPro;

public class MenuManager : MonoBehaviour
{
    public GameObject achievementPanel;
    public GameObject optionsPanel;
    public TMP_InputField usernameInput;
    public GameObject leaderboardPanel;
    public void Start()
    {
        // 初始化菜单或其他设置
        Debug.Log("MenuManager initialized");
    }

    // 移除LoginRoutine方法

    // 调用这个函数用于设置名字
    public void SetPlayerName(){
        LootLockerSDKManager.SetPlayerName(usernameInput.text, (response) =>{
            if (response.success)
            {
                Debug.Log("Set player name successful");
            }
            else
            {
                Debug.LogError("Set player name failed: " + response.errorData.ToString());
            }
        });
    }

    // 退出游戏（打包后有效）
    public void QuitGame()
    {
        Debug.Log("Quit Game");
        Application.Quit();
    }
    
    public void PlayGame()
    {
        SceneManager.LoadScene("LevelSelectScene");
    }
    
    // 返回主菜单（可用于暂停菜单中）
    public void ReturnToMainMenu()
    {
        SceneManager.LoadScene("Menu");
    }
    
    // 显示成就菜单
    public void ShowAchievements()
    {
        Debug.Log("显示成就菜单");
        //将achievementpanel设置为active
        achievementPanel.SetActive(true);
    }

    public void CloseAchievements()
    {
        achievementPanel.SetActive(false);
    }

    public void ShowOptions()
    {
        optionsPanel.SetActive(true);
    }

    public void CloseOptions()
    {
        optionsPanel.SetActive(false);
    }

    public void ShowLeaderboard()
    {
        leaderboardPanel.SetActive(true);
    }

    public void CloseLeaderboard()
    {
        leaderboardPanel.SetActive(false);
    }

    public void OpenLeaderboard()
    {
        LeaderboardUpdater updater = FindObjectOfType<LeaderboardUpdater>();
        if (updater != null)
        {
            updater.ShowLeaderboard();
        }
        else
        {
            Debug.LogWarning("LeaderboardUpdater 未找到");
        }
    }
}
