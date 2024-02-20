using System;
using Assets.Scripts.Phy.Simulations;
using UnityEngine;

public class LureOnTip : LureStateBase
{
	protected override void onEnter()
	{
		this.timeSpent = 0f;
		this.initialLineLength = base.RodSlot.Sim.CurrentLineLength;
		this.needResetLineLength = base.RodSlot.Sim.CurrentLineLength != base.RodSlot.Line.MinLineLength;
		base.Lure.IsOnTipComplete = false;
		base.Line1st.ResetLineWidthChange(0.003f);
		base.RodSlot.Reel.IsIndicatorOn = false;
		base.RodSlot.Sim.TurnLimitsOff();
		GameFactory.FishSpawner.DestroyFishCam();
		base.RodSlot.Sim.TackleMoveCompensationMode = TackleMoveCompensationMode.TackleCasted;
		base.Lure.OnEnterHangingState();
		this._owner.Adapter.FinishGameAction();
	}

	protected override Type onUpdate()
	{
		if (!GameFactory.Player.IsRodActive)
		{
			return typeof(LureHidden);
		}
		if (base.player.IsPitching)
		{
			return typeof(LureIdlePitch);
		}
		if (base.Lure.ThrowData.IsThrowing)
		{
			return typeof(LureFlying);
		}
		base.Lure.CheckSurfaceCollisions();
		if (this.timeSpent < 1f)
		{
			this.timeSpent = Mathf.Clamp01(this.timeSpent + Time.deltaTime);
			if (this.initialLineLength > base.RodSlot.Line.MinLineLength)
			{
				base.RodSlot.Sim.FinalLineLength = Mathf.Lerp(this.initialLineLength, base.RodSlot.Line.MinLineLength, this.timeSpent);
			}
		}
		else
		{
			base.Lure.IsOnTipComplete = true;
		}
		if (this.needResetLineLength && base.RodSlot.Sim.CurrentLineLength == base.RodSlot.Line.MinLineLength)
		{
			base.Line1st.ResetToMinLength();
			this.needResetLineLength = false;
		}
		base.RodSlot.Line.TransitToNewLineWidth();
		return null;
	}

	protected override void onExit()
	{
		base.RodSlot.Sim.TackleMoveCompensationMode = TackleMoveCompensationMode.None;
		base.Lure.OnExitHangingState();
	}

	private float timeSpent;

	private float initialLineLength;

	private bool needResetLineLength;
}
