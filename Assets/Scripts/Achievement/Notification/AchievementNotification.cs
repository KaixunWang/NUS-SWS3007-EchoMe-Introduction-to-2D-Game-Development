using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class AchievementNotification : MonoBehaviour
{
    [Header("Simple UI References")]
    public Image backgroundImage;      // 背景
    public Image iconImage;           // 成就图标
    public TextMeshProUGUI nameText;  // 成就名称
    public TextMeshProUGUI scoreText; // 获得积分
    
    [Header("Animation Settings")]
    public float slideInDuration = 0.5f;
    public float displayDuration = 3f;
    public float slideOutDuration = 0.5f;
    public Vector3 hiddenPosition = new Vector3(0, 600, 0);
    public Vector3 visiblePosition = new Vector3(0, 456.7f, 0);
    
    [Header("Audio")]
    public AudioSource audioSource;
    public AudioClip unlockSound;
    
    private RectTransform rectTransform;
    private CanvasGroup canvasGroup;
    
    private void Awake()
    {
        rectTransform = GetComponent<RectTransform>();
        canvasGroup = GetComponent<CanvasGroup>();
        
        if (canvasGroup == null)
        {
            canvasGroup = gameObject.AddComponent<CanvasGroup>();
        }
        
        // 使用Inspector中设置的位置值，不要被当前transform位置覆盖
        // 初始状态：设置到隐藏位置，但保持active，用透明度控制可见性
        rectTransform.anchoredPosition = hiddenPosition;
        canvasGroup.alpha = 0f;
        
        Debug.Log($"通知初始化 - Hidden: {hiddenPosition}, Visible: {visiblePosition}");
    }
    
    public void ShowNotification(Achievement achievement)
    {
        // 强制确保使用正确的位置（用于调试）
        hiddenPosition = new Vector3(0, 600, 0);    // 强制设置为上方
        visiblePosition = new Vector3(0, 456.7f, 0); // 强制设置为你想要的位置
        
        Debug.Log($"显示通知 - Hidden: {hiddenPosition}, Visible: {visiblePosition}");
        
        // 设置UI内容
        if (nameText != null)
            nameText.text = achievement.name;
            
        if (scoreText != null)
            scoreText.text = $"+{achievement.scoreReward}";
            
        if (iconImage != null && achievement.icon != null)
            iconImage.sprite = achievement.icon;
        
        // 播放音效
        if (audioSource != null && unlockSound != null)
        {
            audioSource.PlayOneShot(unlockSound);
        }
        
        // 开始动画
        StartCoroutine(ShowNotificationCoroutine());
    }
    
    private IEnumerator ShowNotificationCoroutine()
    {
        // 不需要SetActive(true)，因为物体已经是激活的
        
        // 确保从隐藏位置开始
        rectTransform.anchoredPosition = hiddenPosition;
        canvasGroup.alpha = 0f;
        
        // 滑入动画 - 同时改变位置和透明度
        float elapsedTime = 0f;
        while (elapsedTime < slideInDuration)
        {
            elapsedTime += Time.deltaTime;
            float t = elapsedTime / slideInDuration;
            t = EaseOutBack(t);
            
            rectTransform.anchoredPosition = Vector3.Lerp(hiddenPosition, visiblePosition, t);
            canvasGroup.alpha = Mathf.Lerp(0f, 1f, t);
            
            yield return null;
        }
        
        rectTransform.anchoredPosition = visiblePosition;
        canvasGroup.alpha = 1f;
        
        // 显示时间
        yield return new WaitForSeconds(displayDuration);
        
        // 滑出动画 - 同时改变位置和透明度
        elapsedTime = 0f;
        while (elapsedTime < slideOutDuration)
        {
            elapsedTime += Time.deltaTime;
            float t = elapsedTime / slideOutDuration;
            
            rectTransform.anchoredPosition = Vector3.Lerp(visiblePosition, hiddenPosition, t);
            canvasGroup.alpha = Mathf.Lerp(1f, 0f, t);
            
            yield return null;
        }
        
        // 最终设置为完全透明，位置回到隐藏处
        rectTransform.anchoredPosition = hiddenPosition;
        canvasGroup.alpha = 0f;
    }
    
    private float EaseOutBack(float t)
    {
        const float c1 = 1.70158f;
        const float c3 = c1 + 1f;
        return 1f + c3 * Mathf.Pow(t - 1f, 3f) + c1 * Mathf.Pow(t - 1f, 2f);
    }
}