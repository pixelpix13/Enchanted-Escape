using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerCollision : MonoBehaviour
{
    void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Enemy"))
        {
            // Here, you can define what happens when the player collides with an enemy
            Debug.Log("Player collided with enemy!");
        }
    }
}
