using UnityEngine;

public class NoticeManger : MonoBehaviour
{
    public static NoticeManger Instance { get; private set; }
    public int noticeCount = 0; // 通知计数器

    private void Awake()
    {
        // 保证只有一个实例存在
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject); // 销毁多余的副本
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject); // 不随场景销毁
    }
    private void Start()
    {
        // 初始化代码
    }
    private void Update()
    {
        
    }
    public void ShowNotice(bool isShow)
    {
        // 显示通知的逻辑
        if (isShow) return;
        noticeCount++;
    }
    private void OnDisable()
    {

    }
}
