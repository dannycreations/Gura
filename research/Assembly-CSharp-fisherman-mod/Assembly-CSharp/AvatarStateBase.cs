using System;
using UnityEngine;

public abstract class AvatarStateBase : FsmBaseState<PlayerController>
{
	protected PlayerController Player
	{
		get
		{
			return this._owner;
		}
	}

	protected AnimationState AnimationState;
}
