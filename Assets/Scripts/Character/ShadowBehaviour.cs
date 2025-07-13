using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// public class ShadowBehaviour : MonoBehaviour
// {
//     [SerializeField]
//     private Rigidbody2D rb;
//     private float jumpForce = 10;
//     private Animator animator;
//     private float moveSpeed = 6;
//     private bool isGrounded = false;
//     private float moveInput = 0f;
//     private float shadowDuration = 10f; // 影子持续时间10秒
//     private BeaconBehaviour beaconBehaviour = null;
    
//     private int currentFrame = 0;
//     private List<bool[]> input;
//     void Start()
//     {
//         rb = GetComponent<Rigidbody2D>();
//         animator = GetComponent<Animator>();
//         // 启动自动销毁协程
//         StartCoroutine(DestroyAfterTime());
//     }
    
//     // 协程：10秒后自动销毁shadow并通知player
//     IEnumerator DestroyAfterTime()
//     {
//         yield return new WaitForSeconds(shadowDuration);
        
//         // 查找player并通知它回到正常状态
//         GameObject player = GameObject.Find("Player");

//         if (player != null)
//         {
//             PlayerBehaviour playerBehaviour = player.GetComponent<PlayerBehaviour>();
//             if (playerBehaviour != null)
//             {
//                 playerBehaviour.ReturnToPlayer();
//             }
//         }
//         //beaconBehaviour.SwitchPlayer(player.GetComponent<PlayerBehaviour>().nearBeaconPosition);
//         // 销毁shadow
//         Destroy(gameObject);
//     }

//     void FixedUpdate()
//     {
//         rb.velocity = new Vector2(moveInput * moveSpeed, rb.velocity.y);
//         if(input == null){
//             Debug.LogError("Input list is not set. Please set the input list from BeaconBehaviour.");
//             return;
//         }
//         input.Add(new bool[5] { false, false, false, false, false }); // 初始化当前帧的输入状态
//         // 获取当前帧的输入
//         // input[currentFrame] 是一个 bool 数组，代表每帧的 W, A, D, E, G 键状态
//         // W: input[currentFrame][0], A: input[currentFrame][1], D: input[currentFrame][2], E: input[currentFrame][3], G: input[currentFrame][4]
//         var inputState = input[currentFrame];
        
//         // 记录所有按键状态，不使用else if避免冲突
//         if (Input.GetKey(KeyCode.A))
//         {
//             inputState[1] = true; // A键按下
//         } 
//         if (Input.GetKey(KeyCode.D))
//         {
//             inputState[2] = true; // D键按下
//         }
//         if (Input.GetKeyDown(KeyCode.W))
//         {
//             inputState[0] = true; // W键按下
//         }
//         if (Input.GetKeyDown(KeyCode.E))
//         {
//             inputState[3] = true; // E键按下
//         }
//         if(Input.GetKeyDown(KeyCode.G)){ //立刻销毁shadow
//             inputState[4] = true; // G键按下
//         }
        
//         currentFrame++;
//     }

//     void Update()
//     {
//         isGrounded = CheckGrounded();
//         if(isGrounded){
//             animator.SetBool("IsJumping", false);
//         }
        
//         if (Input.GetKey(KeyCode.A))
//         {
//             animator.SetBool("IsWalking", true);
//             transform.localScale = new Vector3(-3, 3, 1);
//             moveInput = -1f;
//         }
//         if (Input.GetKey(KeyCode.D))
//         {
//             animator.SetBool("IsWalking", true);
//             transform.localScale = new Vector3(3, 3, 1);
//             moveInput = 1f;
//         }
//         if (!Input.GetKey(KeyCode.A) && !Input.GetKey(KeyCode.D))
//         {
//             animator.SetBool("IsWalking", false);
//             moveInput = 0f;
//         }
//         if (Input.GetKeyDown(KeyCode.W) && isGrounded)
//         {
//             Debug.Log("Jump");
//             Jump();
//         }
//         if(Input.GetKeyDown(KeyCode.G)){ //立刻销毁shadow
//             GameObject player = GameObject.Find("Player");
//             if (player != null)
//             {
//                 PlayerBehaviour playerBehaviour = player.GetComponent<PlayerBehaviour>();
//                 if (playerBehaviour != null)
//                 {
//                     playerBehaviour.ReturnToPlayer();
//                 }
//             }
//             //beaconBehaviour.SwitchPlayer(player.GetComponent<PlayerBehaviour>().nearBeaconPosition);
//             Debug.Log("Shadow destroyed by G key");
//             Destroy(gameObject);
//         }
        
//     }

//     void MoveLeft()
//     {
//         transform.Translate(Vector3.left * Time.deltaTime * moveSpeed);
//     }

//     void MoveRight()
//     {
//         transform.Translate(Vector3.right * Time.deltaTime * moveSpeed);
//     }

//     void Jump()
//     {
//         animator.SetBool("IsJumping", true);
//         rb.AddForce(Vector2.up * jumpForce, ForceMode2D.Impulse);
//     }
//     public void Interact(){
        
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

//     public void setBeaconBehaviour(BeaconBehaviour p)
//     {
//         beaconBehaviour = p;
//         input=p.getInput();
//         input.Clear();
//     }
// }
public class ShadowBehaviour : MonoBehaviour
{
    public bool isPaused = false; // 新增：暂停状态标志
    [SerializeField] private Rigidbody2D rb;
    [SerializeField] private Animator animator;
    private float jumpForce = 13f;
    private float moveSpeed = 6f;
    [SerializeField] private float shadowDuration = 10f;
    [SerializeField] private float destroyAlpha = 0.2f; // 销毁影子的透明度阈值
    private float maxAlphaDistance;
    
    private bool isGrounded = false;
    private float moveInput = 0f;
    private BeaconBehaviour beaconBehaviour = null;

    private bool isNearSwitch = false;
    private Cainos.PixelArtPlatformer_Dungeon.Switch switchObject = null;
    private GameObject lamp = null; // 灯对象引用
    
    // 基于时间的输入记录
    private List<TimeBasedInputEvent> inputEvents;
    private float recordStartTime;
    private Dictionary<InputType, bool> currentInputStates;
    
    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();
        
        // 初始化输入状态字典
        currentInputStates = new Dictionary<InputType, bool>
        {
            {InputType.W, false},
            {InputType.A, false},
            {InputType.D, false},
            {InputType.E, false},
            {InputType.G, false}
        };
        
        recordStartTime = Time.time;
        StartCoroutine(DestroyAfterTime());
    }
    
    // 协程：10秒后自动销毁shadow并通知player
    IEnumerator DestroyAfterTime()
    {
        yield return new WaitForSeconds(shadowDuration);
        float currentTime = Time.time - recordStartTime;
        beaconBehaviour.SetEchoTime(currentTime);
        beaconBehaviour.restore(); // 恢复Sprite
        // 查找player并通知它回到正常状态
        GameObject player = GameObject.Find("Player");

        if (player != null)
        {
            PlayerBehaviour playerBehaviour = player.GetComponent<PlayerBehaviour>();
            if (playerBehaviour != null)
            {
                playerBehaviour.ReturnToPlayer();
            }
        }
        //beaconBehaviour.SwitchPlayer(player.GetComponent<PlayerBehaviour>().nearBeaconPosition);
        // 销毁shadow
        Destroy(gameObject);
    }
    void Update()
    {
        if (isPaused) return; // 如果游戏暂停，直接返回
        isGrounded = CheckGrounded();
        if (isGrounded)
        {
            animator.SetBool("IsJumping", false);
        }
        
        // 记录输入事件（基于时间）
        RecordInputEvents();
        
        // 处理实际输入
        HandleInput();

        HandleLamp(); // 处理灯光效果
    }
    
    void FixedUpdate()
    {
        if (isPaused) return; // 如果游戏暂停，直接返回
        // 使用固定的物理更新
        rb.velocity = new Vector2(moveInput * moveSpeed, rb.velocity.y);
    }
    
    void RecordInputEvents()
    {
        if (inputEvents == null)
        {
            Debug.LogError("Input events list is not set. Please set it from BeaconBehaviour.");
            return;
        }
        
        float currentTime = Time.time - recordStartTime;
        Vector2 currentPosition = transform.position;
        
        // 检查每个输入键的状态变化
        CheckInputStateChange(KeyCode.W, InputType.W, currentTime, currentPosition);
        CheckInputStateChange(KeyCode.A, InputType.A, currentTime, currentPosition);
        CheckInputStateChange(KeyCode.D, InputType.D, currentTime, currentPosition);
        CheckInputStateChange(KeyCode.E, InputType.E, currentTime, currentPosition);
        CheckInputStateChange(KeyCode.G, InputType.G, currentTime, currentPosition);
    }
    
    void CheckInputStateChange(KeyCode keyCode, InputType inputType, float currentTime, Vector2 position)
    {
        bool currentState = Input.GetKey(keyCode);
        bool previousState = currentInputStates[inputType];
        
        // 检测状态变化
        if (currentState != previousState)
        {
            inputEvents.Add(new TimeBasedInputEvent(currentTime, inputType, currentState, position));
            currentInputStates[inputType] = currentState;
        }
    }
    
    void HandleInput()
    {
        if (isPaused) return; // 如果游戏暂停，直接返回
        // 移动输入
        if (Input.GetKey(KeyCode.A))
        {
            animator.SetBool("IsWalking", true);
            transform.localScale = new Vector3(-3, 3, 1);
            moveInput = -1f;
        }
        else if (Input.GetKey(KeyCode.D))
        {
            animator.SetBool("IsWalking", true);
            transform.localScale = new Vector3(3, 3, 1);
            moveInput = 1f;
        }
        else
        {
            animator.SetBool("IsWalking", false);
            moveInput = 0f;
        }
        
        // 跳跃
        if (Input.GetKeyDown(KeyCode.W) && isGrounded)
        {
            Jump();
        }
        if (Input.GetKeyDown(KeyCode.G))
        {
            DestroyImmediate();
        }
        if (Input.GetKeyDown(KeyCode.E) && isNearSwitch && switchObject != null)
        {
            Debug.Log("E pressed near switch");
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
        
        
        
        // 立即销毁
        if (Input.GetKeyDown(KeyCode.G))
        {
            DestroyImmediate();
        }

        
    }

    void HandleLamp(){
        if (lamp != null){
            // 获取灯的位置和影子位置
            Vector2 lampPos = lamp.transform.position;
            Vector2 shadowPos = transform.position;

            // 计算距离
            float distance = Mathf.Abs(lampPos.x - shadowPos.x);

            // 根据距离计算透明度（越近越透明）
            float alpha = Mathf.InverseLerp(0, maxAlphaDistance, distance);

            // 设置颜色透明度
            SpriteRenderer shadowSpriteRenderer = GetComponent<SpriteRenderer>();
            Color newColor = shadowSpriteRenderer.color;
            newColor.a = alpha;
            shadowSpriteRenderer.color = newColor;

            // 如果透明度低于阈值，销毁影子
            if (alpha <= destroyAlpha)
            {
                AchievementManager.Instance.UnlockAchievement("ShadowSuicide");
                DestroyImmediate();
                
            }
        }
    }
    
    void Jump()
    {
        animator.SetBool("IsJumping", true);
        rb.AddForce(Vector2.up * jumpForce, ForceMode2D.Impulse);
        Debug.Log("Shadow jumped at position: " + transform.position);
        Debug.Log("Shadow jump force applied: " + jumpForce);
    }
    
    void DestroyImmediate()
    {
        float currentTime = Time.time - recordStartTime;
        beaconBehaviour.SetEchoTime(currentTime);
        beaconBehaviour.restore(); // 恢复Sprite
        GameObject player = GameObject.Find("Player");
        if (player != null)
        {
            PlayerBehaviour playerBehaviour = player.GetComponent<PlayerBehaviour>();
            if (playerBehaviour != null)
            {
                playerBehaviour.ReturnToPlayer();
            }
        }
        
        Debug.Log("Shadow destroyed by G key");
        Destroy(gameObject);
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
    
    public void setBeaconBehaviour(BeaconBehaviour beacon)
    {
        beaconBehaviour = beacon;
        inputEvents = beacon.getInput();
        inputEvents.Clear();
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.gameObject.CompareTag("MovingPlatform"))
        {
            jumpForce = 14f;
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
        if (other.gameObject.tag == "lamp")
        {
            lamp = other.gameObject;
            maxAlphaDistance = Mathf.Abs(lamp.transform.position.x - transform.position.x);
        }
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
        if (other.gameObject.tag == "lamp")
        {
            lamp = null;
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
    public void DestroybyTrap()
    {
        float currentTime = Time.time - recordStartTime;
        Debug.Log("Shadow destroyed by trap at time: " + currentTime);
        
        DestroyImmediate();
    }
}
