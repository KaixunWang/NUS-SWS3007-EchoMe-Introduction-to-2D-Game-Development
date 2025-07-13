using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Coin : MonoBehaviour
{
    public CoinSystemScript coinSystem; // 引用 CoinSystemScript 脚本
    public float swingAmount = 0.2f;  // 摆动的幅度
    public float swingSpeed = 2f;  // 摆动的速度
    private Vector3 startPosition;
    // Start is called before the first frame update
    void Start()
    {
        // 确保 coinSystem 已经被赋值
        if (coinSystem == null)
        {
            coinSystem = FindObjectOfType<CoinSystemScript>();
            if (coinSystem == null)
            {
                Debug.LogError("CoinSystemScript not found in the scene!");
            }
        }
        startPosition = transform.position;
    }

    // Update is called once per frame
    void Update()
    {
        float yOffset = Mathf.Sin(Time.time * swingSpeed) * swingAmount;
        transform.position = new Vector3(startPosition.x, startPosition.y + yOffset, startPosition.z);
    }
    void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject.name == "Player" || collision.gameObject.name == "Echo(Clone)")
        {
            // 增加分数或执行其他逻辑
            Debug.Log("Coin collected!");
            coinSystem.AddCoin(); // 调用 CoinSystemScript 的 AddCoin 方法
            // 存入PlayerPrefs当前+1
            PlayerPrefs.SetInt("CoinCount", PlayerPrefs.GetInt("CoinCount") + 1);
            PlayerPrefs.Save();
            //发出事件，成就管理器获取playerPrefs中的coinCount，检查是否达到新的里程碑
            AchievementManager.Instance.CheckCoinMilestones();

            Destroy(gameObject); // 收集后销毁硬币
        }
    }
}
