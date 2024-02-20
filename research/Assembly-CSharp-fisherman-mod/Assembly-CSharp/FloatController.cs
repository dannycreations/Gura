using System;
using TPM;
using UnityEngine;

public class FloatController : TackleControllerBase
{
	public override TackleBehaviour SetBehaviour(UserBehaviours behaviourType, IAssembledRod rodAssembly, GameFactory.RodSlot slot, RodOnPodBehaviour.TransitionData transitionData = null)
	{
		base.SetBehaviour(behaviourType, rodAssembly, slot, transitionData);
		if (behaviourType == UserBehaviours.FirstPerson)
		{
			this._behaviour = new Float1stBehaviour(this, rodAssembly as AssembledRod, transitionData);
		}
		else if (behaviourType == UserBehaviours.ThirdPerson)
		{
			this._behaviour = new Float3rdBehaviour(this, rodAssembly);
		}
		else if (behaviourType == UserBehaviours.RodPod)
		{
			this._behaviour = new FloatOnPodBehaviour(this, rodAssembly, this.waterMark, transitionData);
		}
		return this._behaviour;
	}

	public Transform waterMark;

	public Transform centerAnchor;
}
