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
        
        // 直接控制整个Portals对象
        Transform portals = transform.Find("Portals");
        if (portals != null)
        {
            portals.gameObject.SetActive(active);
            Debug.Log("传送门状态设置为：" + active);
        }
        else
        {
            // 调试信息：打印所有子对象名称
            Debug.LogWarning($"在传送门 {gameObject.name} 中找不到 'Portals' 对象");
            Debug.Log($"传送门 {gameObject.name} 的子对象有：");
            for (int i = 0; i < transform.childCount; i++)
            {
                Transform child = transform.GetChild(i);
                Debug.Log($"  - {child.name}");
            }
        }
    }
    
    // 外部调用的方法
    public void OpenPortal() => SetPortalState(true);
    public void ClosePortal() => SetPortalState(false);
    public void TogglePortal() => SetPortalState(!isActive);
}