using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// public class EchoBehaviour : MonoBehaviour
// {
//     //输入是模拟的list，list代表每帧的各个按键状况, list对应这一帧的W A D E G是否按下
//     public List<bool[]> simulatedInputs; // 每帧的输入，bool[0]=W, bool[1]=A, bool[2]=D, bool[3]=E, bool[4]=G
//     public BeaconBehaviour beaconBehaviour;
//     private int currentFrame = 0;
//     private Rigidbody2D rb;
//     private Animator animator;
//     private float moveSpeed = 6f;
//     private float jumpForce = 10f;
//     private bool isGrounded = false;
//     private bool lastWInput = false;
//     private bool lastEInput = false;
//     private bool lastGInput = false;
//     private float echoDuration = 10f;
//     // Start is called before the first frame update
//     void Start()
//     {
//         rb = GetComponent<Rigidbody2D>();
//         animator = GetComponent<Animator>();
//         // simulatedInputs 需要在外部赋值
//         StartCoroutine(DestroyAfterTime());
//     }

//     // Update is called once per frame
//     void Update()
//     {
        
        
//     }
//     void FixedUpdate(){
//         isGrounded = CheckGrounded();
//         if(isGrounded){
//             animator.SetBool("IsJumping", false);
//         }
//         if (simulatedInputs == null || currentFrame >= simulatedInputs.Count) return;
//         var input = simulatedInputs[currentFrame];

//         // 移动
//         float moveInput = 0f;
//         if (input[1]) // A
//         {
//             moveInput = -1f;
//             animator.SetBool("IsWalking", true);
//             transform.localScale = new Vector3(-3, 3, 1);
//         }
//         if (input[2]) // D
//         {
//             moveInput = 1f;
//             animator.SetBool("IsWalking", true);
//             transform.localScale = new Vector3(3, 3, 1);
//         }
//         if (!input[1] && !input[2]) // 既没有A也没有D
//         {
//             animator.SetBool("IsWalking", false);
//         }

//         rb.velocity = new Vector2(moveInput * moveSpeed, rb.velocity.y);

//         // 跳跃用input[0]模拟key(W), 只在keydown时触发
//         if (input[0] && isGrounded)
//         {
//             animator.SetBool("IsJumping", true);
//             rb.AddForce(Vector2.up * jumpForce, ForceMode2D.Impulse);
//         }

//         // 使用交互物品（E键keydown）
//         if (IsEKeyDown(input))
//         {
//            Interact();
//         }

//         if (IsGKeyDown(input))
//         {
//             beaconBehaviour.SetHasEcho(false);
//             Destroy(gameObject);
//         }

//         currentFrame++;
//     }

//     public bool CheckGrounded(){
//         //3raycast
//         Collider2D col = GetComponent<Collider2D>();
//         float colliderWidth = 0.8f;
//         float colliderHeight = 0.5f;
//         Vector3 basePos = col.bounds.center + Vector3.down * (colliderHeight / 2f - 0.01f);
//         Vector3 left = basePos + Vector3.left * (colliderWidth / 2f - 0.05f);
//         Vector3 center = basePos;
//         Vector3 right = basePos + Vector3.right * (colliderWidth / 2f - 0.05f);
//         float groundCheckDistance = 0.65f;
//         int groundLayer = LayerMask.GetMask("Ground");
//         return Physics2D.Raycast(left, Vector2.down, groundCheckDistance, groundLayer) ||
//                Physics2D.Raycast(center, Vector2.down, groundCheckDistance, groundLayer) ||
//                Physics2D.Raycast(right, Vector2.down, groundCheckDistance, groundLayer);
//     }

//     private bool IsNearBeacon()
//     {
//         // Implementation of IsNearBeacon method
//         return false; // Placeholder return, actual implementation needed
//     }

//     private void SwitchShadow()
//     {
//         // Implementation of SwitchShadow method
//     }

//     // 检测W键keydown
//     private bool IsWKeyDown(bool[] input)
//     {
//         bool result = input[0] && !lastWInput;
//         lastWInput = input[0];
//         return result;
//     }

//     // 检测E键keydown
//     private bool IsEKeyDown(bool[] input)
//     {
//         bool result = input[3] && !lastEInput;
//         lastEInput = input[3];
//         return result;
//     }

//     private bool IsGKeyDown(bool[] input)
//     {
//         bool result = input[4] && !lastGInput;
//         lastGInput = input[4];
//         return result;
//     }
//     private void Interact()
//     {
//         //开关

//     }

//     IEnumerator DestroyAfterTime()
//     {
//         yield return new WaitForSeconds(echoDuration);
//         beaconBehaviour.SetHasEcho(false);
//         Destroy(gameObject);
//     }
// }
// 改进的Echo重播系统
public class EchoBehaviour : MonoBehaviour
{
    [SerializeField] private Rigidbody2D rb;
    [SerializeField] private Animator animator;
    private float moveSpeed = 6f;
    private float jumpForce = 13f;
    public float echoDuration = 10f;
    
    // 新增：音效组件
    public AudioSource footstepAudioSource;
    public AudioSource jumpAudioSource;
    public AudioSource deadAudioSource;
    
    private List<TimeBasedInputEvent> inputEvents;
    private int currentEventIndex = 0;
    [SerializeField]private float replayStartTime;
    private Dictionary<InputType, bool> currentInputStates;
    private bool isGrounded = false;

    // Start is called before the first frame update

    private bool isNearSwitch = false;
    private Cainos.PixelArtPlatformer_Dungeon.Switch switchObject = null;
    private BeaconBehaviour beaconBehaviour;

    private bool isMirrored = false; // 是否镜像
    public GameObject oppEcho = null;
    public Mirror mirror = null;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();

        // 初始化输入状态
        if (!isMirrored) currentInputStates = new Dictionary<InputType, bool>
        {
            {InputType.W, false},
            {InputType.A, false},
            {InputType.D, false},
            {InputType.E, false},
            {InputType.G, false}
        };

        if (!isMirrored) replayStartTime = Time.time;

        // 确保BeaconBehaviour已设置
        if (beaconBehaviour != null)
        {
            echoDuration = beaconBehaviour.GetEchoTime(); // 从BeaconBehaviour获取回声持续时间
        }

        StartCoroutine(DestroyAfterTime());
    }

    void Update()
    {
        isGrounded = CheckGrounded();
        if (isGrounded)
        {
            animator.SetBool("IsJumping", false);
        }

        // 处理基于时间的输入重播
        ProcessTimeBasedInput();
        // 根据当前输入状态执行动作
        ExecuteCurrentInputs();
        // 走路音效控制
        if ((currentInputStates[InputType.A] || currentInputStates[InputType.D]) && isGrounded)
        {
            if (footstepAudioSource != null && !footstepAudioSource.isPlaying)
            {
                footstepAudioSource.Play();
            }
        }
        else
        {
            if (footstepAudioSource != null)
            {
                footstepAudioSource.Stop();
            }
        }
    }
    
    void FixedUpdate()
    {
        // 计算移动输入
        float moveInput = 0f;
        if (currentInputStates[InputType.A])
        {
            moveInput = -1f;
            if (isMirrored) moveInput = 1f; // 如果是镜像，反转移动输入
        }
        else if (currentInputStates[InputType.D])
        {
            moveInput = 1f;
            if (isMirrored) moveInput = -1f; // 如果是镜像，反转移动输入
        }
        
        rb.velocity = new Vector2(moveInput * moveSpeed, rb.velocity.y);
    }
    
    public void setMirrored(bool mirrored)
    {
        isMirrored = mirrored;
    }
    public List<TimeBasedInputEvent> getInputEvents()
    {
        return inputEvents;
    }
    public void SetCurrentEventIndex(int index)
    {
        if (index < 0 || index >= inputEvents.Count)
        {
            Debug.LogError("Index out of bounds for input events.");
            return;
        }
        currentEventIndex = index;
    }

    public int getCurrentEventIndex()
    {
        return currentEventIndex;
    }

    public void setReplayStartTime(float startTime)
    {
        replayStartTime = startTime;
    }

    public float getReplayStartTime()
    {
        return replayStartTime;
    }   

    public void setCurrentInputStates(Dictionary<InputType, bool> states)
    {
        currentInputStates = states;
    }

    public Dictionary<InputType, bool> getCurrentInputStates()
    {
        return currentInputStates;
    }

    void ProcessTimeBasedInput()
    {
        if (inputEvents == null || currentEventIndex >= inputEvents.Count) return;
        
        float currentTime = Time.time - replayStartTime;
        
        // 处理所有应该在当前时间触发的事件
        while (currentEventIndex < inputEvents.Count && 
               inputEvents[currentEventIndex].timestamp <= currentTime)
        {
            var inputEvent = inputEvents[currentEventIndex];
            
            // 更新输入状态
            currentInputStates[inputEvent.inputType] = inputEvent.isPressed;
            // 处理特殊的KeyDown事件
            if (inputEvent.isPressed)
            {
                HandleKeyDown(inputEvent.inputType);
            }
            
            currentEventIndex++;
        }
    }
    
    void HandleKeyDown(InputType inputType)
    {
        switch (inputType)
        {
            case InputType.W:
                if (isGrounded)
                {
                    animator.SetBool("IsJumping", true);
                    rb.AddForce(Vector2.up * jumpForce, ForceMode2D.Impulse);
                    if (jumpAudioSource != null)
                    {
                        jumpAudioSource.Play();
                    }
                }
                break;
            case InputType.E:
                Interact();
                break;
            case InputType.G:
                StartCoroutine(PlayDeadAndDestroy());
                break;
        }
    }
    
    void ExecuteCurrentInputs()
    {
        // 移动和动画
        if (currentInputStates[InputType.A])
        {
            animator.SetBool("IsWalking", true);
            transform.localScale = new Vector3(-3, 3, 1);
            if (isMirrored) transform.localScale = new Vector3(3, 3, 1); // 如果是镜像，反转方向
        }
        else if (currentInputStates[InputType.D])
        {
            animator.SetBool("IsWalking", true);
            transform.localScale = new Vector3(3, 3, 1);
            if (isMirrored) transform.localScale = new Vector3(-3, 3, 1); // 如果是镜像，反转方向
        }
        else
        {
            animator.SetBool("IsWalking", false);
        }
    }
    
    public bool CheckGrounded()
    {
        //3raycast
        Collider2D col = GetComponent<Collider2D>();
        float colliderWidth = 0.7f;
        float colliderHeight = 0.5f;
        Vector3 basePos = col.bounds.center + Vector3.down * (colliderHeight / 2f - 0.01f);
        Vector3 left = basePos + Vector3.left * (colliderWidth / 2f - 0.05f);
        Vector3 center = basePos;
        Vector3 right = basePos + Vector3.right * (colliderWidth / 2f - 0.05f);
        float groundCheckDistance = 0.65f;
        //boxLayer is fine too
        int groundLayer = LayerMask.GetMask("Ground");
        int itemCanJumpLayer = LayerMask.GetMask("ItemCanJump");
        Debug.DrawRay(left, Vector2.down * groundCheckDistance, Color.red);
        Debug.DrawRay(center, Vector2.down * groundCheckDistance, Color.green);
        Debug.DrawRay(right, Vector2.down * groundCheckDistance, Color.blue);
        return Physics2D.Raycast(left, Vector2.down, groundCheckDistance, groundLayer) ||
               Physics2D.Raycast(center, Vector2.down, groundCheckDistance, groundLayer) ||
               Physics2D.Raycast(right, Vector2.down, groundCheckDistance, groundLayer) ||
               Physics2D.Raycast(left, Vector2.down, groundCheckDistance, itemCanJumpLayer) ||
               Physics2D.Raycast(center, Vector2.down, groundCheckDistance, itemCanJumpLayer) ||
               Physics2D.Raycast(right, Vector2.down, groundCheckDistance, itemCanJumpLayer);
    }
    
    public void SetInputEvents(List<TimeBasedInputEvent> events)
    {
        inputEvents = events;
    }
    
    public void SetBeaconBehaviour(BeaconBehaviour beacon)
    {
        beaconBehaviour = beacon;
    }
    
    private void Interact()
    {
        //开关
        if (isNearSwitch && switchObject != null)
        {
            //switchObject.IsOn = !switchObject.IsOn; // 切换开关状态
            switchObject.TriggerSwitch(); // 触发开关
            if (switchObject.targetPlatform != null && switchObject.targetPlatform.tag == "MovingPlatform")
            {
                switchObject.targetPlatform.RemainingCount++; // 设置剩余前进路径点数量为1
            }
            else if (switchObject.targetSpike != null)
            {
                switchObject.targetSpike.cntMove+=2;
            }else if(switchObject.targetPortal != null){
                bool isActive = switchObject.targetPortal.isActive;
                switchObject.targetPortal.SetPortalState(!isActive);
            }

        }
    }
    
    IEnumerator DestroyAfterTime()
    {
        yield return new WaitForSeconds(echoDuration);
        StartCoroutine(PlayDeadAndDestroy());
    }

    // 新增：死亡音效协程
    IEnumerator PlayDeadAndDestroy()
    {
        // 等待一小段时间让音效开始播放
        yield return new WaitForSeconds(0.1f);
        
        if (!isMirrored && beaconBehaviour != null)
        {
            beaconBehaviour.SetHasEcho(false);
            beaconBehaviour.restore();
        }
        
        // 等待音效播放完成后再销毁对象
        if (deadAudioSource != null && deadAudioSource.clip != null)
        {
            yield return new WaitForSeconds(deadAudioSource.clip.length);
        }
        else
        {
            // 如果没有音效文件，等待0.5秒
            yield return new WaitForSeconds(0.5f);
        }
        if (oppEcho != null)
            Destroy(oppEcho);
        Destroy(gameObject);
        if (mirror != null) mirror.reset();
    }
    public void DestroyImmediate()
    {
        if (!isMirrored && beaconBehaviour != null)
        {
            beaconBehaviour.SetHasEcho(false);
            beaconBehaviour.restore();
        }  
        if (oppEcho != null)
        {
            Destroy(oppEcho);
        }
        if (mirror != null) mirror.reset();
        
        Destroy(gameObject);
        
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.gameObject.CompareTag("MovingPlatform"))
        {
            jumpForce = 14f; // 设置跳跃力
            rb.gravityScale = 0f; // 禁用重力
            Debug.Log("Player entered MovingPlatform");
        }
        if (other.gameObject.tag == "switch")
        {
            Debug.Log("Player is near Switch");
            isNearSwitch = true;
            switchObject = other.gameObject.GetComponent<Cainos.PixelArtPlatformer_Dungeon.Switch>();
        }
        // if (other.gameObject.name == "Board")
        // {
        //     Debug.Log("Player is near Board");
        //     BoardBehavior board = other.gameObject.GetComponent<BoardBehavior>();
        //     board.IsOpened = true; // 切换门的开关状态
        //     if (board != null)
        //     {
        //         board.TriggerDoor(); // 触发门的开关
        //     }
        // }
    }

    void OnTriggerExit2D(Collider2D other)
    {
        if (other.gameObject.CompareTag("MovingPlatform"))
        {
            jumpForce = 13f; // 恢复跳跃力
            rb.gravityScale = 3.5f; // 恢复重力
            Debug.Log("Player exited MovingPlatform");
        }
        if (other.gameObject.tag == "switch")
        {
            Debug.Log("Player is out Switch");
            isNearSwitch = false;
            if (switchObject != null)
            {
                switchObject = null; // 清除引用
            }
        }
        // if (other.gameObject.name == "Board")
        // {
        //     Debug.Log("Player is near Board");
        //     BoardBehavior board = other.gameObject.GetComponent<BoardBehavior>();
        //     board.IsOpened = false; // 切换门的开关状态
        //     if (board != null)
        //     {
        //         board.TriggerDoor(); // 触发门的开关
        //     }
        // }
    }
    public void TakeDamage(string source)
    {
        // Time.timeScale = 0; // 停止时间Time.timeScale = 0; // 停止时间
        // lose = true;
        if (deadAudioSource != null)
        {
            deadAudioSource.Play(); // 播放死亡音效
            Debug.Log("111111111111111111111111111111111111111111111111111111111111111111");
        }
        //1s 后AchievementManager.Instance.UnlockAchievement("Die");
        // StartCoroutine(UnlockAchievementAfterDelay(1f));
        StartCoroutine(PlayDeadAndDestroy());
        Debug.Log("Echo took damage from " + source);
    }
}
