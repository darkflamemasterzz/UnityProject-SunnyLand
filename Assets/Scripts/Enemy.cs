using System.Collections;
using System.Collections.Generic;
using UnityEngine;

abstract public class Enemy : MonoBehaviour
{
    public Rigidbody2D rigb;
    public Animator anim;
    public LayerMask ground;
    [SerializeField]
    protected float moveSpeed;
    protected bool faceLeft = true;
    protected float localScale; // localScale的绝对值

    protected virtual void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.name == "killingBoot")
        {
            // destroy enemy
            anim.SetTrigger("dead");
            // make character jump again
            GameObject player = GameObject.FindGameObjectWithTag("Player");
            Rigidbody2D characterRb = player.GetComponent<CharacterController>().rb;
            float JumpV = player.GetComponent<CharacterController>().JumpV;
            characterRb.velocity = new Vector2(characterRb.velocity.x, JumpV);
        }
    }

    protected abstract void Movement();

    protected void destroyObj()
    {
        GameObject.Destroy(this.gameObject);
    }
}
