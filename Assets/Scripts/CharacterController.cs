using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CharacterController : MonoBehaviour
{
    public Rigidbody2D rb;
    public Animator anim;
    public LayerMask ground;
    public Collider2D boxColli;
    public Collider2D cirColli;
    private Manager manager;
    [SerializeField]
    private float speed;
    [SerializeField]
    private float crouchingSpeed;
    [SerializeField]
    private float jumpForce;
    [SerializeField]
    private float recoil;
    private float localScale; // localScale的绝对值
    // user input
    private float horizontalMoving;
    private float RawHorizontalMoving;
    private bool Jump;
    private bool Crouch;
    private bool gettingHurt;
    private enum State { idle, running, jumping, falling, crouch };
    private State state;
    private float[,] boxColliderOffsets = { { 0.07f, -0.13f }, { 0.25f, -0.65f } }; // {idle}, {crouching}
    private float[,] circleColliderOffsets = { { -0.1f, -0.7f }, { -0.1f, -0.7f } };

    void Start()
    {
        localScale = Mathf.Abs(this.transform.localScale.x);
        state = State.idle;
        // 注册Manager.PlayerCollectCherry事件的订阅者
        manager = GameObject.FindGameObjectWithTag("GameController").GetComponent<Manager>();
        //manager.PlayerCollectCherry += new Manager.MessageHandler(PlayerHitCherry);
    }

    private void Update()
    {
        ReadUserInput();
    }

    void FixedUpdate()
    {
        if (!gettingHurt)
        {
            Movement();
        }
        else
        {
            SetAnimStateToGettingHurt();
        }
        if (rb.velocity.magnitude < 0.3f) // 这样会不会太浪费性能？
        {
            gettingHurt = false;
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.tag == "Item")
        {
            PlayerHitCherry(collision);
        }
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        // getting hurt: Player碰到enemy后受到一个向后的反冲力
        if (collision.gameObject.tag == "enemy")
        {
            gettingHurt = true;

            Vector2 Venemy = collision.gameObject.transform.position;
            Vector2 Vplayer = this.gameObject.transform.position;
            Vector2 direction = (Vplayer - Venemy).normalized;

            rb.velocity = direction*recoil;
        }
    }

    // getter/setter
    public float JumpV
    {
        get { return jumpForce; }
    }

    private void PlayerHitCherry(Collider2D collision)
    {
        // 触发Manager.PlayerCollectCherry事件
        //manager.PlayerCollectCherry();
        manager.awakePCCEvent();
        // 销毁该cherry
        Object.Destroy(collision.gameObject);
    }

    void ReadUserInput()
    {
        horizontalMoving = Input.GetAxis("Horizontal");
        RawHorizontalMoving = Input.GetAxisRaw("Horizontal");
        Jump = Input.GetButton("Jump");
        Crouch = Input.GetButton("Fire1");
    }


    void Movement()
    {
        switch (state)
        {
            case State.idle:
                // idle->running
                if (horizontalMoving != 0)
                {
                    state = State.running;
                }
                // idle->jumping
                if (Jump && cirColli.IsTouchingLayers(ground))
                {
                    rb.velocity = new Vector2(rb.velocity.x, jumpForce);
                    state = State.jumping;
                }
                // idle->crouching
                if (Crouch)
                {
                    //更改Player的collider offset
                    boxColli.offset = new Vector2(boxColliderOffsets[1, 0], boxColliderOffsets[1, 1]);
                    cirColli.offset = new Vector2(circleColliderOffsets[1, 0], circleColliderOffsets[1, 1]);
                    state = State.crouch;
                }
                SetAnimStateToIdle();
                break;
            case State.running:
                // running
                rb.velocity = new Vector2(horizontalMoving * speed, 0);
                // flipping
                if (RawHorizontalMoving == 1)
                {
                    this.transform.localScale = new Vector3(localScale, localScale, localScale);
                }
                else if (RawHorizontalMoving == -1)
                {
                    this.transform.localScale = new Vector3(-localScale, localScale, localScale);
                }
                // running->idle
                if (horizontalMoving == 0)
                {
                    state = State.idle;
                }
                // running->jumping
                if (Jump && cirColli.IsTouchingLayers(ground))
                {
                    rb.velocity = new Vector2(rb.velocity.x, jumpForce);
                    state = State.jumping;
                }
                SetAnimStateToRunning(horizontalMoving);
                break;
            case State.jumping:
                SetAnimStateToJumping();
                // jumping->falling
                if (rb.velocity.y < 0)
                {
                    state = State.falling;
                }
                break;
            case State.falling:
                SetAnimStateToFalling();
                // falling->idle
                if (cirColli.IsTouchingLayers(ground))
                {
                    state = State.idle;
                }
                break;
            case State.crouch:
                SetAnimStateTocrouching();
                // moving
                rb.velocity = new Vector2(horizontalMoving * crouchingSpeed, 0);
                // flipping
                if (RawHorizontalMoving == 1)
                {
                    this.transform.localScale = new Vector3(localScale, localScale, localScale);
                }
                else if (RawHorizontalMoving == -1)
                {
                    this.transform.localScale = new Vector3(-localScale, localScale, localScale);
                }
                // crouching->idle
                if (!Crouch)
                {
                    //更改Player的collider offset
                    boxColli.offset = new Vector2(boxColliderOffsets[0, 0], boxColliderOffsets[0, 1]);
                    cirColli.offset = new Vector2(circleColliderOffsets[0, 0], circleColliderOffsets[0, 1]);
                    state = State.idle;
                }
                break;
        }
    }

    // 控制animation state machine
    //? 怎么优化这串臃肿的代码？
    void SetAnimStateToIdle()
    {
        anim.SetBool("idle", true);
        anim.SetFloat("running", 0);
        anim.SetBool("falling", false);
        anim.SetBool("crouching", false);
        anim.SetBool("gettingHurt", false);
    }
    void SetAnimStateToRunning(float horizontalMoving)
    {
        anim.SetBool("idle", false);
        anim.SetFloat("running", horizontalMoving);
    }
    void SetAnimStateToJumping()
    {
        anim.SetBool("jumping", true);
        anim.SetBool("idle", false);
    }
    void SetAnimStateToFalling()
    {
        anim.SetBool("falling", true);
        anim.SetBool("jumping", false);
    }
    void SetAnimStateTocrouching()
    {
        anim.SetBool("crouching", true);
        anim.SetBool("idle", false);
    }
    void SetAnimStateToGettingHurt()
    {
        anim.SetBool("gettingHurt", true);
        anim.SetBool("idle", false);
    }
}
