using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.SceneManagement;

public class PlayerBehaviour : MonoBehaviour
{
    public bool isGrounded = false;

    public bool isPaused = false; // 是否暂停
    [SerializeField]
    private Rigidbody2D rb;
    private float jumpForce = 13f;
    private Animator animator;
    private float moveSpeed = 6;

    private float moveInput = 0f;
    private bool isNearBeacon = false;
    private bool isNearSwitch = false;
    private Cainos.PixelArtPlatformer_Dungeon.Switch switchObject = null;
    private bool isInputEnabled = true;
    private bool isInDoor = false; // 是否在门内
    private bool isBeaconSystem = false; // 是否是信标系统
    public Cainos.PixelArtPlatformer_Dungeon.Door Exit = null;
    public bool win = false; // 是否赢得游戏
    public bool lose= false; // 是否输掉游戏
    public bool AchievementMove = false;
    public bool AchievementJump = false;
    public bool getState()
    {
        return animator.GetBool("IsShadow");
    }
    public Vector3 nearBeaconPosition;
    private BeaconBehaviour beaconBehaviour;

    public AudioSource footstepAudioSource;
    public AudioSource jumpAudioSource;
    public AudioSource deadAudioSource;
    public InputManager inputManager;
    void Start()
    {
        QualitySettings.vSyncCount = 0;           // 关闭 VSync
        Application.targetFrameRate = 60;         // 手动锁帧
        rb = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();
        inputManager = InputManager.Instance; // 获取 InputManager 实例
    }
    public void setBeaconpos(Vector3 pos)
    {
        nearBeaconPosition = pos;
        nearBeaconPosition.y += 0.5f; // 调整Y轴位置
        Debug.Log("nearBeaconPosition set to: " + nearBeaconPosition);
    }
    void SwitchShadow()
    {
        if (!beaconBehaviour)
        {
            return;
        }
        if (beaconBehaviour.HasEcho())
        {
            return;
        }
        animator.SetBool("IsShadow", true);
        animator.SetBool("IsWalking", false);
        beaconBehaviour.SwitchShadow(nearBeaconPosition);
        AchievementManager.Instance.UnlockAchievement("Beacon");
        Debug.Log("Switching shadow");
    }
    void FixedUpdate()
    {
        if (isPaused||lose||win) return; // 如果游戏暂停，直接返回
        // 检查是否处于影子状态
        bool isShadow = animator.GetBool("IsShadow");

        if (!isShadow)
        {
            // 只有在非影子状态下才应用移动速度
            rb.velocity = new Vector2(moveInput * moveSpeed, rb.velocity.y);
        }
        else
        {
            // 影子状态下停止水平移动，但保持垂直速度（重力）
            rb.velocity = new Vector2(0, rb.velocity.y);
        }
    }

    void Update()
    {
        if (isPaused || lose || win) return; // 如果游戏暂停，直接返回
        // 检查是否处于影子状态，如果是则禁用移动
        bool isShadow = animator.GetBool("IsShadow");
        isGrounded = CheckGrounded();
        if (isGrounded)
        {
            animator.SetBool("IsJumping", false);
        }
        if (!isGrounded)
        {
            if (footstepAudioSource != null)
            {
                footstepAudioSource.Stop();
            }
        }

        if (!isShadow)
        {
            // 只有在非影子状态下才允许移动
            if (isInputEnabled && inputManager != null)
            {
                bool left = inputManager.IsLeftPressed();
                bool right = inputManager.IsRightPressed();
                if (left)
                {
                    animator.SetBool("IsWalking", true);
                    transform.localScale = new Vector3(-3, 3, 1);
                    moveInput = -1f;
                    if (footstepAudioSource != null && !footstepAudioSource.isPlaying && isGrounded)
                    {
                        footstepAudioSource.Play();
                    }
                    AchievementMove = true;
                }
                if (right)
                {
                    animator.SetBool("IsWalking", true);
                    transform.localScale = new Vector3(3, 3, 1);
                    moveInput = 1f;
                    if (footstepAudioSource != null && !footstepAudioSource.isPlaying && isGrounded)
                    {
                        footstepAudioSource.Play();
                    }
                    AchievementMove = true;
                }
                if (!left && !right)
                {
                    animator.SetBool("IsWalking", false);
                    moveInput = 0f;
                }
                // 只有在非影子状态下才允许跳跃
                if (inputManager.IsJumpPressed() && isGrounded)
                {
                    Debug.Log("Jump");
                    Jump();
                    if (jumpAudioSource != null)
                    {
                        jumpAudioSource.Play();
                    }
                    AchievementJump = true;
                }
                if (AchievementMove) AchievementManager.Instance.UnlockAchievement("Move");
                if (AchievementJump) AchievementManager.Instance.UnlockAchievement("PressW");
            }
        }
        else
        {
            // 影子状态下停止所有移动
            animator.SetBool("IsWalking", false);
            moveInput = 0f;
        }

        // 切换影子的按键在任何状态下都可以使用
        if (isInputEnabled && inputManager != null && inputManager.IsInteractPressed() && isNearBeacon && !isShadow)
        {
            Debug.Log("E pressed");
            SwitchShadow();
        }

        if (isInputEnabled && inputManager != null && inputManager.IsInteractPressed() && isNearSwitch && switchObject != null)
        {
            Debug.Log("E pressed near switch");
            switchObject.TriggerSwitch(); // 触发开关
            if (switchObject.targetPlatform != null && switchObject.targetPlatform.tag == "MovingPlatform")
            {
                switchObject.targetPlatform.RemainingCount++; // 设置剩余前进路径点数量为1
            }
            else if (switchObject.targetSpike != null)
            {
                Debug.Log("Spike behaviour found, incrementing cntMove");
                switchObject.targetSpike.cntMove += 2;
            }
            else if (switchObject.targetPortal != null)
            {
                bool isActive = switchObject.targetPortal.isActive;
                switchObject.targetPortal.SetPortalState(!isActive);
            }
        }


        // R键（召唤Echo）可选：如需自定义可在InputManager添加对应接口
        if ((beaconBehaviour) && isInputEnabled && inputManager != null && inputManager.IsEchoPressed() && isNearBeacon && !isShadow && !beaconBehaviour.HasEcho())

        {
            Debug.Log("R pressed to summon echo");
            SummonEcho();
        }

        if (isInDoor && Exit != null && Exit.IsOpened)
        {
            Debug.Log("Player is in door and exit is opened");
            isInputEnabled = false; // 禁用输入
            StartCoroutine(GoOutCoroutine()); // 调用GoOut方法
        }
    }
    public void MoveLeft()
    {
        transform.Translate(Vector3.left * Time.deltaTime * moveSpeed);
    }

    public void MoveRight()
    {
        transform.Translate(Vector3.right * Time.deltaTime * moveSpeed);
    }

    public void Jump()
    {
        animator.SetBool("IsJumping", true);
        rb.AddForce(Vector2.up * jumpForce, ForceMode2D.Impulse);
        Debug.Log("Player jumped at position: " + transform.position);
        Debug.Log("Player jump force applied: " + jumpForce);
    }
    public void Interact()
    {

    }
    public void SummonEcho()
    {
        beaconBehaviour.SwitchPlayer(nearBeaconPosition);
    }

    // 当shadow销毁时调用，让player回到正常状态
    public void ReturnToPlayer()
    {
        isInputEnabled = false;
        Debug.Log("Returning to player");

        // 重置动画状态
        animator.SetBool("IsShadow", false);
        animator.SetBool("IsWalking", false);
        animator.SetBool("IsJumping", false);

        // 重置移动输入
        moveInput = 0f;

        // 如果是player获取主摄像机并设置回player
        if (gameObject.name == "Player")
        {
            GameObject mainCamera = GameObject.Find("Main Camera");
            if (mainCamera != null)
            {
                CameraFollow cameraFollow = mainCamera.GetComponent<CameraFollow>();
                if (cameraFollow != null)
                {
                    cameraFollow.target = transform;
                    Debug.Log("Camera target set back to player");
                }
            }
        }
        StartCoroutine(EnableInputAfterDelay(0.16f));
    }
    private IEnumerator EnableInputAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        isInputEnabled = true;
        Debug.Log("Input re-enabled after delay");
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.gameObject.CompareTag("MovingPlatform"))
        {
            jumpForce = 14; // 重置跳跃力
            rb.gravityScale = 10; // 黏住
            Debug.Log("Player entered MovingPlatform");
        }
        if (other.gameObject.CompareTag("beacon"))
        {

            Debug.Log("Beacon Updated");
            isNearBeacon = true; 
            beaconBehaviour = other.gameObject.GetComponent<BeaconBehaviour>();
            if (!(beaconBehaviour.getcallbeacon()))
            {
                beaconBehaviour = null;
                return;
            }
            if (beaconBehaviour.getSystem())
            {
                isBeaconSystem = true; // 标记为信标系统
            }
            else
            {
                nearBeaconPosition = other.gameObject.transform.position;
                nearBeaconPosition.y += 0.5f;
                isBeaconSystem = false; // 标记为非信标系统
            }
        }

        if (other.gameObject.tag == "switch")
        {
            Debug.Log("Player is near Switch");
            isNearSwitch = true;
            switchObject = other.gameObject.GetComponent<Cainos.PixelArtPlatformer_Dungeon.Switch>();
        }

        if (other.gameObject.name == "Board")
        {
            Debug.Log("Player is near Board");
            BoardBehavior board = other.gameObject.GetComponent<BoardBehavior>();
            board.IsOpened = true; // 切换门的开关状态
            Debug.Log("Board is opened: " + board.IsOpened);
            if (board != null)
            {
                board.TriggerDoor(); // 触发门的开关
            }
        }

        if (Exit != null && other.GetComponent<Cainos.PixelArtPlatformer_Dungeon.Door>() == Exit)

        {
            if (Exit.IsOpened)
            {
                // win = true; // 设置赢得游戏的状态
                isInputEnabled = false; // 禁用输入
                Debug.Log("Exit door is opened, player will go out");
                StartCoroutine(GoOutCoroutine()); // 调用GoOut方法
            }
        }

    }

    void OnTriggerStay2D(Collider2D other)
    {
        if (Exit != null && other.GetComponent<Cainos.PixelArtPlatformer_Dungeon.Door>() == Exit)
        {
            isInDoor = true; // 标记玩家在门内
        }
    }

    void OnTriggerExit2D(Collider2D other)
    {
        if (other.gameObject.CompareTag("MovingPlatform"))
        {
            jumpForce = 13; // 恢复跳跃力
            rb.gravityScale = 3.5f; // 恢复重力
            Debug.Log("Player exited MovingPlatform");
        }
        if (other.gameObject.CompareTag("beacon"))
        {
            isNearBeacon = false;
            if (!isBeaconSystem) nearBeaconPosition = Vector3.zero;
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
        if (Exit != null && other.GetComponent<Cainos.PixelArtPlatformer_Dungeon.Door>() == Exit)
        {
            isInDoor = false;
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

    //新增：设置moveInput的public方法
    public void SetMoveInput(float input)
    {

        moveInput = input;
    }

    // 新增：获取isNearBeacon的public方法
    public bool IsNearBeacon()
    {
        return isNearBeacon;
    }

    IEnumerator GoOutCoroutine()
    {
        bool isRight = Exit.transform.position.x > transform.position.x;
        Exit.SetControl(false); // 禁用门的控制

        float duration = 2.0f; // 动画持续时间
        float elapsed = 0.0f;

        Vector3 initialPosition = transform.position;
        Vector3 targetPosition = new Vector3(Exit.transform.position.x, transform.position.y, transform.position.z);

        Color initialColor = GetComponent<SpriteRenderer>().color;

        while (elapsed < duration)
        {
            float t = elapsed / duration;

            // 平滑移动
            transform.position = Vector3.Lerp(initialPosition, targetPosition, t);

            // 逐渐透明
            Color newColor = initialColor;
            newColor.a = Mathf.Lerp(initialColor.a, 0, t);
            GetComponent<SpriteRenderer>().color = newColor;

            elapsed += Time.deltaTime;
            yield return null;
        }

        GetComponent<SpriteRenderer>().color = new Color(0f, 0f, 0f, 0f);
        moveInput = 0f; // 停止移动
        Debug.Log("Exit door opened");
        win = true; // 设置赢得游戏的状态
        // 加载下一个场景，延迟一点让动画完成
        yield return new WaitForSeconds(1.3f); // 可选：等待门打开动画完成
        

        // SceneManager.LoadScene("Menu"); // 替换为实际的场景名称
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
    public bool IsWin()
    {
        return win; // 返回是否赢得游戏的状态
    }
    public bool IsLose()
    {
        
        return lose; // 返回是否输掉游戏的状态
    }
    public void TakeDamage(string source)
    {
        Time.timeScale = 0; // 停止时间Time.timeScale = 0; // 停止时间
        lose = true;
        if (deadAudioSource != null)
        {
            deadAudioSource.Play(); // 播放死亡音效
        }
        //1s 后AchievementManager.Instance.UnlockAchievement("Die");
        StartCoroutine(UnlockAchievementAfterDelay(1f));
        Debug.Log("Player took damage from " + source);
    }
    public void Restart()
    {
        Debug.Log("Restarting scene");
        SceneManager.LoadScene(SceneManager.GetActiveScene().name); // 重新加载当前场景
    }
    private IEnumerator UnlockAchievementAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        AchievementManager.Instance.UnlockAchievement("Die");
    }
}