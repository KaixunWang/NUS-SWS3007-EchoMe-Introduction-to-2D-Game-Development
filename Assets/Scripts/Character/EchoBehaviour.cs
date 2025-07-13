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
    [SerializeField] private float echoDuration = 10f;
    
    private List<TimeBasedInputEvent> inputEvents;
    private int currentEventIndex = 0;
    private float replayStartTime;
    private Dictionary<InputType, bool> currentInputStates;
    private bool isGrounded = false;

    // Start is called before the first frame update

    private bool isNearSwitch = false;
    private Cainos.PixelArtPlatformer_Dungeon.Switch switchObject = null;
    private BoxesBehavior boxes = null;
    private BeaconBehaviour beaconBehaviour;
    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();

        // 初始化输入状态
        currentInputStates = new Dictionary<InputType, bool>
        {
            {InputType.W, false},
            {InputType.A, false},
            {InputType.D, false},
            {InputType.E, false},
            {InputType.G, false}
        };

        replayStartTime = Time.time;

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
        // if (currentEventIndex >= inputEvents.Count)
        // {
        //     beaconBehaviour.SetHasEcho(false);
        //         Destroy(gameObject);
        // }
        // 根据当前输入状态执行动作
        ExecuteCurrentInputs();
        
    }
    
    void FixedUpdate()
    {
        // 计算移动输入
        float moveInput = 0f;
        if (currentInputStates[InputType.A])
        {
            moveInput = -1f;
        }
        else if (currentInputStates[InputType.D])
        {
            moveInput = 1f;
        }
        
        rb.velocity = new Vector2(moveInput * moveSpeed, rb.velocity.y);
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
                }
                break;
            case InputType.E:
                Interact();
                break;
            case InputType.G:
                beaconBehaviour.SetHasEcho(false);
                Destroy(gameObject);
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
        }
        else if (currentInputStates[InputType.D])
        {
            animator.SetBool("IsWalking", true);
            transform.localScale = new Vector3(3, 3, 1);
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
            switchObject.IsOn = !switchObject.IsOn; // 切换开关状态
            if (switchObject.targetPlatform != null && switchObject.targetPlatform.tag == "MovingPlatform")
            {
                switchObject.targetPlatform.RemainingCount++; // 设置剩余前进路径点数量为1
            }
            else if (switchObject.targetSpike != null)
            {
                switchObject.targetSpike.cntMove+=2;
            }

        }
    }
    
    IEnumerator DestroyAfterTime()
    {
        yield return new WaitForSeconds(echoDuration);
        beaconBehaviour.SetHasEcho(false);
        Destroy(gameObject);
    }
    public void DestroyImmediate()
    {
        beaconBehaviour.SetHasEcho(false);
        Destroy(gameObject);
    }
    void OnCollisionEnter2D(Collision2D collision)
    {
       
        if (collision.gameObject.name == "Boxes")
        {
            boxes = collision.gameObject.GetComponent<BoxesBehavior>();
            Debug.Log("Collided with Boxes");
            boxes.SetSpeed(moveSpeed); // 设置盒子的移动速度
            if (collision.contacts[0].normal.x > 0)
            {
                Debug.Log("Collision on right side, pushing left");
                // 如果碰撞发生在右侧，向左推动
                boxes.PushLeft();
            }
            else if (collision.contacts[0].normal.x < 0)
            {
                Debug.Log("Collision on left side, pushing right");
                // 如果碰撞发生在左侧，向右推动
                boxes.PushRight();
            }
        }
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
}
