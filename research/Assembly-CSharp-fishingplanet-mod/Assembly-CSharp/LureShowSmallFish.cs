using System;
using ObjectModel;
using Phy;
using Phy.Verlet;
using UnityEngine;

public class LureShowSmallFish : LureStateBase
{
	protected override void onEnter()
	{
		this._owner.Adapter.CatchFish();
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
			if (this.lure.SwimbaitOwner != null)
			{
				this.lure.SwimbaitOwner.hinge1Anchor.localRotation = Quaternion.identity;
				this.lure.SwimbaitOwner.hinge2Anchor.localRotation = Quaternion.identity;
				this.lure.SwimbaitOwner.hinge3Anchor.localRotation = Quaternion.identity;
			}
		}
		if (this.kinematicLineMass == null)
		{
			this.kinematicLineMass = base.Line1st.GetKinematicMass();
		}
		this.kinematicLineMass.IsKinematic = false;
		base.Lure.KinematicHandLineMass = this.kinematicLineMass;
		this.fishHeadMass = base.Lure.Fish.FishObject.Masses[0];
		this.headToLineDistance = Mathf.Max(0.15f, (base.Lure.TopLineAnchor.position - base.Lure.HookAnchor.position).magnitude + 0.05f);
		if (this.fishHeadMass is PointOnRigidBody)
		{
			this.fishHeadMass = (this.fishHeadMass as PointOnRigidBody).RigidBody;
		}
		this.fishTailMass = base.Lure.Fish.FishObject.Masses[16];
		this.fishHeadMass.IsKinematic = true;
		this.fishHeadMass.StopMass();
		base.Lure.Fish.Show();
		float num = this.kinematicLineMass.Position.y + (base.HandTransform.position.y - this.kinematicLineMass.Position.y) * 0.35f;
		Debug.DrawLine(this.kinematicLineMass.Position, base.HandTransform.position, Color.magenta, 10f);
		this.flightCurve = new VerticalParabola(this.kinematicLineMass.Position - Vector3.up * this.headToLineDistance, base.HandTransform.position - Vector3.up * this.headToLineDistance, num - this.headToLineDistance);
		this.duration = 0.01f * (base.HandTransform.position - this.fishHeadMass.Position).magnitude;
		this.kvp = new KinematicVerticalParabola(this.fishHeadMass, this.fishTailMass, this.duration, 2f, this.flightCurve);
		this.kvp.DestinationPoint = base.HandTransform.position - Vector3.up * this.headToLineDistance;
		base.RodSlot.Sim.Connections.Add(this.kvp);
		base.RodSlot.Sim.RefreshObjectArrays(true);
		base.RodSlot.SimThread.StopLine();
	}

	protected override Type onUpdate()
	{
		if (base.AssembledRod.IsRodDisassembled)
		{
			base.Lure.IsShowing = false;
			return typeof(LureBroken);
		}
		if (base.Lure.IsShowing && base.Lure.Fish != null)
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
				this.fishHeadMass.IsKinematic = false;
				base.Lure.Fish.FishObject.IgnoreEnvForces = true;
				base.Lure.Fish.FishObject.StopMasses();
				this.kinematicLineMassDiverged = new VerletMass(base.RodSlot.Sim, 0.01f, base.HandTransform.position, Mass.MassType.Unknown)
				{
					Rotation = Quaternion.LookRotation(base.HandTransform.forward, Vector3.up),
					IsKinematic = true
				};
				this.fishHeadMass.PriorSpring.Mass2 = this.kinematicLineMass;
				this.lastLineMass = this.fishHeadMass.PriorSpring.Mass1;
				this.fishHeadMass.PriorSpring = null;
				VerletMass[] array = new VerletMass[4];
				for (int i = 0; i < array.Length; i++)
				{
					array[i] = base.Lure.Fish.FishObject.Masses[i] as VerletMass;
				}
				TetrahedronTorsionSpring tetrahedronTorsionSpring = new TetrahedronTorsionSpring(this.kinematicLineMassDiverged as VerletMass, array)
				{
					Torsion = 0f,
					SpringLength = this.headToLineDistance,
					SpringFriction = 0.2f,
					TorsionStiffness = 0.1f,
					TorsionFriction = 5f,
					BendStiffness = 0f,
					BendFriction = 0f
				};
				base.RodSlot.Sim.Masses.Add(this.kinematicLineMassDiverged);
				base.RodSlot.Sim.Connections.Add(tetrahedronTorsionSpring);
				base.RodSlot.Sim.RefreshObjectArrays(true);
				this.kinematicLineMass.IsKinematic = true;
				this.kinematicLineMass.StopMass();
			}
			this.kinematicLineMass.Position = base.HandTransform.position;
			this.kinematicLineMassDiverged.Position = base.HandTransform.position;
			this.kinematicLineMassDiverged.Rotation = Quaternion.LookRotation(base.HandTransform.forward, Vector3.up);
			if (!this.rotationFinalised)
			{
				GameFactory.Player.FinalizeTransitionToZeroXRotation();
				this.rotationFinalised = true;
			}
		}
		Vector3 vector = base.HandTransform.position - this.kinematicLineMass.Position;
		if (this.kvp.TimeSpent < this.duration)
		{
			Vector3 vector2 = Vector3.Lerp(this.flightCurve.GetPoint(num), base.HandTransform.position, Mathf.Pow(num, 2f));
			this.kvp.DestinationPoint = base.HandTransform.position - Vector3.up * this.headToLineDistance;
			this.kvp.ConnectionNeedSyncMark();
		}
		else
		{
			this.kinematicLineMass.Position += vector;
			if (GameFactory.Player.State == typeof(PlayerShowFishLineIdle) && this.realingOnTransitionToIdle)
			{
				this.realingOnTransitionToIdle = false;
				base.Lure.IsShowingComplete = true;
			}
		}
		Vector3 vector3 = base.RodSlot.Rod.CurrentUnbendTipPosition - this.kinematicLineMass.Position;
		base.RodSlot.Sim.FinalLineLength = vector3.magnitude * 0.75f;
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
		if (this.lure != null)
		{
			if (this.lure.RodTemplate == RodTemplate.TexasRig)
			{
				this.lure.IsVisibleLeader = true;
			}
			if (this.lure.BaitObject != null)
			{
				this.lure.SetForward(this.lure.BaitObject, this.lure.BaitTopAnchor.position + this.lure.BaitShift, this.lure.BaitTopAnchor.position - this.lure.BaitAnchor.position);
			}
		}
		base.Lure.SetFreeze(false);
		this.kinematicLineMass = null;
		base.RodSlot.Sim.RemoveConnection(this.kvp);
		base.RodSlot.Sim.RefreshObjectArrays(true);
	}

	private const float durationFactor = 0.25f;

	private const float flightMidpointYRatio = 0.35f;

	private const float lineStretch = 0.75f;

	private const float lineHangLength = 0.15f;

	private float timeSpent;

	private bool rotationFinalised;

	private Mass kinematicLineMass;

	private Mass kinematicLineMassDiverged;

	private Mass lastLineMass;

	private Mass fishHeadMass;

	private Mass fishTailMass;

	private float headToLineDistance;

	private bool realingOnTransitionToIdle;

	private VerticalParabola flightCurve;

	private float duration;

	private KinematicVerticalParabola kvp;

	private Lure1stBehaviour lure;
}
