using System.Collections.Generic;
using UnityEngine;

public class AchievementNotificationManager : MonoBehaviour
{
    public static AchievementNotificationManager Instance;
    
    [Header("Notification Settings")]
    public AchievementNotification notificationPrefab;
    public Canvas notificationCanvas; // 专用的通知Canvas，跨场景保持
    public int maxSimultaneousNotifications = 3;
    public float verticalSpacing = 300f;
    
    private Queue<Achievement> notificationQueue = new Queue<Achievement>();
    private List<AchievementNotification> activeNotifications = new List<AchievementNotification>();
    
    private void Awake()
    {
        // 单例模式 - 跨场景保持
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            
            // 确保通知Canvas也跨场景保持
            if (notificationCanvas != null)
            {
                DontDestroyOnLoad(notificationCanvas.gameObject);
            }
            
            // 订阅成就解锁事件
            AchievementManager.OnAchievementUnlocked += OnAchievementUnlocked;
            
            Debug.Log("AchievementNotificationManager 初始化完成，支持跨场景通知");
        }
        else
        {
            Destroy(gameObject);
        }
    }
    
    private void OnDestroy()
    {
        // 取消订阅
        AchievementManager.OnAchievementUnlocked -= OnAchievementUnlocked;
    }
    
    private void OnAchievementUnlocked(Achievement achievement)
    {
        Debug.Log($"收到成就解锁通知: {achievement.name}");
        ShowNotification(achievement);
    }
    
    public void ShowNotification(Achievement achievement)
    {
        // 确保有Canvas用于显示通知
        if (notificationCanvas == null)
        {
            Debug.LogError("通知Canvas未设置！无法显示成就通知");
            return;
        }
        
        // 如果当前显示的通知数量已达上限，加入队列
        if (activeNotifications.Count >= maxSimultaneousNotifications)
        {
            notificationQueue.Enqueue(achievement);
            return;
        }
        
        // 创建通知实例
        GameObject notificationObj = Instantiate(notificationPrefab.gameObject, notificationCanvas.transform);
        AchievementNotification notification = notificationObj.GetComponent<AchievementNotification>();
        
        // 计算这个通知的固定位置（不会因为其他通知消失而改变）
        float yOffset = -activeNotifications.Count * verticalSpacing;
        
        // 设置固定的显示和隐藏位置
        Vector3 baseVisible = new Vector3(0, 456.7f, 0);
        Vector3 baseHidden = new Vector3(0, 600, 0);
        
        notification.visiblePosition = baseVisible + new Vector3(0, yOffset, 0);
        notification.hiddenPosition = baseHidden + new Vector3(0, yOffset, 0);
        
        // 添加到活跃列表
        activeNotifications.Add(notification);
        
        // 显示通知
        notification.ShowNotification(achievement);
        
        Debug.Log($"显示成就通知: {achievement.name} 位置: {notification.visiblePosition}");
        
        // 4秒后移除
        StartCoroutine(RemoveNotificationAfterDelay(notification, 4f));
    }
    
    private System.Collections.IEnumerator RemoveNotificationAfterDelay(AchievementNotification notification, float delay)
    {
        yield return new WaitForSeconds(delay);
        
        // 从活跃列表中移除
        if (activeNotifications.Contains(notification))
        {
            activeNotifications.Remove(notification);
        }
        
        // 销毁通知对象
        if (notification != null)
        {
            Destroy(notification.gameObject);
        }
        
        // 处理队列中的通知
        if (notificationQueue.Count > 0)
        {
            Achievement nextAchievement = notificationQueue.Dequeue();
            ShowNotification(nextAchievement);
        }
    }
}