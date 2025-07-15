using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Mirror : MonoBehaviour
{
    private Collider2D Shadow = null;
    private Collider2D Echo = null;
    int shadowNumber = 1;
    int echoNumber = 1;
    private bool isleft;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if (Shadow != null) Debug.Log("shadow position" + Shadow.gameObject.transform.position.x + "trigger position" + transform.position.x);
        if (shadowNumber == 1 && Shadow != null && (Shadow.gameObject.transform.position.x > transform.position.x ^ isleft))
            HandleShadow(Shadow);
        if (echoNumber == 1 && Echo != null && (Echo.gameObject.transform.position.x > transform.position.x ^ isleft))
            HandleEcho(Echo);
    }

    void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("shadow"))
        {
            Shadow = collision;
            collision.GetComponent<ShadowBehaviour>().mirror = this;
            if (Shadow.gameObject.transform.position.x < transform.position.x)
            {
                isleft = true;
            }
            else
            {
                isleft = false;
            }
        }
        if (collision.CompareTag("echo"))
        {
            Echo = collision;
            collision.GetComponent<EchoBehaviour>().mirror = this;
            if (Echo.gameObject.transform.position.x < transform.position.x)
            {
                isleft = true;
            }
            else
            {
                isleft = false;
            }
        }
    }

    public void reset()
    {
        shadowNumber = 1;
        echoNumber = 1;
    }

    void HandleShadow(Collider2D collision){
        shadowNumber--;
        GameObject shadow_mirror = Resources.Load<GameObject>("Prefab/ShadowMirror");
        shadow_mirror.tag = "shadow";
        shadow_mirror = Instantiate(shadow_mirror, collision.transform.position, Quaternion.identity);
        Vector3 originalPos = collision.gameObject.transform.position;
        Vector3 mirroredPos = new Vector3(
            2 * transform.position.x - originalPos.x, // X 轴镜像
            originalPos.y, // Y 保持不变
            originalPos.z  // Z 保持不变
        );

        shadow_mirror.transform.position = mirroredPos;

        ShadowBehaviour shadow_behaviour = collision.GetComponent<ShadowBehaviour>();
        ShadowBehaviour shadow_mirror_behaviour = shadow_mirror.GetComponent<ShadowBehaviour>();

        shadow_mirror_behaviour.setMirrored(true); // 设置镜像状态
        shadow_behaviour.oppShadow = shadow_mirror; // 设置镜像影子
        shadow_mirror_behaviour.oppShadow = collision.gameObject; // 设置镜像影子的原影
        // 设置镜像影子剩余时间
        shadow_mirror_behaviour.shadowDuration = shadow_behaviour.shadowDuration - (Time.time - shadow_behaviour.getRecordStartTime());

        // 触发镜子效果
        Debug.Log("Shadow entered the mirror area.");
        // 在这里添加镜子效果的逻辑
    }

    void HandleEcho(Collider2D collision)
    {
        echoNumber--;
        // 触发玩家进入镜子区域的逻辑
        GameObject echo_mirror = Resources.Load<GameObject>("Prefab/EchoMirror");
        echo_mirror.tag = "echo";
        echo_mirror = Instantiate(echo_mirror, collision.transform.position, Quaternion.identity);
        Vector3 originalPos = collision.gameObject.transform.position;
        Vector3 mirroredPos = new Vector3(
            2 * transform.position.x - originalPos.x, // X 轴镜像
            originalPos.y, // Y 保持不变
            originalPos.z  // Z 保持不变
        );

echo_mirror.transform.position = mirroredPos;

        EchoBehaviour echo_mirror_Behaviour = echo_mirror.GetComponent<EchoBehaviour>();
        EchoBehaviour echo_Behaviour = collision.GetComponent<EchoBehaviour>();

        echo_Behaviour.oppEcho = echo_mirror;
        echo_mirror_Behaviour.oppEcho = collision.gameObject;

        echo_mirror_Behaviour.setMirrored(true); // 设置镜像状态
        // 传递输入事件
        echo_mirror_Behaviour.SetInputEvents(echo_Behaviour.getInputEvents());
        // 传递当前事件索引
        echo_mirror_Behaviour.SetCurrentEventIndex(echo_Behaviour.getCurrentEventIndex());
        // 设置当前状态
        echo_mirror_Behaviour.setCurrentInputStates(echo_Behaviour.getCurrentInputStates());
        // 设置镜像回放开始时间，与原影子相同
        echo_mirror_Behaviour.setReplayStartTime(echo_Behaviour.getReplayStartTime()); // 设置回放开始时间
        // 设置镜像回音剩余时间
        echo_mirror_Behaviour.echoDuration = echo_Behaviour.echoDuration - (Time.time - echo_Behaviour.getReplayStartTime());

        Debug.Log("Player entered the mirror area.");
        // 在这里添加玩家进入镜子区域的逻辑
    }
}
