using UnityEngine;
using UnityEngine.SceneManagement;

public class PauseMenuManager : MonoBehaviour
{
    public PlayerBehaviour playerBehaviour; // 关联 PlayerBehaviour 脚本
    public GameObject pausePanel;  // 关联 PausePanel UI
    public GameObject helpPanel;
    private bool isPaused = false;
    public bool isEnd = false;
    void Start()
    {

        playerBehaviour = FindObjectOfType<PlayerBehaviour>(); // 获取 PlayerBehaviour 实例
        // 确保暂停面板初始状态为隐藏
        if (pausePanel != null)
            pausePanel.SetActive(false);
        if (helpPanel != null)
            helpPanel.SetActive(false);
    }
    void Update()
    {
        if (isEnd||pausePanel==null) return; // 如果游戏结束，直接返回
        // 按下 Esc 切换暂停状态
        if (Input.GetKeyDown(KeyCode.Escape)&&(!isEnd))
        {
            if (isPaused)
                ResumeGame();
            else
                PauseGame();
        }
    }

    public void PauseGame()
    {
        Debug.Log("Game Paused");
        if(pausePanel != null)
            pausePanel.SetActive(true);
        if (playerBehaviour != null)
        {
            playerBehaviour.isPaused = true;
            ShadowBehaviour shadowBehaviour = FindObjectOfType<ShadowBehaviour>();
            if (shadowBehaviour != null)
                shadowBehaviour.isPaused = true; // 恢复影子状态   
        }
        Time.timeScale = 0f; // 暂停游戏时间
        isPaused = true;
    }
    public void RestartGame()
    {
        Time.timeScale = 1f; // 恢复时间再重启
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
        NoticeManger.Instance.ShowNotice(false); // 隐藏通知
    }
    public void LoseRestartGame()
    {
        Time.timeScale = 1f; // 恢复时间再重启
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
        NoticeManger.Instance.ShowNotice(false); // 隐藏通知
        AchievementManager.Instance.UnlockAchievement("Restart");
    }
    public void ResumeGame()
    {
        if (pausePanel != null){
            pausePanel.SetActive(false);
        }
        if (playerBehaviour != null)
        {
            playerBehaviour.isPaused = false;
            ShadowBehaviour shadowBehaviour = FindObjectOfType<ShadowBehaviour>();
            if (shadowBehaviour != null)
                shadowBehaviour.isPaused = false; // 恢复影子状态  
        }

        Time.timeScale = 1f; // 恢复时间
        isPaused = false;
    }
    public void LoadLevel(string levelName)
    {
        SceneManager.LoadScene(levelName);
    }
    public void ReturnToMainMenu()
    {
        Time.timeScale = 1f; // 恢复时间再切换场景
        SceneManager.LoadScene("Menu");
    }
    public void ReturnToLevelSelect()
    {
        Time.timeScale = 1f; // 恢复时间再切换场景
        SceneManager.LoadScene("LevelSelectScene");
    }
    public void QuitGame()
    {
        Debug.Log("Quit Game");
        Application.Quit();
    }
    public void SetEndState()
    {
        isEnd = true; // 设置游戏结束状态
    }
    public void ShowHelpPanel()
    {
        helpPanel.SetActive(true);
    }
    public void HideHelpPanel()
    {
        helpPanel.SetActive(false);
    }
}
