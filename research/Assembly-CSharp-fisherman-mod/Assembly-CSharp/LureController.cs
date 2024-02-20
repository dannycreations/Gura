using System;
using TPM;
using UnityEngine;

public class LureController : TackleControllerBase
{
	public override TackleBehaviour SetBehaviour(UserBehaviours behaviourType, IAssembledRod rodAssembly, GameFactory.RodSlot slot, RodOnPodBehaviour.TransitionData transitionData = null)
	{
		base.SetBehaviour(behaviourType, rodAssembly, slot, transitionData);
		if (behaviourType == UserBehaviours.FirstPerson)
		{
			this._behaviour = new Lure1stBehaviour(this, rodAssembly as AssembledRod, transitionData);
		}
		else if (behaviourType == UserBehaviours.ThirdPerson)
		{
			this._behaviour = new Lure3rdBehaviour(this, rodAssembly);
		}
		else if (behaviourType == UserBehaviours.RodPod)
		{
			this._behaviour = new LureOnPodBehaviour(this, rodAssembly, this.topLineAnchor, transitionData);
		}
		return this._behaviour;
	}

	public Transform hookAnchor;

	public Transform baitTopAnchor;

	public Transform baitAnchor;
}
