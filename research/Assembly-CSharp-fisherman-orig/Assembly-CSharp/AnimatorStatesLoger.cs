using System;
using System.Collections.Generic;
using UnityEngine;

[SharedBetweenAnimators]
public class AnimatorStatesLoger : StateMachineBehaviour
{
	public static void Init()
	{
		AnimatorStatesLoger.tagHashToName = new Dictionary<int, string>
		{
			{
				Animator.StringToHash("EmptyState"),
				"EmptyState"
			},
			{
				Animator.StringToHash("FishSelector"),
				"FishSelector"
			},
			{
				Animator.StringToHash("BackToIdleSelector"),
				"BackToIdleSelector"
			},
			{
				Animator.StringToHash("DrawInSelector"),
				"DrawInSelector"
			},
			{
				Animator.StringToHash("OpenDraw"),
				"OpenDraw"
			},
			{
				Animator.StringToHash("Idle"),
				"Idle"
			}
		};
	}

	public override void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
	{
		if (AnimatorStatesLoger.tagHashToName.ContainsKey(stateInfo.tagHash))
		{
		}
	}

	public static Dictionary<int, string> tagHashToName;
}
