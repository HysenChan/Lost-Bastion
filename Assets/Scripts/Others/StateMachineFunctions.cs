using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StateMachineFunctions : StateMachineBehaviour
{
    public bool NotifyAnimationOnFinish;

    public override void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        if (NotifyAnimationOnFinish)
        {
            animator.gameObject.SendMessage("Ready", SendMessageOptions.DontRequireReceiver);
        }
    }
}
