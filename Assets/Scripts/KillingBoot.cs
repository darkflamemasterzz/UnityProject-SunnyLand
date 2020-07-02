using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class KillingBoot : MonoBehaviour
{
    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.tag == "enemy")
        {
            // destroy enemy
            Debug.Log("destroy enemy");
            GameObject.Destroy(collision.gameObject);
            // make character jump again
        }
    }
}
