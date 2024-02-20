using System;
using Phy;
using Phy.Verlet;
using UnityEngine;

public class FeederShowBigFish : FeederStateBase
{
	protected override void onEnter()
	{
		this._owner.Adapter.CatchFish();
		base.Feeder.IsShowingComplete = false;
		base.RodSlot.Reel.IsIndicatorOn = false;
		base.RodSlot.Sim.TurnLimitsOn(true);
		this.timeSpent = 0f;
		GameFactory.Player.InitTransitionToZeroXRotation();
		base.Line1st.ResetLineWidthChange(0.00075f);
		base.Feeder.Fish.Show();
		base.Feeder.HookSetFreeze(true);
		this.fishBody = base.Feeder.Fish.FishObject as AbstractFishBody;
		this.fishHeadMass = this.fishBody.Masses[0];
		this.fishTailMass = base.Feeder.Fish.TailMass;
		this.fishBody.StopMasses();
		float num = this.fishHeadMass.Position.y + (this.GripTransform.position.y - this.fishHeadMass.Position.y) * 0.35f;
		Debug.DrawLine(this.fishHeadMass.Position, this.GripTransform.position, Color.magenta, 10f);
		this.flightCurve = new VerticalParabola(this.fishHeadMass.Position, this.GripTransform.position, num);
		this.fishHeadMass.IsKinematic = true;
		base.Feeder.KinematicHandLineMass = base.RodSlot.Sim.RodTipMass;
		this.duration = TackleBehaviour.ShowGripInAnimDuration(base.Feeder.Fish);
		this.kvp = new KinematicVerticalParabola(this.fishHeadMass, this.fishTailMass, this.duration, 2f, this.flightCurve)
		{
			DestinationPoint = this.GripTransform.position
		};
		base.RodSlot.Sim.Connections.Add(this.kvp);
		base.RodSlot.Sim.RefreshObjectArrays(true);
	}

	protected override Type onUpdate()
	{
		if (base.AssembledRod.IsRodDisassembled)
		{
			base.Feeder.IsShowing = false;
			return typeof(FeederBroken);
		}
		if (base.Feeder.IsShowing && base.Feeder.Fish != null)
		{
			this.timeSpent += Time.deltaTime;
			GameFactory.Player.TransitToZeroXRotation();
			base.RodSlot.Line.TransitToNewLineWidth();
			if (this.kvp.TimeSpent < this.duration)
			{
				this.UpdateFishPool();
			}
			else
			{
				this.fishHeadMass.Motor = Vector3.zero;
				if (this.lineDisconnectionMass == null)
				{
					base.Feeder.HookSetFreeze(false);
					this.fishHeadMass.IsKinematic = true;
					this.fishCollarMasses = new Mass[3];
					this.fishCollarConnections = new VerletSpring[3];
					for (int i = 0; i < this.fishCollarMasses.Length; i++)
					{
						this.fishCollarMasses[i] = new VerletMass(base.RodSlot.Sim, 0.1f, this.fishBody.Masses[i + 1].Position, Mass.MassType.Unknown)
						{
							IsKinematic = true
						};
						base.RodSlot.Sim.Masses.Add(this.fishCollarMasses[i]);
						this.fishCollarConnections[i] = new VerletSpring(this.fishCollarMasses[i] as VerletMass, this.fishBody.Masses[i + 1] as VerletMass, 0.025f, 0.005f, false);
						base.RodSlot.Sim.Connections.Add(this.fishCollarConnections[i]);
					}
					GameFactory.Player.Grip.SetGameVisibility(true);
					this.fishBody.Masses[1].MotionDamping = 0.999f;
					this.fishBody.Masses[2].MotionDamping = 0.999f;
					this.fishBody.Masses[3].MotionDamping = 0.999f;
					VerletMass[] array = new VerletMass[4];
					for (int j = 0; j < array.Length; j++)
					{
						array[j] = this.fishBody.Masses[j] as VerletMass;
					}
					this.lineDisconnectionMass = new Mass(base.RodSlot.Sim, 0.001f, this.fishHeadMass.Position, Mass.MassType.Unknown)
					{
						IsKinematic = true
					};
					this.lineHeadMassSpring = this.fishHeadMass.PriorSpring;
					this.lineHeadMassSpring.Mass2 = this.lineDisconnectionMass;
					this.lineDisconnectionMass.PriorSpring = this.lineHeadMassSpring;
					this.fishHeadMass.PriorSpring = null;
					base.RodSlot.Sim.Masses.Add(this.lineDisconnectionMass);
					this.fishBody.StopMasses();
					base.Feeder.SetHookMass(1, this.lineDisconnectionMass);
					base.RodSlot.Sim.RefreshObjectArrays(true);
				}
				this.UpdateTacklePosition();
				if (!this.rotationFinalised)
				{
					GameFactory.Player.FinalizeTransitionToZeroXRotation();
					this.rotationFinalised = true;
				}
			}
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

	private void UpdateFishPool()
	{
		float progress = this.kvp.Progress;
		GameFactory.Player.MovedDown = Mathf.Lerp(0f, 1f, progress);
		base.RodSlot.Sim.FinalLineLength = base.RodSlot.Line.MinLineLength;
		Vector3 vector = base.Feeder.Fish.HeadMass.Position - base.Feeder.HookAnchor.position;
		base.Feeder.HookTransform.position += vector;
		Vector3 normalized = (base.Feeder.GetHookMass(0).Position - base.Feeder.GetHookMass(1).Position).normalized;
		Vector3 segmentRight = this.fishBody.GetSegmentRight(0);
		base.Feeder.HookTransform.rotation = Quaternion.FromToRotation(Vector3.up, normalized);
		this.kvp.DestinationPoint = this.HandTransform.position;
		this.kvp.ConnectionNeedSyncMark();
	}

	private void UpdateTacklePosition()
	{
		Vector3 vector = Quaternion.AngleAxis(-30f + base.Feeder.Fish.Owner.GripRotation, Vector3.up) * this.GripTransform.right;
		if (this.lineDisconnectionMass != null)
		{
			this.fishHeadMass.Position = this.GripTransform.position;
			this.fishHeadMass.Rotation = Quaternion.LookRotation(vector, Vector3.up);
		}
		if (this.fishCollarMasses != null)
		{
			Vector3[] idealTetrahedronPositions = this.fishBody.GetIdealTetrahedronPositions(this.GripTransform.position - this.fishBody.TetrahedronHeight * Vector3.up, Vector3.Cross(Vector3.up, vector), vector, Vector3.up);
			for (int i = 0; i < this.fishCollarMasses.Length; i++)
			{
				this.fishCollarMasses[i].Position = idealTetrahedronPositions[i + 1];
			}
		}
		float y = this.HandTransform.rotation.eulerAngles.y;
		base.Feeder.HookTransform.rotation = Quaternion.Euler(-50f, y, 0f);
		if (this.lineDisconnectionMass.IsKinematic)
		{
			Vector3 vector2 = base.RodSlot.Sim.rodObject.TipMass.Position + (base.RodSlot.Sim.rodObject.RootMass.Position - base.RodSlot.Sim.rodObject.TipMass.Position).normalized * (base.RodSlot.Sim.FinalLineLength + base.Feeder.Size + base.Feeder.LeaderLength);
			this.lineDisconnectionMass.Position = Vector3.Lerp(vector2, this.lineDisconnectionMass.Position, Mathf.Exp(-Time.deltaTime * 100f));
		}
		base.Feeder.HookTransform.position = this.lineDisconnectionMass.Position;
		float num = (base.RodSlot.Rod.CurrentUnbendTipPosition - base.Feeder.GetHookMass(0).Position).magnitude - base.Feeder.Size - base.Feeder.LeaderLength;
		base.RodSlot.Sim.FinalLineLength = base.RodSlot.Line.MinLineLength;
		GameFactory.Player.MovedDown = Mathf.Lerp(1f, 0f, (this.timeSpent - this.duration) / 1f);
		if (this.timeSpent > this.duration + 1f)
		{
			base.Feeder.IsShowingComplete = true;
		}
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

	private Transform GripTransform
	{
		get
		{
			return GameFactory.Player.Grip.Fish;
		}
	}

	protected override void onExit()
	{
		GameFactory.Player.Grip.SetGameVisibility(false);
		if (this.fishCollarMasses != null)
		{
			for (int i = 0; i < this.fishCollarMasses.Length; i++)
			{
				base.RodSlot.Sim.RemoveMass(this.fishCollarMasses[i]);
				base.RodSlot.Sim.RemoveConnection(this.fishCollarConnections[i]);
			}
		}
		if (base.Feeder.Fish != null)
		{
			this.fishBody.IsKinematic = false;
			this.fishBody.StopMasses();
		}
		base.Feeder.UserSetLeaderLength = base.Feeder.UserSetLeaderLength;
		base.Feeder.HookSetFreeze(false);
		this.kinematicHookMass = null;
		if (this.lineDisconnectionMass != null)
		{
			this.lineDisconnectionMass.IsKinematic = false;
			base.Feeder.SetHookMass(0, this.lineDisconnectionMass);
			base.Feeder.SetHookMass(1, this.lineDisconnectionMass);
		}
		GameFactory.Player.MovedDown = 0f;
		base.RodSlot.Sim.RemoveConnection(this.kvp);
		base.RodSlot.Sim.RefreshObjectArrays(true);
	}

	private const float lineStretch = 0.5f;

	private const float fishSpringConstantModifier = 0.2f;

	private const float flightMidpointYRatio = 0.35f;

	private float timeSpent;

	private Mass kinematicHookMass;

	private bool rotationFinalised;

	private Mass fishHeadMass;

	private Mass fishTailMass;

	private Mass[] fishCollarMasses;

	private ConnectionBase[] fishCollarConnections;

	private Mass lineDisconnectionMass;

	private AbstractFishBody fishBody;

	private VerticalParabola flightCurve;

	private float duration;

	private KinematicVerticalParabola kvp;

	private Spring lineHeadMassSpring;
}
