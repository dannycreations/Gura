using System;
using Phy;
using UnityEngine;

public class FeederShowItem : FeederStateBase
{
	protected override void onEnter()
	{
		this._owner.Adapter.CatchItem();
		base.RodSlot.Reel.IsIndicatorOn = false;
		this.timeSpent = 0f;
		base.Feeder.ItemIsShowing = true;
		base.Feeder.IsShowing = true;
		base.Feeder.UnderwaterItem.NeedUpdate = false;
		base.Feeder.IsShowingComplete = false;
		this.realingOnTransitionToIdle = true;
		GameFactory.Player.InitTransitionToZeroXRotation();
		base.Line1st.ResetLineWidthChange(0.00075f);
		base.Feeder.HookSetFreeze(true);
		base.Feeder.LeaderLength = base.RodSlot.Line.MinLineLength;
		this.kinematicLineMass = base.Feeder.GetKinematicLeaderMass(0);
		base.Feeder.KinematicHandLineMass = this.kinematicLineMass;
		this.kinematicLineMass.IsKinematic = false;
		this.kinematicLineMass.StopMass();
		this.behaviour = base.Feeder.UnderwaterItem.Behaviour as UnderwaterItem1stBehaviour;
		if (this.behaviour != null)
		{
			this.itemHeadMass = this.behaviour.phyObject.ForHookMass;
			this.itemHeadMass.IsKinematic = true;
			this.itemHeadMass.StopMass();
			this.itemTailMass = this.behaviour.phyObject.TailMass;
			this.headToLineDistance = 0.2f;
			this.initialLeaderLength = base.Feeder.LeaderLength;
			float num = this.kinematicLineMass.Position.y + (this.HandTransform.position.y - this.kinematicLineMass.Position.y) * 0.35f;
			this.flightCurve = new VerticalParabola(this.kinematicLineMass.Position - Vector3.up * this.headToLineDistance, this.HandTransform.position - Vector3.up * this.headToLineDistance, num - this.headToLineDistance);
			this.duration = 0.25f * (this.HandTransform.position - this.itemHeadMass.Position).magnitude;
			this.kvp = new KinematicVerticalParabola(this.itemHeadMass, this.itemTailMass ?? this.itemHeadMass, this.duration, 2f, this.flightCurve)
			{
				DestinationPoint = this.HandTransform.position - Vector3.up * this.headToLineDistance
			};
			base.RodSlot.Sim.Connections.Add(this.kvp);
			base.RodSlot.Sim.RefreshObjectArrays(true);
			this.behaviour.phyObject.StopMasses();
		}
	}

	protected override Type onUpdate()
	{
		if (base.Feeder.IsShowing && !(base.Feeder.UnderwaterItem == null))
		{
			GameFactory.Player.TransitToZeroXRotation();
			base.RodSlot.Line.TransitToNewLineWidth();
			this.UpdateTacklePosition();
			return null;
		}
		base.Feeder.IsShowing = false;
		this._owner.Adapter.FinishGameAction();
		if (base.player.IsPitching)
		{
			return typeof(FeederIdlePitch);
		}
		return typeof(FeederOnTip);
	}

	private void UpdateTacklePosition()
	{
		float num = this.timeSpent / this.duration;
		if (this.kvp.TimeSpent < this.duration)
		{
			this.timeSpent = Mathf.Clamp(this.timeSpent + Time.deltaTime, 0f, this.duration);
		}
		if (this.kvp.TimeSpent >= this.duration)
		{
			if (!this.kinematicLineMass.IsKinematic)
			{
				this.itemHeadMass.IsKinematic = false;
				this.behaviour.phyObject.IgnoreEnvForces = true;
				this.behaviour.phyObject.StopMasses();
				this.kinematicLineMassDiverged = new Mass(base.RodSlot.Sim, 0.01f, this.HandTransform.position, Mass.MassType.Auxiliary)
				{
					IsKinematic = true
				};
				this.disconnectionLineMassDummy = new Mass(base.RodSlot.Sim, 0.01f, this.HandTransform.position, Mass.MassType.Auxiliary);
				this.itemHeadMass.PriorSpring.Mass2 = this.disconnectionLineMassDummy;
				this.lastLineMass = this.itemHeadMass.PriorSpring.Mass1;
				this.itemHeadMass.PriorSpring = null;
				Spring spring = new Spring(this.kinematicLineMassDiverged, this.itemHeadMass, 500f, 0.2f, 0.02f);
				base.RodSlot.Sim.Masses.Add(this.kinematicLineMassDiverged);
				base.RodSlot.Sim.Masses.Add(this.disconnectionLineMassDummy);
				base.RodSlot.Sim.Connections.Add(spring);
				this.holdLine = new KinematicSpline(this.kinematicLineMassDiverged, false);
				base.RodSlot.Sim.Connections.Add(this.holdLine);
				base.RodSlot.Sim.RefreshObjectArrays(true);
				this.kinematicLineMass.IsKinematic = true;
				this.kinematicLineMass.StopMass();
				this.behaviour.phyObject.StopMasses();
				GameFactory.Player.ShowCatchedItemDialog(base.Feeder.UnderwaterItemName, base.Feeder.UnderwaterItemCategory, base.Feeder.UnderwaterItemId);
			}
			this.kinematicLineMass.Position = this.HandTransform.position;
			this.holdLine.SetNextPointAndRotation(this.HandTransform.position, Quaternion.identity);
			if (!this.rotationFinalised)
			{
				GameFactory.Player.FinalizeTransitionToZeroXRotation();
				this.rotationFinalised = true;
			}
		}
		if (this.kvp.TimeSpent < this.duration)
		{
			Vector3 vector = Vector3.Lerp(this.flightCurve.GetPoint(num), this.HandTransform.position, Mathf.Pow(num, 2f));
			this.kvp.DestinationPoint = this.HandTransform.position - Vector3.up * this.headToLineDistance;
			this.kvp.ConnectionNeedSyncMark();
		}
		else
		{
			this.kinematicLineMass.Position = this.HandTransform.position;
			if (GameFactory.Player.State == typeof(PlayerShowFishLineIdle) && this.realingOnTransitionToIdle)
			{
				this.realingOnTransitionToIdle = false;
			}
			base.Feeder.IsShowingComplete = true;
		}
		float num2 = (base.RodSlot.Rod.CurrentUnbendTipPosition - this.kinematicLineMass.Position).magnitude - base.Feeder.Size - base.Feeder.LeaderLength;
		base.RodSlot.Sim.FinalLineLength = Mathf.Clamp(num2, base.RodSlot.Line.MinLineLength, float.PositiveInfinity) * 0.75f;
	}

	protected override void onExit()
	{
		this.kinematicLineMass.IsKinematic = false;
		base.Feeder.UserSetLeaderLength = base.Feeder.UserSetLeaderLength;
		base.Line1st.LineObj.IsKinematic = false;
		base.Feeder.IsKinematic = false;
		base.RodSlot.SimThread.StopLine();
		if (this.kinematicLineMassDiverged != null)
		{
			this.kinematicLineMassDiverged.IsKinematic = false;
		}
		if (this.kinematicLineMass != null)
		{
			base.Line1st.ReleaseKinematicMass(this.kinematicLineMass);
		}
		base.Feeder.HookSetFreeze(false);
		base.Feeder.SetHookMass(0, this.lastLineMass);
		base.Feeder.SetHookMass(1, this.lastLineMass);
		this.kinematicLineMass = null;
		base.RodSlot.Sim.RemoveConnection(this.kvp);
		base.RodSlot.Sim.RemoveConnection(this.holdLine);
		base.RodSlot.Sim.RefreshObjectArrays(true);
		base.Feeder.ItemIsShowing = false;
	}

	private Transform HandTransform
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

	private const float durationFactor = 0.25f;

	private const float flightMidpointYRatio = 0.35f;

	private const float lineHangLength = 0.2f;

	private const float lineHangFrictionConstant = 0.02f;

	private const float lineStretch = 0.75f;

	private const float auxiliaryMassValue = 0.01f;

	private float timeSpent;

	private bool isKinematic;

	private Mass kinematicLineMass;

	private Mass kinematicLineMassDiverged;

	private Mass disconnectionLineMassDummy;

	private Mass lastLineMass;

	private Mass itemHeadMass;

	private Mass itemTailMass;

	private float headToLineDistance;

	private bool rotationFinalised;

	private float initialLeaderLength;

	private bool realingOnTransitionToIdle;

	private VerticalParabola flightCurve;

	private float duration;

	private KinematicVerticalParabola kvp;

	private KinematicSpline holdLine;

	private UnderwaterItem1stBehaviour behaviour;
}
