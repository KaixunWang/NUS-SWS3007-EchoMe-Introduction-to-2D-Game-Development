using System.Collections;
using System.Collections.Generic;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.SceneManagement;
#endif

public class BoardBehavior : MonoBehaviour
{
    public SpriteRenderer spriteRenderer;
    public Sprite spriteOpened;
    public Sprite spriteClosed;
    public Cainos.PixelArtPlatformer_Dungeon.Door door = null;
    public Portal portal = null;
    public List<GameObject> spikes;
    private float targetY = 0f;
    private float originY = 0f;
    public float speed = 0.1f; // 刺上升或下降的速度
    private int count = 0;


    private Animator Animator
    {
        get
        {
            if (animator == null ) animator = GetComponent<Animator>();
            return animator;
        }
    }
    private Animator animator;

    public bool IsOpened
    {
        get { return isOpened; }
        set
        {
            isOpened = value;

            #if UNITY_EDITOR
            if (Application.isPlaying == false)
            {
                EditorUtility.SetDirty(this);
                EditorSceneManager.MarkSceneDirty(gameObject.scene);
            }
            #endif


            if (Application.isPlaying)
            {
                Animator.SetBool("IsOpened", isOpened);
            }
            else
            {
                if(spriteRenderer) spriteRenderer.sprite = isOpened ? spriteOpened : spriteClosed;
            }
        }
    }
    [SerializeField,HideInInspector]
    private bool isOpened;

    public void TriggerDoor() {
        if (IsOpened && door != null)
        {
            Debug.Log("Switch: Open the door");
            door.SetDoor(true);
            if (door.tag == "gate")
            {
                door.SetGate(true); // 确保门的状态被正确设置
            }
        }
        else if (!IsOpened && door != null)
        {
            Debug.Log("Switch: Close the door");
            door.SetDoor(false);
            if(door.tag == "gate")
            {
                door.SetGate(false); // 确保门的状态被正确设置
            }
        }
    }

    // Start is called before the first frame update
    void Start()
    {
        Animator.Play(isOpened ? "Opened" : "Closed");
        IsOpened = isOpened;
        if(spikes.Count != 0)
        {
            originY = spikes[0].transform.position.y;
            targetY = originY - 0.8f; // 假设刺下降1个单位
        }
    }
    void OnTriggerEnter2D(Collider2D other)
    {
        count++;
        IsOpened = true;
        TriggerDoor(); // 触发门的开关
        if (portal != null)
        {
            portal.SetPortalState(true);
        }
        if(spikes.Count != 0)
        {
            LowerSpikes();
        }
    }
    void OnTriggerExit2D(Collider2D other)
    {
        count--;
        if (count <= 0)
        {
            IsOpened = false; // 切换门的开关状态
            TriggerDoor(); // 触发门的开关
            if (portal != null)
            {
                portal.SetPortalState(false);
            }
        }
        if( spikes.Count != 0)
        {
            RaiseSpikes();
        }  
    }
    void LowerSpikes()
    {
        foreach (GameObject spike in spikes)
        {

            spike.transform.position = new Vector3(spike.transform.position.x, targetY, spike.transform.position.z);

        }
    }
    void RaiseSpikes()
    {
        foreach (GameObject spike in spikes)
        {
            spike.transform.position = new Vector3(spike.transform.position.x, originY, spike.transform.position.z);

        }
    }
    public bool GetBoardState()
    {
        return isOpened;
    }
    public void SetBoardState(bool state)
    {
        isOpened = state;
    }
}
