using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro; 

public class SceneTrans : MonoBehaviour
{
    public TMP_Text tipsText; // 引用你的Text组件
    public List<string> tips = new List<string>(); // 存储提示文本的列表
    public CanvasGroup canvasGroup; // 引用CanvasGroup组件
    public GameObject player;
    public float fadeInDuration = 1f; // 淡入持续时间
    public float displayDuration = 3f; // 显示持续时间
    public float fadeOutDuration = 1f; // 淡出持续时间
    
    private SpriteRenderer playerSprite;

    // Start is called before the first frame update
    void Start()
    {
        playerSprite = player.GetComponent<SpriteRenderer>();

        Color color = playerSprite.color;
        color.a = 0f;
        playerSprite.color = color;

        if (canvasGroup == null)
        {
            Debug.LogError("CanvasGroup is null.");
        }
        if (tips.Count > 0)
        {
            // 随机选择一个提示并显示
            string randomTip = tips[Random.Range(0, tips.Count)];
            tipsText.text = "(Tips: " + randomTip + ")";
        }   

        StartCoroutine(FadeInAndOut());
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    IEnumerator FadeInAndOut()
    {
        float elapsed = 0f;

        // 淡入
        while (elapsed < fadeInDuration)
        {
            elapsed += Time.deltaTime;
            canvasGroup.alpha = Mathf.Clamp01(elapsed / fadeInDuration);
            Color color = playerSprite.color;
            color.a = Mathf.Clamp01(elapsed / fadeInDuration);
            playerSprite.color = color;
            yield return null;
        }

        // 保持显示
        yield return new WaitForSeconds(displayDuration);

        elapsed = 0f;

        // 淡出
        while (elapsed < fadeOutDuration)
        {
            elapsed += Time.deltaTime;
            canvasGroup.alpha = 1f - Mathf.Clamp01(elapsed / fadeOutDuration);

            Color color = playerSprite.color;
            color.a = 1f - Mathf.Clamp01(elapsed / fadeOutDuration);
            playerSprite.color = color;

            yield return null;
        }

        canvasGroup.alpha = 0f; // 确保最终透明度为0
    }
}
