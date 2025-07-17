using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MenuParallax : MonoBehaviour
{
    public float offsetMultiplier = 1f;
    public float smoothTime = 0.5f;
    private Vector2 startPosition;
    private Vector2 velocity;
    // Start is called before the first frame update
    void Start()
    {
        startPosition = transform.position;
    }

    // Update is called once per frame
    void Update()
    {
        // 获取鼠标在屏幕上的位置（0-1范围）
        Vector2 mouseViewport = Camera.main.ScreenToViewportPoint(Input.mousePosition);
        
        // 计算偏移量（将0-1范围转换为-0.5到0.5范围，这样鼠标在屏幕中心时偏移为0）
        Vector2 offset = (mouseViewport - new Vector2(0.5f, 0.5f)) * offsetMultiplier;
        
        // 计算目标位置：起始位置 + 偏移量
        Vector2 targetPosition = startPosition + offset;
        
        // 平滑移动到目标位置
        transform.position = Vector2.SmoothDamp(transform.position, targetPosition, ref velocity, smoothTime);

        if(SceneManager.GetActiveScene().name == "Level_11"){
            AchievementManager.Instance.UnlockAchievement("GameOver");
        }
    }
}
