using System;
using ObjectModel;
using UnityEngine;

public abstract class FloatStateBase : FsmBaseState<IFloatBehaviour>
{
	protected IFloatBehaviour Float
	{
		get
		{
			return this._owner;
		}
	}

	protected GameFactory.RodSlot RodSlot
	{
		get
		{
			return this.Float.RodSlot;
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

	protected PlayerController player
	{
		get
		{
			return GameFactory.Player;
		}
	}

	protected void ResetLeaderLengthChange()
	{
		this.leaderShrinkTime = 0f;
		this.targetDistance = Mathf.Clamp(this.Float.UserSetLeaderLength, 0.3f, 0.75f);
	}

	protected void UpdateLeaderLength()
	{
		float num = this.Float.UserSetLeaderLength;
		if (this.RodSlot.Rod != null && this.RodSlot.Rod.RodAssembly != null && this.RodSlot.Rod.RodAssembly.RodInterface != null)
		{
			num = (this.RodSlot.Rod.RodAssembly.RodInterface as Rod).LeaderLength;
		}
		if (this.Float.UserSetLeaderLength != num)
		{
			this.Float.UserSetLeaderLength = num;
			this.ResetLeaderLengthChange();
		}
		if (this.leaderShrinkTime > 1.8f)
		{
			return;
		}
		this.leaderShrinkTime += Time.deltaTime;
		this.Float.LeaderLength = Mathf.Lerp(this.Float.LeaderLength, this.targetDistance, this.leaderShrinkTime / 1.8f);
	}

	protected float CastLength()
	{
		Vector3 vector;
		vector..ctor(this.Float.transform.position.x, 0f, this.Float.transform.position.z);
		Vector3 vector2;
		vector2..ctor(GameFactory.Player.transform.position.x, 0f, GameFactory.Player.transform.position.z);
		return Vector3.Distance(vector, vector2);
	}

	public const float ReelingBlockOnStrikeDuration = 0.5f;

	private float targetDistance;

	private float leaderShrinkTime;

	private const float LeaderTimeout = 1.8f;
}
