using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GroundCheck : MonoBehaviour
{
    Player player;
    List<Collider> collidingGrounds;

    private void Awake()
    {
        collidingGrounds = new List<Collider>();
        player = transform.parent.GetComponent<Player>();
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Ground") || other.CompareTag("Obstacle"))
        {
            collidingGrounds.Add(other);
            player.inAir = false;
            player.jumpPressed = false;
            player.RunAction.Invoke();
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Ground") || other.CompareTag("Obstacle"))
        {
            collidingGrounds.Remove(other);
            if (collidingGrounds.Count <= 0)
            {
                player.inAir = true;
            }

            //if (!player.jumpPressed)
            //{
            //    player.rb.velocity = new Vector2(player.movementVector.x * 2, player.rb.velocity.y);
            //}
        }
    }
}
