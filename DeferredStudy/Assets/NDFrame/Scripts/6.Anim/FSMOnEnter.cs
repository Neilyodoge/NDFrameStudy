using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 把这个OnEnter的信息发送出去，别的脚本也能调用
/// 要优化，这里比较耗
/// </summary>
public class FSMOnEnter : StateMachineBehaviour
{
    public string[] onEnterMessages;
    // OnStateEnter is called when a transition starts and the state machine starts to evaluate this state
    override public void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        foreach (var msg in onEnterMessages){
            // animator.gameObject.SendMessage(msg);
            animator.gameObject.SendMessageUpwards(msg);//向上发送,就不用挂在角色模型上，可以挂在ActorController同级
        }
    }

    // OnStateUpdate is called on each Update frame between OnStateEnter and OnStateExit callbacks
    //override public void OnStateUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    //{
    //    
    //}

    // OnStateExit is called when a transition ends and the state machine finishes evaluating this state
    //override public void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    //{
    //    
    //}

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
