using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ObstacleScore : MonoBehaviour
{
    GameController gameControl;
    private void Awake()
    {
        gameControl = GameObject.Find("GameController").GetComponent<GameController>();
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            other.GetComponent<Player>().lobePoints[1]++;
            other.GetComponent<Player>().lobeScores[1] += other.GetComponent<Player>().lobePoints[1] * 5;
            gameControl.jumpPointText.text = "Jump Point: " + other.GetComponent<Player>().lobePoints[1];
        }
    }
}
