using System;
using ObjectModel;
using Phy;
using Phy.Verlet;
using UnityEngine;

public class LureShowBigFish : LureStateBase
{
	protected override void onEnter()
	{
		this._owner.Adapter.CatchFish();
		base.Lure.IsShowingComplete = false;
		base.RodSlot.Reel.IsIndicatorOn = false;
		base.RodSlot.Sim.TurnLimitsOn(true);
		this.timeSpent = 0f;
		GameFactory.Player.InitTransitionToZeroXRotation();
		base.Line1st.ResetLineWidthChange(0.00075f);
		base.Lure.Fish.Show();
		base.Lure.SetFreeze(true);
		this.fishBody = base.Lure.Fish.FishObject as AbstractFishBody;
		this.fishHeadMass = this.fishBody.Masses[0];
		this.fishTailMass = this.fishBody.Masses[16];
		this.fishBody.StopMasses();
		this.fishToHookConnection = this.fishHeadMass.PriorSpring;
		float num = this.fishHeadMass.Position.y + (base.GripTransform.position.y - this.fishHeadMass.Position.y) * 0.35f;
		Debug.DrawLine(this.fishHeadMass.Position, base.GripTransform.position, Color.magenta, 10f);
		this.flightCurve = new VerticalParabola(this.fishHeadMass.Position, base.GripTransform.position, num);
		this.fishHeadMass.IsKinematic = true;
		base.Lure.KinematicHandLineMass = base.RodSlot.Sim.RodTipMass;
		this.duration = TackleBehaviour.ShowGripInAnimDuration(base.Lure.Fish);
		this.kvp = new KinematicVerticalParabola(this.fishHeadMass, this.fishTailMass, this.duration, 2f, this.flightCurve)
		{
			DestinationPoint = base.GripTransform.position
		};
		base.RodSlot.Sim.Connections.Add(this.kvp);
		base.RodSlot.Sim.RefreshObjectArrays(true);
		this.leaderLength = 0f;
		Lure1stBehaviour lure1stBehaviour = base.RodSlot.Tackle as Lure1stBehaviour;
		if (lure1stBehaviour != null && lure1stBehaviour.RodAssembly.LeaderInterface != null && lure1stBehaviour.RodAssembly.LeaderInterface is Leader)
		{
			this.leaderLength = (lure1stBehaviour.RodAssembly.LeaderInterface as Leader).LeaderLength;
		}
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
					base.Lure.SetFreeze(false);
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
						this.fishCollarConnections[i] = new VerletSpring(this.fishCollarMasses[i] as VerletMass, this.fishBody.Masses[i + 1] as VerletMass, 0.025f, 0.005f, true);
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
					this.fishHeadMass.PriorSpring.Mass2 = this.lineDisconnectionMass;
					this.lineDisconnectionMass.PriorSpring = this.fishHeadMass.PriorSpring;
					this.fishHeadMass.PriorSpring = null;
					base.RodSlot.Sim.Masses.Add(this.lineDisconnectionMass);
					this.fishBody.StopMasses();
					base.Lure.SetHookMass(this.lineDisconnectionMass);
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
		base.Lure.IsShowing = false;
		this._owner.Adapter.FinishGameAction();
		if (base.player.IsPitching)
		{
			return typeof(LureIdlePitch);
		}
		return typeof(LureOnTip);
	}

	private void UpdateFishPool()
	{
		float progress = this.kvp.Progress;
		GameFactory.Player.MovedDown = Mathf.Lerp(0f, 1f, this.timeSpent / 1.5f);
		base.RodSlot.Sim.FinalLineLength = Mathf.Max(base.RodSlot.Rod.CurrentUnbendTipPosition.y, base.RodSlot.Line.MinLineLength) * 0.5f;
		Vector3 vector = this.fishBody.Masses[0].Position - base.Lure.HookAnchor.position;
		base.Lure.transform.position += vector;
		Vector3 directionVector = base.Lure.DirectionVector;
		Vector3 segmentRight = this.fishBody.GetSegmentRight(0);
		base.Lure.transform.rotation = Quaternion.LookRotation(directionVector, segmentRight);
		this.kvp.DestinationPoint = base.HandTransform.position;
		this.kvp.ConnectionNeedSyncMark();
	}

	private void UpdateTacklePosition()
	{
		Vector3 vector = Quaternion.AngleAxis(-30f + base.Lure.Fish.Owner.GripRotation, Vector3.up) * base.GripTransform.right;
		if (this.lineDisconnectionMass != null)
		{
			this.fishHeadMass.Position = base.GripTransform.position;
			this.fishHeadMass.Rotation = Quaternion.LookRotation(vector, Vector3.up);
		}
		if (this.fishCollarMasses != null)
		{
			Vector3[] idealTetrahedronPositions = this.fishBody.GetIdealTetrahedronPositions(base.GripTransform.position - this.fishBody.TetrahedronHeight * Vector3.up, Vector3.Cross(Vector3.up, vector), vector, Vector3.up);
			for (int i = 0; i < this.fishCollarMasses.Length; i++)
			{
				this.fishCollarMasses[i].Position = idealTetrahedronPositions[i + 1];
			}
		}
		float y = base.HandTransform.rotation.eulerAngles.y;
		Vector3 normalized = (this.fishBody.Masses[0].Position - this.fishBody.Masses[4].Position).normalized;
		Vector3 segmentRight = this.fishBody.GetSegmentRight(0);
		base.Lure.transform.rotation = Quaternion.LookRotation(normalized, segmentRight);
		Vector3 vector2 = this.fishBody.Masses[0].Position - base.Lure.HookAnchor.position;
		base.Lure.transform.position += vector2;
		if (this.lineDisconnectionMass.IsKinematic)
		{
			Vector3 vector3 = base.RodSlot.Sim.rodObject.TipMass.Position + (base.RodSlot.Sim.rodObject.RootMass.Position - base.RodSlot.Sim.rodObject.TipMass.Position).normalized * (base.RodSlot.Sim.FinalLineLength + base.Lure.Size + this.leaderLength);
			this.lineDisconnectionMass.Position = Vector3.Lerp(vector3, this.lineDisconnectionMass.Position, Mathf.Exp(-Time.deltaTime * 100f));
		}
		Vector3 vector4 = base.RodSlot.Rod.CurrentUnbendTipPosition - base.Lure.TopLineAnchor.position;
		base.RodSlot.Sim.FinalLineLength = base.RodSlot.Line.MinLineLength;
		GameFactory.Player.MovedDown = Mathf.Lerp(1f, 0f, (this.timeSpent - this.duration) / 1f);
		if (this.timeSpent > this.duration + 1f)
		{
			base.Lure.IsShowingComplete = true;
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
		if (base.Lure.Fish != null)
		{
			this.fishBody.IsKinematic = false;
			this.fishBody.StopMasses();
		}
		base.Lure.SetFreeze(false);
		this.kinematicLureMass = null;
		if (this.lineDisconnectionMass != null)
		{
			this.lineDisconnectionMass.IsKinematic = false;
			base.Lure.SetLureMasses(this.lineDisconnectionMass);
		}
		GameFactory.Player.MovedDown = 0f;
		base.RodSlot.Sim.RemoveConnection(this.kvp);
		base.RodSlot.Sim.RefreshObjectArrays(true);
	}

	private const float lineStretch = 0.5f;

	private const float fishSpringConstantModifier = 0.2f;

	private const float flightMidpointYRatio = 0.35f;

	private float timeSpent;

	private Mass kinematicLureMass;

	private bool rotationFinalised;

	private Mass fishHeadMass;

	private Mass fishTailMass;

	private ConnectionBase fishToHookConnection;

	private Mass[] fishCollarMasses;

	private ConnectionBase[] fishCollarConnections;

	private AbstractFishBody fishBody;

	private Mass lineDisconnectionMass;

	private VerticalParabola flightCurve;

	private float duration;

	private KinematicVerticalParabola kvp;

	private float xRotationOffset = -90f;

	private float yRotationOffset = 90f;

	private float leaderLength;
}
