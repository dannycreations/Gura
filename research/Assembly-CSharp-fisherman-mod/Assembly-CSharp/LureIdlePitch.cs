using System;
using Assets.Scripts.Phy.Simulations;
using Phy;
using UnityEngine;

public class LureIdlePitch : LureStateBase
{
	protected override void onEnter()
	{
		this.timeSpent = 0f;
		this.isInMoveToHandPhase = false;
		base.Lure.IsOnTipComplete = false;
		this.need2MakeLineLonger = Mathf.Abs(base.RodSlot.Line.SecuredLineLength - base.RodSlot.Line.MinLineLength) < 0.01f;
		base.RodSlot.Reel.IsIndicatorOn = false;
		base.Line1st.ResetLineWidthChange(0.00075f);
		this.initialLineLength = base.RodSlot.Line.SecuredLineLength;
		base.RodSlot.Sim.TurnLimitsOn(false);
		GameFactory.FishSpawner.DestroyFishCam();
		base.RodSlot.Sim.TackleMoveCompensationMode = TackleMoveCompensationMode.PitchIdle;
		this._owner.Adapter.FinishGameAction();
	}

	protected override Type onUpdate()
	{
		FishingRodSimulation sim = base.RodSlot.Sim;
		if (!GameFactory.Player.IsRodActive)
		{
			return typeof(LureHidden);
		}
		if (!base.player.IsPitching)
		{
			return typeof(LureOnTip);
		}
		if (base.Lure.ThrowData.IsThrowing)
		{
			return typeof(LurePitching);
		}
		Vector3 position = GameFactory.Player.LinePositionInLeftHand.position;
		this.timeSpent += Time.deltaTime;
		base.RodSlot.Line.TransitToNewLineWidth();
		base.Lure.CheckSurfaceCollisions();
		Vector3 vector = base.RodSlot.Rod.CurrentUnbendTipPosition - position;
		if (this.need2MakeLineLonger && !this.isInMoveToHandPhase)
		{
			if (this.timeSpent < 1f)
			{
				sim.FinalLineLength = Mathf.Lerp(this.initialLineLength, vector.magnitude, this.timeSpent);
			}
			else
			{
				sim.FinalLineLength = vector.magnitude;
			}
		}
		if (GameFactory.Player.State == typeof(PlayerIdlePitch))
		{
			if (!this.isInMoveToHandPhase)
			{
				this.timeSpent = Time.deltaTime;
				this.isInMoveToHandPhase = true;
			}
			if (this.kinematicLineMass == null)
			{
				this.kinematicLineMass = base.Line1st.GetKinematicMass();
			}
			Vector3 vector2 = position - this.kinematicLineMass.Position;
			Vector3 vector3;
			if (this.timeSpent < 1f)
			{
				vector3 = Vector3.Lerp(Vector3.zero, vector2, this.timeSpent);
			}
			else
			{
				vector3 = vector2;
				base.Lure.IsOnTipComplete = true;
			}
			this.kinematicLineMass.Position += vector3;
			base.RodSlot.Sim.CompensateKinematicMassMovement(this.kinematicLineMass, vector3);
			int num = sim.Connections.IndexOf(this.kinematicLineMass.NextSpring);
			sim.ForwardRealignMasses(num);
			sim.FinalLineLength = (base.RodSlot.Rod.CurrentUnbendTipPosition - this.kinematicLineMass.Position).magnitude;
			return null;
		}
		return null;
	}

	protected override void onExit()
	{
		if (this.kinematicLineMass != null)
		{
			base.Line1st.ReleaseKinematicMass(this.kinematicLineMass);
		}
		this.kinematicLineMass = null;
		base.RodSlot.Sim.TackleMoveCompensationMode = TackleMoveCompensationMode.None;
	}

	private float timeSpent;

	private Mass kinematicLineMass;

	private bool need2MakeLineLonger;

	private float initialLineLength;

	private bool isInMoveToHandPhase;
}
