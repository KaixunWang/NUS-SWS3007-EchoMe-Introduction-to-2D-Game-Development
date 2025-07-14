using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpikeBehaviour : MonoBehaviour
{
    public int cntMove = 0;
    public bool inMove=false; // 是否开启
    public float moveSpeed = 2f; // 移动速度
    public Vector3 Destination ; // 相对距离
    public Vector3 StartPosition; // 起始位置
    // Start is called before the first frame update
    void Start()
    {

    }
    public void setState(SpikeState state)
    {
        cntMove = state.cntMove;
        inMove = state.inMove;
        Destination = state.Destination;
        StartPosition = state.StartPosition;
        transform.position = state.pos; // 设置当前位置信息
    }
    public SpikeState getState()
    {
        SpikeState state = new SpikeState();
        state.cntMove = cntMove;
        state.inMove = inMove;
        state.Destination = Destination;
        state.StartPosition = StartPosition;
        state.pos = transform.position; // 获取当前位置信息
        return state;
    }

    // Update is called once per frame
    void Update()
    {
        if (inMove)
        {
            // 移动到目标位置
            transform.position = Vector3.MoveTowards(transform.position, Destination, moveSpeed * Time.deltaTime);
            // 检查是否到达目标位置
            if (Vector3.Distance(transform.position, Destination) < 0.01f)
            {
                cntMove--;
                inMove = false; // 停止移动
                Destination = StartPosition; // 重置目标位置为起始位置
                Debug.Log("Spike reached the destination, remaining count: " + cntMove);
            }
        }
        else if (cntMove > 0)
        {
            inMove = true; 
            StartPosition = transform.position; // 记录起始位置
            Debug.Log("Spike begins moving.");
        }
    }
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.name == "Player")
        {
            Debug.Log("Player hit the spike!");
            // 示例调用：玩家掉血
            other.GetComponent<PlayerBehaviour>().TakeDamage("Spike");
        }
        // if (other.name == "Shadow(Clone)")
        // {
        //     Debug.Log("Shadow hit the spike!");
        //     // 示例调用：影子掉血
        //     other.GetComponent<ShadowBehaviour>().DestroybyTrap();
        // }
        if (other.name == "Echo(Clone)")
        {
            Debug.Log("Echo hit the spike!");
            // 示例调用：回声掉血
            other.GetComponent<EchoBehaviour>().TakeDamage("Spike");
            AchievementManager.Instance.UnlockAchievement("EchoSuicide");
        }
    }
}
