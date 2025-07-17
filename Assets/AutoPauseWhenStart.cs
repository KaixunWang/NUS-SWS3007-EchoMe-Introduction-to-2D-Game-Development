using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AutoPauseWhenStart : MonoBehaviour
{
    public PauseMenuManager pauseMenuManager;
    // Start is called before the first frame update
    void Start()
    {
        pauseMenuManager.PauseGameWithPopUp();
        if (NoticeManger.Instance != null)
        {
            if (NoticeManger.Instance.noticeCount > 0)
            {
                NoticeManger.Instance.noticeCount--; // 重置通知计数
                gameObject.SetActive(false); // 隐藏当前对象
                pauseMenuManager.ResumeGame(); // 恢复游戏
            }
        }
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
