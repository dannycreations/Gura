using System;
using Phy;
using Phy.Verlet;
using UnityEngine;

public class FloatShowSmallFish : FloatStateBase
{
	protected override void onEnter()
	{
		this._owner.Adapter.CatchFish();
		base.RodSlot.Reel.IsIndicatorOn = false;
		base.Float.IsShowingComplete = false;
		this.realingOnTransitionToIdle = true;
		GameFactory.Player.InitTransitionToZeroXRotation();
		base.Line1st.ResetLineWidthChange(0.00075f);
		base.Float.HookSetFreeze(true);
		this.floatBehaviour = base.Float as Float1stBehaviour;
		if (this.floatBehaviour != null && this.floatBehaviour.IsUncuttable)
		{
			this.kinematicLineMass = this.floatBehaviour.GetKinematicLeashMass(0);
		}
		else
		{
			this.kinematicLineMass = base.Float.GetKinematicLeaderMass(0);
		}
		this.kinematicLineMass.IsKinematic = false;
		base.Float.KinematicHandLineMass = this.kinematicLineMass;
		this.fishHeadMass = base.Float.Fish.HeadMass;
		this.fishHeadMass.IsKinematic = true;
		this.fishHeadMass.StopMass();
		this.fishTailMass = base.Float.Fish.TailMass;
		this.headToLineDistance = 0.15f;
		this.initialLeaderLength = base.Float.LeaderLength;
		base.Float.Fish.Show();
		float num = this.kinematicLineMass.Position.y + (this.HandTransform.position.y - this.kinematicLineMass.Position.y) * 0.35f;
		this.flightCurve = new VerticalParabola(this.kinematicLineMass.Position - Vector3.up * this.headToLineDistance, this.HandTransform.position - Vector3.up * this.headToLineDistance, num - this.headToLineDistance);
		this.duration = 0.01f * (this.HandTransform.position - this.fishHeadMass.Position).magnitude;
		this.kvp = new KinematicVerticalParabola(this.fishHeadMass, this.fishTailMass, this.duration, 2f, this.flightCurve);
		this.kvp.DestinationPoint = this.HandTransform.position - Vector3.up * this.headToLineDistance;
		base.RodSlot.Sim.Connections.Add(this.kvp);
		base.RodSlot.Sim.RefreshObjectArrays(true);
		base.Float.Fish.FishObject.StopMasses();
	}

	protected override Type onUpdate()
	{
		if (base.AssembledRod.IsRodDisassembled)
		{
			base.Float.IsShowing = false;
			return typeof(FloatBroken);
		}
		if (base.Float.IsShowing && base.Float.Fish != null)
		{
			GameFactory.Player.TransitToZeroXRotation();
			base.RodSlot.Line.TransitToNewLineWidth();
			this.UpdateTacklePosition();
			return null;
		}
		base.Float.IsShowing = false;
		this._owner.Adapter.FinishGameAction();
		if (base.player.IsPitching)
		{
			return typeof(FloatIdlePitch);
		}
		return typeof(FloatOnTip);
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
				base.Float.Fish.FishObject.IgnoreEnvForces = true;
				base.Float.Fish.FishObject.StopMasses();
				this.kinematicLineMassDiverged = new VerletMass(base.RodSlot.Sim, 0.01f, this.HandTransform.position, Mass.MassType.Unknown);
				this.kinematicLineMassDiverged.Rotation = Quaternion.LookRotation(this.HandTransform.forward, Vector3.up);
				this.kinematicLineMassDiverged.IsKinematic = true;
				this.fishHeadMass.PriorSpring.Mass2 = this.kinematicLineMass;
				this.lastLineMass = this.fishHeadMass.PriorSpring.Mass1;
				this.fishHeadMass.PriorSpring = null;
				VerletMass[] array = new VerletMass[4];
				for (int i = 0; i < array.Length; i++)
				{
					array[i] = base.Float.Fish.FishObject.Masses[i] as VerletMass;
				}
				TetrahedronTorsionSpring tetrahedronTorsionSpring = new TetrahedronTorsionSpring(this.kinematicLineMassDiverged as VerletMass, array)
				{
					Torsion = 0f,
					SpringLength = 0.15f,
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
			this.kinematicLineMass.Position = this.HandTransform.position;
			this.kinematicLineMassDiverged.Position = this.HandTransform.position;
			this.kinematicLineMassDiverged.Rotation = Quaternion.LookRotation(this.HandTransform.forward, Vector3.up);
			if (!this.rotationFinalised)
			{
				GameFactory.Player.FinalizeTransitionToZeroXRotation();
				this.rotationFinalised = true;
			}
		}
		Vector3 vector = this.HandTransform.position - this.kinematicLineMass.Position;
		if (this.kvp.TimeSpent < this.duration)
		{
			Vector3 vector2 = Vector3.Lerp(this.flightCurve.GetPoint(num), this.HandTransform.position, Mathf.Pow(num, 2f));
			this.kvp.DestinationPoint = this.HandTransform.position - Vector3.up * this.headToLineDistance;
			this.kvp.ConnectionNeedSyncMark();
		}
		else
		{
			this.kinematicLineMass.Position = this.HandTransform.position;
			if (GameFactory.Player.State == typeof(PlayerShowFishLineIdle) && this.realingOnTransitionToIdle)
			{
				this.realingOnTransitionToIdle = false;
				base.Float.IsShowingComplete = true;
			}
		}
		float num2 = (base.RodSlot.Rod.CurrentUnbendTipPosition - this.kinematicLineMass.Position).magnitude - base.Float.Size - base.Float.LeaderLength;
		base.RodSlot.Sim.FinalLineLength = Mathf.Clamp(num2, base.RodSlot.Line.MinLineLength, float.PositiveInfinity) * 0.75f;
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

	protected override void onExit()
	{
		base.Float.UserSetLeaderLength = base.Float.UserSetLeaderLength;
		base.Line1st.LineObj.IsKinematic = false;
		base.Float.IsKinematic = false;
		base.RodSlot.SimThread.StopLine();
		if (this.kinematicLineMassDiverged != null)
		{
			this.kinematicLineMassDiverged.IsKinematic = false;
		}
		if (this.kinematicLineMass != null)
		{
			base.Line1st.ReleaseKinematicMass(this.kinematicLineMass);
		}
		base.Float.HookSetFreeze(false);
		base.Float.SetHookMass(0, this.lastLineMass);
		base.Float.SetHookMass(1, this.lastLineMass);
		this.kinematicLineMass = null;
		base.RodSlot.Sim.RemoveConnection(this.kvp);
		base.RodSlot.Sim.RefreshObjectArrays(true);
	}

	private const float durationFactor = 0.25f;

	private const float flightMidpointYRatio = 0.35f;

	private const float lineStretch = 0.75f;

	private const float lineHangLength = 0.15f;

	private float timeSpent;

	private bool isKinematic;

	private Mass kinematicLineMass;

	private Mass kinematicLineMassDiverged;

	private Mass lastLineMass;

	private Mass fishHeadMass;

	private Mass fishTailMass;

	private float headToLineDistance;

	private bool rotationFinalised;

	private float initialLeaderLength;

	private bool realingOnTransitionToIdle;

	private VerticalParabola flightCurve;

	private float duration;

	private KinematicVerticalParabola kvp;

	private Float1stBehaviour floatBehaviour;
}
