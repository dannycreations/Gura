using System;
using Assets.Scripts.Phy.Simulations;
using UnityEngine;

public class FeederOnTip : FeederStateBase
{
	protected override void onEnter()
	{
		this.timeSpent = 0f;
		this.needResetLineLength = true;
		base.Feeder.IsOnTipComplete = false;
		base.ResetLeaderLengthChange();
		this.initialLineLength = base.RodSlot.Sim.CurrentLineLength;
		base.Line1st.ResetLineWidthChange(0.003f);
		base.RodSlot.Reel.IsIndicatorOn = false;
		base.RodSlot.Sim.TurnLimitsOff();
		GameFactory.FishSpawner.DestroyFishCam();
		base.RodSlot.Sim.TackleMoveCompensationMode = TackleMoveCompensationMode.TackleCasted;
		this._owner.Adapter.FinishGameAction();
	}

	protected override Type onUpdate()
	{
		if (!GameFactory.Player.IsRodActive)
		{
			return typeof(FeederHidden);
		}
		if (base.player.IsPitching)
		{
			return typeof(FeederIdlePitch);
		}
		if (base.Feeder.IsThrowing)
		{
			return typeof(FeederFlying);
		}
		base.Feeder.CheckSurfaceCollisions();
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
			base.Feeder.IsOnTipComplete = true;
		}
		if (this.needResetLineLength && base.RodSlot.Sim.CurrentLineLength == base.RodSlot.Line.MinLineLength)
		{
			base.Line1st.ResetToMinLength();
			this.needResetLineLength = false;
		}
		base.RodSlot.Line.TransitToNewLineWidth();
		base.UpdateLeaderLength();
		return null;
	}

	protected override void onExit()
	{
		base.RodSlot.Sim.TackleMoveCompensationMode = TackleMoveCompensationMode.None;
	}

	private float timeSpent;

	private float initialLineLength;

	private bool needResetLineLength;
}
