using System.Collections;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;

public class FrogMotion : MonoBehaviour
{
    public Rigidbody2D rigb;
    public BoxCollider2D boxColli;
    public CircleCollider2D cirColli;
    public Animator anim;
    public LayerMask ground;
    [SerializeField]
    private float moveSpeed;
    [SerializeField]
    private float jumpV;
    [SerializeField]
    private float idlingTime;
    [SerializeField]
    private GameObject leftBound;
    [SerializeField]
    private GameObject rightBound;
    private float leftBoundX;
    private float rightBoundX;
    private bool faceLeft = true;
    private float localScale; // localScale的绝对值
    private enum State { idling, jumping, falling, gettingHurt};
    private State state;

    private void Start()
    {
        localScale = Mathf.Abs(this.transform.localScale.x);
        // 获取该游戏对象的left/rightBoundX，并销毁left/rightBound
        leftBoundX = leftBound.transform.position.x;
        rightBoundX = rightBound.transform.position.x;
        GameObject.Destroy(leftBound);
        GameObject.Destroy(rightBound);

        state = State.idling;
    }

    private void FixedUpdate()
    {
        Movement();
        SwitchAnim();
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.name == "killingBoot")
        {
            // destroy enemy
            Debug.Log("destroy enemy");
            GameObject.Destroy(this.gameObject);
            // make character jump again
            GameObject player = GameObject.FindGameObjectWithTag("Player");
            Rigidbody2D characterRb = player.GetComponent<CharacterController>().rb;
            float JumpV = player.GetComponent<CharacterController>().JumpV;
            characterRb.velocity = new Vector2(characterRb.velocity.x, JumpV);
        }
    }

    IEnumerator Timer()
    {
        yield return new WaitForSeconds(idlingTime); // 停止执行x秒
        idlingMovement();
    }

    private void idlingMovement()
    {
        // 在leftBound和rightBound之间来回跳动
        if (faceLeft)
        {
            if (this.transform.position.x < leftBoundX)
            {
                faceLeft = false;
            }
            this.transform.localScale = new Vector3(localScale, localScale, localScale);
            rigb.velocity = new Vector2(-moveSpeed, jumpV);
            state = State.jumping;
            Debug.Log(state);
        }
        else if (!faceLeft)
        {
            if (this.transform.position.x > rightBoundX)
            {
                faceLeft = true;
            }
            this.transform.localScale = new Vector3(-localScale, localScale, localScale);
            rigb.velocity = new Vector2(moveSpeed, jumpV);
            state = State.jumping;
            Debug.Log(state);
        }
    }

    private void jumpingMovement()
    {
        if (rigb.velocity.y < 0)
        {
            state = State.falling;
        }
    }

    private void fallingMovement()
    {
        if (cirColli.IsTouchingLayers(ground))
        {
            state = State.idling;
        }
    }

    private void SwitchAnim()
    {
        switch (state)
        {
            case State.idling:
                anim.SetBool("idle", true);
                anim.SetBool("falling", false);
                break;
            case State.jumping:
                anim.SetBool("jumping", true);
                anim.SetBool("idle", false);
                break;
            case State.falling:
                anim.SetBool("falling", true);
                anim.SetBool("jumping", false);
                break;
        }
    }

    private void Movement()
    {
        switch (state)
        {
            case State.idling:
                // idle一定时间后才能起跳
                StartCoroutine(Timer());
                break;
            case State.jumping:
                jumpingMovement();
                break;
            case State.falling:
                fallingMovement();
                break;
        }
    }
}
