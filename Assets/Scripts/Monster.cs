using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Monster : MonoBehaviour
{
    public enum DirectionEnum
    {
        Idle,
        Left,
        Rigth
    }

    public DirectionEnum moveDirection;
    public bool invincible;
    public float speed;
    Rigidbody rb;
    public Player target;

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();   
    }

    private void Start()
    {
        switch (moveDirection)
        {
            case DirectionEnum.Idle:
                rb.velocity = new Vector2(0, 0);
                break;
            case DirectionEnum.Left:
                rb.velocity = new Vector2(-speed, 0);
                break;
            case DirectionEnum.Rigth:
                rb.velocity = new Vector2(speed, 0);
                transform.localScale = new Vector3(-transform.localScale.x, transform.localScale.y, transform.localScale.z);
                break;
            default:
                break;
        }

    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player") && target == other.GetComponent<Player>())
        {
            other.GetComponent<Player>().Damage(1);
        }
        //Destroy(gameObject);
    }


}
