using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Portal : MonoBehaviour
{
    [Header("传送门状态")]
    public bool isActive = true;  // 传送门是否激活
    
    [Header("传送门配对")]
    public Portal linkedPortal;
    
    [Header("传送门位置")]
    public Transform sideAPosition;
    public Transform sideBPosition;
    
    [Header("控制的组件")]
    public GameObject[] particleSystems;     // 粒子特效
    public Collider2D[] portalColliders;    // 3个碰撞器
    
    [Header("设置")]
    public float teleportCooldown = 0.5f;
    private static System.Collections.Generic.Dictionary<GameObject, float> lastTeleportTime = 
        new System.Collections.Generic.Dictionary<GameObject, float>();
    
    void Start()
    {
        SetPortalState(isActive);
    }
    
    // 传送逻辑
    public void TeleportPlayer(GameObject player, bool fromASide)
    {
        if (!isActive || linkedPortal == null || !linkedPortal.isActive) return;
        
        // 检查冷却时间
        if (lastTeleportTime.ContainsKey(player) && 
            Time.time - lastTeleportTime[player] < teleportCooldown)
        {
            return;
        }
        
        // 传送
        Transform exitPosition = fromASide ? 
            linkedPortal.sideAPosition : linkedPortal.sideBPosition;
        
        player.transform.position = exitPosition.position;
        lastTeleportTime[player] = Time.time;
    }
    
    // 设置传送门状态
    public void SetPortalState(bool active)
    {
        isActive = active;
        
        // 控制粒子特效
        foreach (GameObject particle in particleSystems)
        {
            if (particle != null)
                particle.SetActive(active);
        }
        
        // 控制碰撞器
        foreach (Collider2D collider in portalColliders)
        {
            if (collider != null)
                collider.enabled = active;
        }
    }
    
    // 外部调用的方法
    public void OpenPortal() => SetPortalState(true);
    public void ClosePortal() => SetPortalState(false);
    public void TogglePortal() => SetPortalState(!isActive);
}