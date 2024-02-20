using System;
using ObjectModel;
using UnityEngine;

public abstract class FeederStateBase : FsmBaseState<IFeederBehaviour>
{
	protected IFeederBehaviour Feeder
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
			return this.Feeder.RodSlot;
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

	protected void UpdateBottomIndicator()
	{
		if (GameFactory.BottomIndicator != null && GameFactory.BottomIndicator.IsShow)
		{
			if (this.Feeder.FeederObject != null)
			{
				GameFactory.BottomIndicator.SetBottomType(this.Feeder.FeederObject.LayerNameUnderObject, null);
			}
			if (this.Feeder.RigidBody.IsLying)
			{
				GameFactory.BottomIndicator.MoveBottom(-this.Feeder.RigidBody.Velocity.magnitude);
			}
			GameFactory.BottomIndicator.SetAngle((!this.Feeder.RigidBody.IsLying) ? 0f : (-70f));
			GameFactory.BottomIndicator.SetDepth(this.Feeder.RigidBody.Position.y, this.Feeder.RigidBody.GroundHeight, this.Feeder.RigidBody.IsLying);
		}
	}

	protected void UpdateQuiverIndicator()
	{
		if (this.player.Rod.IsQuiver && GameFactory.QuiverIndicator != null && GameFactory.QuiverIndicator.IsShow)
		{
			GameFactory.QuiverIndicator.SetPositions();
		}
	}

	protected void ResetLeaderLengthChange()
	{
		this.leaderShrinkTime = 0f;
		this.targetDistance = Mathf.Clamp(this.Feeder.UserSetLeaderLength, 0.3f, 0.75f);
	}

	protected void UpdateLeaderLength()
	{
		float num = this.Feeder.UserSetLeaderLength;
		if (this.RodSlot.Rod != null && this.RodSlot.Rod.RodAssembly != null && this.RodSlot.Rod.RodAssembly.RodInterface != null)
		{
			num = (this.RodSlot.Rod.RodAssembly.RodInterface as Rod).LeaderLength;
		}
		if (this.Feeder.UserSetLeaderLength != num)
		{
			this.Feeder.UserSetLeaderLength = num;
			this.ResetLeaderLengthChange();
		}
		if (this.leaderShrinkTime > 1.8f)
		{
			return;
		}
		this.leaderShrinkTime += Time.deltaTime;
		this.Feeder.LeaderLength = Mathf.Lerp(this.Feeder.LeaderLength, this.targetDistance, this.leaderShrinkTime / 1.8f);
	}

	protected float CastLength()
	{
		Vector3 vector;
		vector..ctor(this.Feeder.transform.position.x, 0f, this.Feeder.transform.position.z);
		Vector3 vector2;
		vector2..ctor(GameFactory.Player.transform.position.x, 0f, GameFactory.Player.transform.position.z);
		return Vector3.Distance(vector, vector2);
	}

	public const float ReelingBlockOnStrikeDuration = 0.5f;

	private float targetDistance;

	private float leaderShrinkTime;

	private const float LeaderTimeout = 1.8f;
}
