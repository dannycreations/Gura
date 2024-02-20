using System;
using ObjectModel;
using UnityEngine;

public class FeederSwallowed : FeederStateBase
{
	public static float ActiveStrikeTimeout(Fish fish, bool isBottom, bool isRodOnStand)
	{
		if (fish.AttackDelay != null)
		{
			return fish.AttackDelay.Value + fish.HoldDelay.Value + fish.EndGameDelay.Value;
		}
		if (isRodOnStand)
		{
			return 35f;
		}
		if (isBottom)
		{
			return 20f;
		}
		return 20f;
	}

	private void DetectStriking()
	{
		if (base.Feeder.Fish == null || base.Feeder.IsAttackFinished || base.Feeder.IsFinishAttackRequested || (base.IsInHands && GameFactory.Player.State == typeof(TakeRodFromPodOut)))
		{
			return;
		}
		if (base.IsInHands && (GameFactory.Player.IsStriking || GameFactory.Player.IsReeling) && base.RodSlot.Line.IsTensioned)
		{
			if (base.RodSlot.Bell != null)
			{
				base.RodSlot.Bell.Voice(false);
			}
			bool flag = GameFactory.Player.IsStriking || (GameFactory.Player.IsReeling && Random.value > 0.5f);
			if (base.Feeder.Fish.State == typeof(FishSwimAway))
			{
				base.Reel1st.BlockLineLengthChange(0.5f);
				this._owner.Adapter.FinishAttack(flag, false, false, true, 0f);
				return;
			}
			if (base.Feeder.Fish.State == typeof(FishPredatorAttack))
			{
				this._owner.Adapter.FinishAttack(flag, false, false, true, 0f);
				return;
			}
		}
		if (Time.time > base.Feeder.StrikeTimeEnd)
		{
			this._owner.Adapter.FinishAttack(false, false, false, true, 0f);
			base.Feeder.IsStrikeTimedOut = true;
		}
	}

	protected override void onEnter()
	{
		if (base.IsInHands)
		{
			base.RodSlot.Sim.TurnLimitsOn(true);
			base.RodSlot.Reel.IsIndicatorOn = true;
		}
		PhotonConnectionFactory.Instance.ChangeSwitch(GameSwitchType.FeederSwallowed, true, new int?(base.RodSlot.Index));
		if (!base.Feeder.IsAttackFinished && !base.Feeder.IsFinishAttackRequested && base.Feeder.FishTemplate != null)
		{
			if (base.Feeder.StrikeTimeEnd == 0f)
			{
				base.Feeder.StrikeTimeEnd = Time.time + FeederSwallowed.ActiveStrikeTimeout(base.Feeder.FishTemplate, base.Feeder.IsBottom, !base.IsInHands);
			}
			base.Feeder.IsStrikeTimedOut = false;
		}
	}

	protected override Type onUpdate()
	{
		if (base.AssembledRod.IsRodDisassembled)
		{
			return typeof(FeederBroken);
		}
		if (!base.Feeder.FishIsLoading && (base.Feeder.Fish == null || base.Feeder.Fish.Behavior == FishBehavior.Go) && (base.Feeder.AttackingFish == null || base.Feeder.AttackingFish.State != typeof(FishPredatorAttack)))
		{
			return typeof(FeederFloating);
		}
		if (base.Feeder.IsHitched)
		{
			base.Feeder.EscapeFish();
			return typeof(FeederHitched);
		}
		if (base.IsInHands && base.RodSlot.Reel.IsReeling && (base.RodSlot.Line.FullLineLength <= base.RodSlot.Line.MinLineLengthWithFish || base.RodSlot.Line.SecuredLineLength <= base.RodSlot.Line.MinLineLength))
		{
			if (base.Feeder.Fish.Behavior != FishBehavior.Hook)
			{
				this._owner.Adapter.FinishAttack(false, true, false, false, 0f);
				base.Feeder.EscapeFish();
				this._owner.Adapter.FinishGameAction();
				GameFactory.Message.ShowIncorrectReeling(RodHelper.FindRodInSlot(base.RodSlot.Index, null));
				if (base.player.IsPitching)
				{
					return typeof(FeederIdlePitch);
				}
				return typeof(FeederOnTip);
			}
			else
			{
				base.Feeder.IsShowing = true;
				if (base.Feeder.Fish.IsBig)
				{
					return typeof(FeederShowBigFish);
				}
				return typeof(FeederShowSmallFish);
			}
		}
		else
		{
			if (base.Feeder.Rod.CurrentTipPosition.y > base.RodSlot.Line.SecuredLineLength)
			{
				return typeof(FeederHanging);
			}
			this.DetectStriking();
			if (!base.Feeder.FishIsLoading && base.Feeder.IsAttackFinished && base.Feeder.Fish.Behavior == FishBehavior.Hook)
			{
				if (base.IsInHands)
				{
					this._owner.Adapter.FightFish();
				}
				else
				{
					this._owner.Adapter.FightFishOnPod();
				}
			}
			if (base.IsInHands && !base.RodSlot.Reel.IsReeling)
			{
				base.Reel1st.IncreaseLineLength();
			}
			if (base.IsInHands)
			{
				if (base.Feeder.Fish == null || base.Feeder.Fish.Behavior != FishBehavior.Hook)
				{
					base.UpdateQuiverIndicator();
				}
				if (base.RodSlot.Bell != null && base.RodSlot.Bell.IsVoice && base.RodSlot.Reel.IsReeling && GameFactory.Player.IsStriking)
				{
					base.RodSlot.Bell.Voice(false);
				}
			}
			return null;
		}
	}

	protected override void onExit()
	{
		if (base.IsInHands)
		{
			base.RodSlot.Reel.UpdateFrictionState(0f);
			if (base.NextState != typeof(FeederFloating) && base.player.Rod.IsQuiver && GameFactory.QuiverIndicator != null)
			{
				GameFactory.QuiverIndicator.Hide();
			}
		}
		PhotonConnectionFactory.Instance.ChangeSwitch(GameSwitchType.FeederSwallowed, false, new int?(base.RodSlot.Index));
	}

	private const float StrikeTimeout = 20f;

	private const float StrikeTimeoutForBottom = 20f;

	private const float StrikeTimeoutOnRodStand = 35f;

	private const float StrikeByReelingChance = 0.5f;
}
