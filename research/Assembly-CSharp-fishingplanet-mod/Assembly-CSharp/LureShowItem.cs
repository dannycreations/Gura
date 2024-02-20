using System;
using ObjectModel;
using Phy;
using UnityEngine;

public class LureShowItem : LureStateBase
{
	protected override void onEnter()
	{
		this._owner.Adapter.CatchItem();
		base.Lure.ItemIsShowing = true;
		base.Lure.IsShowing = true;
		base.Lure.UnderwaterItem.NeedUpdate = false;
		base.Lure.IsShowingComplete = false;
		this.realingOnTransitionToIdle = true;
		base.RodSlot.Reel.IsIndicatorOn = false;
		this.timeSpent = 0f;
		GameFactory.Player.InitTransitionToZeroXRotation();
		base.Line1st.ResetLineWidthChange(0.00075f);
		base.Lure.SetFreeze(true);
		this.kinematicLineMass = null;
		this.lure = base.Lure as Lure1stBehaviour;
		if (this.lure != null)
		{
			if (this.lure.IsUncuttable)
			{
				this.kinematicLineMass = this.lure.GetKinematicLeaderMass(0);
			}
			else if (this.lure.RodTemplate.IsSinkerRig())
			{
				if (this.lure.RodTemplate != RodTemplate.TexasRig)
				{
					this.kinematicLineMass = this.lure.GetKinematicLeaderMass(0);
				}
				else
				{
					this.lure.IsVisibleLeader = false;
					this.kinematicLineMass = this.lure.GetKinematicLeashMass(0);
				}
			}
		}
		if (this.kinematicLineMass == null)
		{
			this.kinematicLineMass = base.Line1st.GetKinematicMass();
		}
		base.Lure.KinematicHandLineMass = this.kinematicLineMass;
		this.kinematicLineMass.IsKinematic = false;
		this.kinematicLineMass.StopMass();
		this.behaviour = base.Lure.UnderwaterItem.Behaviour as UnderwaterItem1stBehaviour;
		if (this.behaviour != null)
		{
			this.itemHeadMass = this.behaviour.phyObject.ForHookMass;
			this.headToLineDistance = Mathf.Max((GameFactory.Player.ReelType != ReelTypes.Baitcasting) ? 0.2f : 0.1f, (base.Lure.TopLineAnchor.position - base.Lure.HookAnchor.position).magnitude + 0.05f);
			this.itemHeadMass.IsKinematic = true;
			this.itemHeadMass.StopMass();
			this.itemTailMass = this.behaviour.phyObject.TailMass;
			float num = this.kinematicLineMass.Position.y + (base.HandTransform.position.y - this.kinematicLineMass.Position.y) * 0.35f;
			this.flightCurve = new VerticalParabola(this.kinematicLineMass.Position - Vector3.up * this.headToLineDistance, base.HandTransform.position - Vector3.up * this.headToLineDistance, num - this.headToLineDistance);
			this.duration = 0.25f * (base.HandTransform.position - this.itemHeadMass.Position).magnitude;
			this.kvp = new KinematicVerticalParabola(this.itemHeadMass, this.itemTailMass ?? this.itemHeadMass, this.duration, 2f, this.flightCurve)
			{
				DestinationPoint = base.HandTransform.position - Vector3.up * this.headToLineDistance
			};
			base.RodSlot.Sim.Connections.Add(this.kvp);
			base.RodSlot.Sim.RefreshObjectArrays(true);
			this.behaviour.phyObject.StopMasses();
		}
		base.RodSlot.SimThread.StopLine();
	}

	protected override Type onUpdate()
	{
		if (base.AssembledRod.IsRodDisassembled)
		{
			base.Lure.IsShowing = false;
			return typeof(LureBroken);
		}
		if (base.Lure.IsShowing && !(base.Lure.UnderwaterItem == null))
		{
			GameFactory.Player.TransitToZeroXRotation();
			base.RodSlot.Line.TransitToNewLineWidth();
			this.UpdateTacklePosition();
			return null;
		}
		base.Lure.IsShowing = false;
		this._owner.Adapter.FinishGameAction();
		if (base.player.IsPitching)
		{
			return typeof(LureIdlePitch);
		}
		return typeof(LureOnTip);
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
				this.kinematicLineMassDiverged = new Mass(base.RodSlot.Sim, 0.01f, base.HandTransform.position, Mass.MassType.Auxiliary)
				{
					IsKinematic = true
				};
				this.disconnectionLineMassDummy = new Mass(base.RodSlot.Sim, 0.01f, base.HandTransform.position, Mass.MassType.Auxiliary);
				this.kinematicLineMass.IsKinematic = false;
				this.kinematicLineMass.StopMass();
				this.itemHeadMass.PriorSpring.Mass2 = this.disconnectionLineMassDummy;
				this.lastLineMass = this.itemHeadMass.PriorSpring.Mass1;
				this.itemHeadMass.PriorSpring = null;
				Spring spring = new Spring(this.kinematicLineMassDiverged, this.itemHeadMass, 500f, this.headToLineDistance, 0.02f);
				base.RodSlot.Sim.Masses.Add(this.kinematicLineMassDiverged);
				base.RodSlot.Sim.Masses.Add(this.disconnectionLineMassDummy);
				base.RodSlot.Sim.Connections.Add(spring);
				this.holdLine = new KinematicSpline(this.kinematicLineMassDiverged, false);
				base.RodSlot.Sim.Connections.Add(this.holdLine);
				base.RodSlot.Sim.RefreshObjectArrays(true);
				this.kinematicLineMass.IsKinematic = true;
				this.kinematicLineMass.StopMass();
				this.behaviour.phyObject.StopMasses();
				GameFactory.Player.ShowCatchedItemDialog(base.Lure.UnderwaterItemName, base.Lure.UnderwaterItemCategory, base.Lure.UnderwaterItemId);
			}
			this.kinematicLineMass.Position = base.HandTransform.position;
			this.holdLine.SetNextPointAndRotation(base.HandTransform.position, Quaternion.identity);
			if (!this.rotationFinalised)
			{
				GameFactory.Player.FinalizeTransitionToZeroXRotation();
				this.rotationFinalised = true;
			}
		}
		if (this.kvp.TimeSpent < this.duration)
		{
			Vector3 vector = Vector3.Lerp(this.flightCurve.GetPoint(num), base.HandTransform.position, Mathf.Pow(num, 2f));
			this.kvp.DestinationPoint = base.HandTransform.position - Vector3.up * this.headToLineDistance;
			this.kvp.ConnectionNeedSyncMark();
		}
		else
		{
			this.kinematicLineMass.Position = base.HandTransform.position;
			if (GameFactory.Player.State == typeof(PlayerShowFishLineIdle) && this.realingOnTransitionToIdle)
			{
				this.realingOnTransitionToIdle = false;
			}
			base.Lure.IsShowingComplete = true;
		}
		Vector3 vector2 = base.RodSlot.Rod.CurrentUnbendTipPosition - this.kinematicLineMass.Position;
		base.RodSlot.Sim.FinalLineLength = vector2.magnitude * 0.75f;
		base.Line1st.LineObj.Springs[base.Line1st.LineObj.Springs.Count - 1].SpringLength = 0.1f;
	}

	protected override void onExit()
	{
		base.Line1st.LineObj.IsKinematic = false;
		base.Lure.SetFreeze(false);
		base.RodSlot.SimThread.StopLine();
		if (this.kinematicLineMassDiverged != null)
		{
			this.kinematicLineMassDiverged.IsKinematic = false;
		}
		if (this.kinematicLineMass != null)
		{
			base.Line1st.ReleaseKinematicMass(this.kinematicLineMass);
		}
		if (this.lure != null && this.lure.RodTemplate == RodTemplate.TexasRig)
		{
			this.lure.IsVisibleLeader = true;
		}
		base.Lure.SetFreeze(false);
		this.kinematicLineMass = null;
		base.RodSlot.Sim.RemoveConnection(this.kvp);
		base.RodSlot.Sim.RemoveConnection(this.holdLine);
		base.RodSlot.Sim.RefreshObjectArrays(true);
		base.Lure.ItemIsShowing = false;
	}

	private const float durationFactor = 0.25f;

	private const float flightMidpointYRatio = 0.35f;

	private const float lineStretch = 0.75f;

	private const float lineHangFrictionConstant = 0.02f;

	private const float lineHangLength = 0.2f;

	private const float lineHangCastLength = 0.1f;

	private const float auxiliaryMassValue = 0.01f;

	private float timeSpent;

	private bool rotationFinalised;

	private Mass kinematicLineMass;

	private Mass kinematicLineMassDiverged;

	private Mass disconnectionLineMassDummy;

	private Mass lastLineMass;

	private Mass itemHeadMass;

	private Mass itemTailMass;

	private float headToLineDistance;

	private bool realingOnTransitionToIdle;

	private VerticalParabola flightCurve;

	private float duration;

	private KinematicVerticalParabola kvp;

	private KinematicSpline holdLine;

	private UnderwaterItem1stBehaviour behaviour;

	private Lure1stBehaviour lure;
}
