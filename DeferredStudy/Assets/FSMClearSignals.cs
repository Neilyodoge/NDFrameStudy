using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 当重复按键的时候，清除多余的信号
/// </summary>
public class FSMClearSignals : StateMachineBehaviour
{
    public string[] ClearAtEnter;
    public string[] ClearAtExit;

    // OnStateEnter is called when a transition starts and the state machine starts to evaluate this state
    

    // OnStateUpdate is called on each Update frame between OnStateEnter and OnStateExit callbacks
    override public void OnStateUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        foreach (var signal in ClearAtEnter){
            animator.ResetTrigger(signal);  // 清空所有信号
        }
    }

    // OnStateExit is called when a transition ends and the state machine finishes evaluating this state
    override public void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        foreach (var signal in ClearAtExit)
        {
            animator.ResetTrigger(signal);  // 清空所有信号
        }
    }

    // OnStateMove is called right after Animator.OnAnimatorMove()
    //override public void OnStateMove(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    //{
    //    // Implement code that processes and affects root motion
    //}

    // OnStateIK is called right after Animator.OnAnimatorIK()
    //override public void OnStateIK(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    //{
    //    // Implement code that sets up animation IK (inverse kinematics)
    //}
}
