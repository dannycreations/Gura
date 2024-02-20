using System;
using Assets.Scripts.Phy.Simulations;
using Phy;
using UnityEngine;

public class FloatIdlePitch : FloatStateBase
{
	protected override void onEnter()
	{
		this.timeSpent = 0f;
		this.isInMoveToHandPhase = false;
		base.Float.IsOnTipComplete = false;
		this.need2MakeLineLonger = Mathf.Abs(base.RodSlot.Line.SecuredLineLength - base.RodSlot.Line.MinLineLength) < 0.01f;
		base.ResetLeaderLengthChange();
		this.initialLineLength = base.RodSlot.Line.SecuredLineLength;
		base.RodSlot.Reel.IsIndicatorOn = false;
		base.Line1st.ResetLineWidthChange(0.00075f);
		GameFactory.FishSpawner.DestroyFishCam();
		base.RodSlot.Sim.TurnLimitsOn(false);
		base.RodSlot.Sim.TackleMoveCompensationMode = TackleMoveCompensationMode.PitchIdle;
		this.bobberAirDragBackup = base.Float.GetBobberMainMass.AirDragConstant;
		base.Float.GetBobberMainMass.AirDragConstant = 0.01f;
		this._owner.Adapter.FinishGameAction();
	}

	protected override Type onUpdate()
	{
		FishingRodSimulation sim = base.RodSlot.Sim;
		if (!GameFactory.Player.IsRodActive)
		{
			return typeof(FloatHidden);
		}
		if (!base.player.IsPitching)
		{
			return typeof(FloatOnTip);
		}
		if (base.Float.IsThrowing)
		{
			return typeof(FloatPitching);
		}
		Vector3 position = GameFactory.Player.LinePositionInLeftHand.position;
		this.timeSpent += Time.deltaTime;
		base.RodSlot.Line.TransitToNewLineWidth();
		base.Float.CheckSurfaceCollisions();
		float num = (base.RodSlot.Rod.CurrentUnbendTipPosition - position).magnitude - base.Float.Size - base.Float.LeaderLength;
		if (this.need2MakeLineLonger && !this.isInMoveToHandPhase)
		{
			if (this.timeSpent < 1.25f)
			{
				sim.FinalLineLength = Mathf.Lerp(this.initialLineLength, num, this.timeSpent / 1.25f);
			}
			else
			{
				sim.FinalLineLength = num;
			}
		}
		base.UpdateLeaderLength();
		if (GameFactory.Player.State == typeof(PlayerIdlePitch))
		{
			if (!this.isInMoveToHandPhase)
			{
				this.timeSpent = Time.deltaTime;
				this.isInMoveToHandPhase = true;
			}
			if (this.kinematicLineMass == null)
			{
				this.kinematicLineMass = base.Float.GetHookMass(0);
				this.kinematicLineMass.StopMass();
				this.startPoint = this.kinematicLineMass.Position;
				this.kinematicLineMass.IsKinematic = true;
				if (this.startPoint.y > position.y)
				{
					this.flightCurve = new VerticalParabola(this.startPoint, position, position.y);
				}
				else
				{
					this.flightCurve = new VerticalParabola(this.startPoint, position, this.startPoint.y);
				}
				Debug.DrawLine(this.startPoint, position, Color.red, 10f);
				this.holdLine = new KinematicSpline(this.kinematicLineMass, false);
				base.RodSlot.Sim.Connections.Add(this.holdLine);
				base.RodSlot.Sim.RefreshObjectArrays(true);
				base.RodSlot.Sim.rodObject.HandKinematicSpline = this.holdLine;
			}
			float num2 = this.timeSpent / 1.25f;
			if (this.timeSpent < 1.25f)
			{
				Vector3 point = this.flightCurve.GetPoint(num2);
				Debug.DrawLine(point, this.kinematicLineMass.Position, Color.green, 2f);
				this.holdLine.SetNextPointAndRotation(point, Quaternion.identity);
			}
			else
			{
				this.holdLine.SetNextPointAndRotation(position, Quaternion.identity);
				this.kinematicLineMass.IsKinematic = true;
				base.Float.IsOnTipComplete = true;
			}
			int num3 = sim.Connections.IndexOf(this.kinematicLineMass.NextSpring);
			float num4 = (base.RodSlot.Rod.CurrentUnbendTipPosition - position).magnitude - base.Float.Size - base.Float.LeaderLength - 0.05f;
			sim.FinalLineLength = num4;
			return null;
		}
		if (GameFactory.Player.State == typeof(PlayerDrawOut))
		{
			if (this.kinematicLineMass != null)
			{
				base.Line1st.ReleaseKinematicMass(this.kinematicLineMass);
			}
			sim.FinalLineLength = base.RodSlot.Line.MinLineLength;
		}
		return null;
	}

	protected override void onExit()
	{
		base.Float.GetBobberMainMass.AirDragConstant = this.bobberAirDragBackup;
		if (this.kinematicLineMass != null)
		{
			base.Line1st.ReleaseKinematicMass(this.kinematicLineMass);
		}
		this.kinematicLineMass = null;
		base.RodSlot.Sim.TackleMoveCompensationMode = TackleMoveCompensationMode.None;
		if (this.holdLine != null)
		{
			base.RodSlot.Sim.RemoveConnection(this.holdLine);
		}
		base.RodSlot.Sim.rodObject.HandKinematicSpline = null;
		base.RodSlot.Sim.RefreshObjectArrays(true);
	}

	private const float catchDuration = 1.25f;

	private const float BobberAirDrag = 0.01f;

	private float timeSpent;

	private Mass kinematicLineMass;

	private bool need2MakeLineLonger;

	private float initialLineLength;

	private bool isInMoveToHandPhase;

	private Vector3 startPoint;

	private VerticalParabola flightCurve;

	private KinematicSpline holdLine;

	private float bobberAirDragBackup;
}
