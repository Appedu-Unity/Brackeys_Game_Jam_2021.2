using UnityEngine;
using UnityEngine.SceneManagement;

public class Player : MonoBehaviour
{
    private Rigidbody2D rig;
    private BoxCollider2D coll;
    private AudioSource aud;
    int trapsLayer;

    [Header("玩家")]
    public GameObject pl;

    [Header("特效物件")]
    public GameObject deathVFXPrefab;

    [Header("音效")]
    public AudioClip jump;
    public AudioClip eat;
    public AudioClip die;
    public AudioClip Teleport;

    [Header("移動速度")]
    public float speed = 5f;                //正常
    public float crouchSpeedDivisor = 3f;   //蹲下

    [Header("跳躍值")]
    public float jumpForce = 6f;            //跳躍
    public float jumpHoldForce = 2f;        //長按加成力道
    public float jumpHoldDuration = 0.1f;   //可長按時間
    public float crouchJumpBoost = 2.5f;    //額外跳躍力道加乘
    public float hangingJumpForce = 15;     //懸掛後之額外跳躍

    private float jumpTime;

    [Header("狀態")]
    public bool isCrouch;       //下蹲狀態
    public bool isOnGround;     //是否在地面上
    public bool isJump;         //跳躍狀態
    public bool isHeadBlocked;  //頭頂
    public bool isHanging;      //懸掛



    [Header("環境檢測")]
    public float footOffset = 0.4f;         //玩家左右距離位置
    public float headClearance = 0.5f;      //頭頂射線
    public float groundDistance = 0.2f;     //地面距離檢測
    private float playerHeight;             //玩家coll高度
    public float eyeHeight = -0.4f;          //玩家視野
    public float grabDistance = 0.4f;       //牆壁與玩家距離
    public float reachOffset = 0.7f;        //頭頂牆壁與玩家相對距離

    public LayerMask groundLayer;

    public float Xvelocity;

    //按鍵檢測
    private bool jumpPressed;
    private bool jumpHeld;
    private bool crouchHeld;
    private bool crouchPressed;


    //collider尺寸
    private Vector2 colliderStandSize;
    private Vector2 colliderStandOffset;
    private Vector2 colliderCrouchSize;
    private Vector2 colliderCrouchOffset;


    private void Start()
    {
        rig = GetComponent<Rigidbody2D>();
        coll = GetComponent<BoxCollider2D>();
        aud = GetComponent<AudioSource>();

        trapsLayer = LayerMask.NameToLayer("Trap");

        playerHeight = coll.size.y;

        colliderStandSize = coll.size;
        colliderStandOffset = coll.offset;
        colliderCrouchSize = new Vector2(coll.size.x, coll.size.y / 2f);
        colliderCrouchOffset = new Vector2(coll.offset.x, coll.offset.y / 2f);
    }
    private void Update()
    {
        

        jumpPressed = Input.GetButtonDown("Jump");
        jumpHeld = Input.GetButton("Jump");
        crouchHeld = Input.GetButton("Crouch");
        crouchPressed = Input.GetButtonDown("Crouch");
    }
    private void FixedUpdate()
    {
        PhysicsChech();
        GroundMovement();
        MidAirMovement();
    }
    /// <summary>
    /// 物理檢測
    /// </summary>
    private void PhysicsChech()
    {
        /*Vector2 pos = transform.position;
        Vector2 offset = new Vector2(-footOffset, 0f);

        RaycastHit2D leftCheck = Physics2D.Raycast(pos + offset,Vector2.down , groundDistance , groundLayer);*/

        RaycastHit2D leftCheck = Raycast(new Vector2(-footOffset, -.5f), Vector2.down, groundDistance, groundLayer);
        RaycastHit2D rightCheck = Raycast(new Vector2(footOffset, -.5f), Vector2.down, groundDistance, groundLayer);

        if (leftCheck || rightCheck)
            isOnGround = true;
        else isOnGround = false;

        RaycastHit2D headCheck = Raycast(new Vector2(0, 0.7f - coll.size.y), Vector2.up, headClearance, groundLayer);

        if (headCheck)
            isHeadBlocked = true;
        else isHeadBlocked = false;

        float direction = transform.localScale.x;       //初始懸掛面向
        Vector2 graDir = new Vector2(direction, 0f);    //射線方向

        RaycastHit2D blockedCheck = Raycast(new Vector2(footOffset * direction, 0.2f - playerHeight), graDir, grabDistance, groundLayer);  //頭部
        //(footOffset * direction:腳步位置，高度)，射線方向，距離，判斷碰撞圖層
        RaycastHit2D wallCheck = Raycast(new Vector2(footOffset * direction, .2f - eyeHeight), graDir, grabDistance, groundLayer);           //眼部
        RaycastHit2D ledgeCheck = Raycast(new Vector2(reachOffset * direction, .2f - playerHeight), Vector2.down, grabDistance, groundLayer);//眼前距離

        if (!isOnGround && rig.velocity.y < 0f && ledgeCheck && wallCheck && !blockedCheck)
        {
            Vector3 pos = transform.position;

            pos.x += (wallCheck.distance - 0.05f) * direction;

            pos.y -= ledgeCheck.distance;

            transform.position = pos;

            rig.bodyType = RigidbodyType2D.Static;  //懸掛固定
            isHanging = true;
        }
    }
    private void GroundMovement()
    {
        if (isHanging)                              //懸掛(停止炫掛後左右旋轉)
            return;

        if (crouchHeld && !isCrouch && isOnGround)  //下蹲鍵
            Crouch();
        else if (!crouchHeld && isCrouch && !isHeadBlocked)
            StandUp();
        else if (!isOnGround && isCrouch)
            StandUp();
        Xvelocity = Input.GetAxis("Horizontal");    //1~-1X值

        if (isCrouch)
            Xvelocity /= crouchSpeedDivisor;
        rig.velocity = new Vector2(Xvelocity * speed, rig.velocity.y);
        FilpDirction();
    }
    /// <summary>
    /// 跳躍
    /// </summary>
    private void MidAirMovement()
    {
        if (isHanging)
        {
            if (jumpPressed)
            {
                aud.PlayOneShot(jump, Random.Range(0.3f, 0.5f));
                rig.bodyType = RigidbodyType2D.Dynamic;
                rig.velocity = new Vector2(rig.velocity.x, hangingJumpForce);
                isHanging = false;
            }
            if (crouchPressed)  //單次按下蹲
            {
                rig.bodyType = RigidbodyType2D.Dynamic;
                isHanging = false;
            }
        }

        if (jumpPressed && isOnGround && !isJump && !isHeadBlocked)       //確認按下跳躍

        {
            aud.PlayOneShot(jump, Random.Range(0.3f, 0.5f));
            if (isCrouch)  //下蹲跳躍
            {
                StandUp();  //恢復coll狀態
                rig.AddForce(new Vector2(0f, jumpForce), ForceMode2D.Impulse);
            }

            isOnGround = false;                         //正在地面=false
            isJump = true;                              //正在跳躍 = true

            jumpTime = Time.time + jumpHoldDuration;    //跳躍時間計算 = 當前跳躍時間 + 長按時間

            rig.AddForce(new Vector2(0f, jumpForce), ForceMode2D.Impulse);
        }
        else if (isJump)
        {
            aud.PlayOneShot(jump, Random.Range(0.3f, 0.5f));
            if (jumpHeld)   //同時持續按住跳躍鍵
                rig.AddForce(new Vector2(0f, jumpHoldForce), ForceMode2D.Impulse);
            if (jumpTime < Time.time)
                isJump = false;
        }
    }
    /// <summary>
    /// 角色面向
    /// </summary>
    private void FilpDirction()
    {
        if (Xvelocity < 0)
            transform.localScale = new Vector3(-1, 1, 1);
        if (Xvelocity > 0)
            transform.localScale = new Vector3(1, 1, 1);
    }
    /// <summary>
    /// 角色下蹲
    /// </summary>
    private void Crouch()
    {
        isCrouch = true;
        coll.size = colliderCrouchSize;
        coll.offset = colliderCrouchOffset;
    }
    /// <summary>
    /// 恢復狀態
    /// </summary>
    private void StandUp()
    {
        isCrouch = false;
        coll.size = colliderStandSize;
        coll.offset = colliderStandOffset;
    }
    RaycastHit2D Raycast(Vector2 offset, Vector2 rayDiraction, float lenght, LayerMask layer)   //位移、方向、距離、圖層
    {
        Vector2 pos = transform.position;       //獲得角色初始位置

        RaycastHit2D hit = Physics2D.Raycast(pos + offset, rayDiraction, lenght, layer);    //撞擊圖層

        Color color = hit ? Color.red : Color.green;

        Debug.DrawRay(pos + offset, rayDiraction * lenght, color); //線顯示

        return hit; //回傳
    }
    public void Dead(string obj)
    {
        if (obj == "死亡區域" )
        {
            aud.PlayOneShot(die, Random.Range(0.3f, 0.5f));
            enabled = false;
            Invoke("Replay", 2.5f);
            //gm.PlayerDead();
        }
        if (obj == "傳送門")                      // 如果 在門裡面 並且 按下 W
        {
            aud.PlayOneShot(Teleport, Random.Range(0.3f, 0.5f));
            int lvIndex = SceneManager.GetActiveScene().buildIndex;     // 取得當前場景的編號

            lvIndex++;                                                  // 編號加一

            SceneManager.LoadScene(lvIndex);                            // 載入下一關
        }
    }
    

    public bool inDoor;

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.name == "傳送門") inDoor = true;
        aud.PlayOneShot(Teleport, Random.Range(0.3f, 0.5f));
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.name == "傳送門") inDoor = false;
    }
    private void OnCollisionEnter2D(Collision2D collision)
    {
        aud.PlayOneShot(die, Random.Range(0.3f, 0.5f));
        Dead(collision.gameObject.tag);
    }
    private void Replay()
    {
        // 載入場景(當前場景 的 名稱)
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }


}
