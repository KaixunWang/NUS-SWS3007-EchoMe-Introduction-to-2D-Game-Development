using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PortalSide : MonoBehaviour
{
    [Header("设置")]
    public Portal parentPortal;  // 父传送门引用
    public bool isAside = true;  // 是否是A面
    
    void OnTriggerEnter2D(Collider2D other)
    {
        // 检查是否是玩家
        if (other.CompareTag("Player"))
        {
            // 告诉父传送门有人进入了
            parentPortal.TeleportPlayer(other.gameObject, isAside);
        }
    }
}