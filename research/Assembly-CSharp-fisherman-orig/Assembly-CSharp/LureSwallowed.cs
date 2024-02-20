using System;
using ObjectModel;
using UnityEngine;

public class LureSwallowed : LureStateBase
{
	protected override void onEnter()
	{
		if (base.IsInHands)
		{
			base.RodSlot.Sim.TurnLimitsOn(true);
			base.RodSlot.Reel.IsIndicatorOn = true;
			if (base.Lure.Fish != null)
			{
				base.RodSlot.Sim.HasFishAttackForceImpulse = true;
				base.RodSlot.Sim.FishAttackImpulseForce = base.Lure.Fish.CurrentForce * 0.1f;
			}
		}
		if (!base.Lure.IsAttackFinished && !base.Lure.IsFinishAttackRequested)
		{
			this.strikeTimeEnd = Time.time + 3f;
			this.wasPulledDuringAttack = false;
		}
	}

	protected override Type onUpdate()
	{
		if (base.AssembledRod.IsRodDisassembled)
		{
			return typeof(LureBroken);
		}
		if (!base.Lure.FishIsLoading && (base.Lure.Fish == null || base.Lure.Fish.Behavior == FishBehavior.Go))
		{
			if (this.timeoutExpired && base.Lure.Fish != null && base.Lure.Fish.AttackLure != 1f)
			{
				GameFactory.Message.ShowStrikeTimeoutExpired(RodHelper.FindRodInSlot(base.RodSlot.Index, null));
			}
			return typeof(LureFloating);
		}
		if (base.Lure.IsHitched)
		{
			base.Lure.EscapeFish();
			return typeof(LureHitched);
		}
		if (base.IsInHands && base.RodSlot.Reel.IsReeling && base.RodSlot.Line.SecuredLineLength <= base.RodSlot.Line.MinLineLengthWithFish)
		{
			if (base.Lure.Fish.Behavior != FishBehavior.Hook)
			{
				this._owner.Adapter.FinishAttack(false, true, false, false, 0f);
				base.Lure.EscapeFish();
				this._owner.Adapter.FinishGameAction();
				GameFactory.Message.ShowIncorrectReeling(RodHelper.FindRodInSlot(base.RodSlot.Index, null));
				if (base.player.IsPitching)
				{
					return typeof(LureIdlePitch);
				}
				return typeof(LureOnTip);
			}
			else
			{
				base.Lure.IsShowing = true;
				if (base.Lure.Fish.IsBig)
				{
					return typeof(LureShowBigFish);
				}
				return typeof(LureShowSmallFish);
			}
		}
		else
		{
			if (base.Lure.Rod.CurrentTipPosition.y > base.RodSlot.Line.SecuredLineLength)
			{
				return typeof(LureHanging);
			}
			this.DetectStriking();
			if (!base.Lure.FishIsLoading && base.Lure.IsAttackFinished && base.Lure.Fish.Behavior == FishBehavior.Hook)
			{
				if (base.IsInHands)
				{
					this._owner.Adapter.FightFish();
				}
				else
				{
					this._owner.Adapter.FightFishOnPod();
				}
				this.timeoutExpired = false;
			}
			if (base.IsInHands && !base.RodSlot.Reel.IsReeling)
			{
				base.Reel1st.IncreaseLineLength();
			}
			base.Lure.UpdateLureDepthStatus();
			return null;
		}
	}

	protected override void onExit()
	{
		if (base.IsInHands)
		{
			base.RodSlot.Reel.UpdateFrictionState(0f);
		}
	}

	protected void DetectStriking()
	{
		if (base.Lure.IsFinishAttackRequested || base.Lure.IsAttackFinished || (base.IsInHands && GameFactory.Player.State == typeof(TakeRodFromPodOut)))
		{
			return;
		}
		if (base.Lure.IsBeingPulled)
		{
			this.wasPulledDuringAttack = true;
		}
		if (base.IsInHands && GameFactory.Player.IsStriking && base.RodSlot.Line.IsTensioned)
		{
			this._owner.Adapter.FinishAttack(true, false, this.wasPulledDuringAttack, true, 0f);
			return;
		}
		if (Time.time > this.strikeTimeEnd)
		{
			this._owner.Adapter.FinishAttack(false, false, this.wasPulledDuringAttack, true, 0f);
			this.timeoutExpired = true;
		}
	}

	private const float FishBiteImpulseStrenth = 0.1f;

	private float strikeTimeEnd;

	private bool timeoutExpired;

	private bool wasPulledDuringAttack;

	public const float StrikeTimeout = 3f;
}
