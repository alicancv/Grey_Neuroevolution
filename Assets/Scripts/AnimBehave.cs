using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AnimBehave : StateMachineBehaviour
{
    //OnStateUpdate is called on each Update frame between OnStateEnter and OnStateExit callbacks
    override public void OnStateUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        if (animator.GetComponent<Player>().inAir)
            return;
        animator.GetComponent<Rigidbody>().velocity = Vector2.zero;
    }

    // OnStateExit is called when a transition ends and the state machine finishes evaluating this state
    override public void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        Player player = animator.GetComponent<Player>();
        player.attacking = false;
        player.RunAction.Invoke();
    }
}
