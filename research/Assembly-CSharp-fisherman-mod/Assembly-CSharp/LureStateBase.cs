using System;
using UnityEngine;

public abstract class LureStateBase : FsmBaseState<ILureBehaviour>
{
	protected ILureBehaviour Lure
	{
		get
		{
			return this._owner;
		}
	}

	protected PlayerController player
	{
		get
		{
			return GameFactory.Player;
		}
	}

	protected GameFactory.RodSlot RodSlot
	{
		get
		{
			return this.Lure.RodSlot;
		}
	}

	protected Line1stBehaviour Line1st
	{
		get
		{
			return (Line1stBehaviour)this.RodSlot.Line;
		}
	}

	protected Reel1stBehaviour Reel1st
	{
		get
		{
			return (Reel1stBehaviour)this.RodSlot.Reel;
		}
	}

	protected bool IsInHands
	{
		get
		{
			return this.RodSlot.IsInHands;
		}
	}

	protected AssembledRod AssembledRod
	{
		get
		{
			return this.RodSlot.Rod.RodAssembly as AssembledRod;
		}
	}

	protected Transform HandTransform
	{
		get
		{
			Transform transform = null;
			ReelTypes reelType = GameFactory.Player.ReelType;
			if (reelType != ReelTypes.Spinning)
			{
				if (reelType == ReelTypes.Baitcasting)
				{
					transform = GameFactory.Player.LinePositionInRightHand;
				}
			}
			else
			{
				transform = GameFactory.Player.LinePositionInLeftHand;
			}
			return transform;
		}
	}

	protected Transform GripTransform
	{
		get
		{
			return GameFactory.Player.Grip.Fish;
		}
	}

	protected float CastLength()
	{
		Vector3 vector;
		vector..ctor(this.Lure.transform.position.x, 0f, this.Lure.transform.position.z);
		Vector3 vector2;
		vector2..ctor(GameFactory.Player.transform.position.x, 0f, GameFactory.Player.transform.position.z);
		return Vector3.Distance(vector, vector2);
	}
}
