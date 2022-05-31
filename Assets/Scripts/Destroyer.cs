using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Destroyer : MonoBehaviour
{
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            other.GetComponent<Player>().Damage(1);

        }else if(!other.CompareTag("Ground") && other.CompareTag("Monster"))
        {
            other.GetComponent<Monster>().target.currentEnemies.Remove(other.GetComponent<Monster>());
            Destroy(other.gameObject);
        }
    }
}
