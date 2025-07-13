using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BoxBehavior : MonoBehaviour
{

     public AudioSource boxAudioSource; // 音频源，用于播放箱子打开音效

    private bool isGrounded = false; // 标记箱子是否在地面上

    void Start()
    {
        // 初始化为 true，假设箱子开始时在地面上
        isGrounded = true;
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    void OnCollisionEnter2D(Collision2D collision)
    {
        // 检查碰撞对象是否是地面，并且箱子之前不在地面上
        if (collision.gameObject.layer == 7 && !isGrounded)
        {
            if (boxAudioSource != null && !boxAudioSource.isPlaying)
            {
                boxAudioSource.Play(); // 播放箱子落地音效
            }
            isGrounded = true; // 更新状态为已着陆
        }
    }

    // 可选：当箱子离开地面时更新状态（例如被角色推动到空中）
    void OnCollisionExit2D(Collision2D collision)
    {
        if (collision.gameObject.layer == 7)
        {
            isGrounded = false; // 箱子离开地面
        }
    }
}
