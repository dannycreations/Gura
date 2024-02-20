using System;
using ObjectModel;
using UnityEngine;

public class FloatSwallowed : FloatStateBase
{
	public static float ActiveStrikeTimeout(Fish fish, bool isRodOnStand)
	{
		if (fish.AttackDelay != null)
		{
			return fish.AttackDelay.Value + fish.HoldDelay.Value + fish.EndGameDelay.Value;
		}
		if (isRodOnStand)
		{
			return 35f;
		}
		return 5f;
	}

	private void DetectStriking()
	{
		if (base.Float.Fish == null || base.Float.IsAttackFinished || base.Float.IsFinishAttackRequested || (base.IsInHands && GameFactory.Player.State == typeof(TakeRodFromPodOut)))
		{
			return;
		}
		if (base.IsInHands && GameFactory.Player.IsStriking && base.RodSlot.Line.IsTensioned)
		{
			if (base.Float.Fish.State == typeof(FishSwimAway))
			{
				base.Reel1st.BlockLineLengthChange(0.5f);
				this._owner.Adapter.FinishAttack(true, false, false, true, 0f);
				return;
			}
			if (base.Float.Fish.State == typeof(FishPredatorAttack))
			{
				this._owner.Adapter.FinishAttack(true, false, false, true, 0f);
				return;
			}
		}
		if (Time.time > base.Float.StrikeTimeEnd)
		{
			this._owner.Adapter.FinishAttack(false, false, false, true, 0f);
			base.Float.IsStrikeTimedOut = true;
		}
	}

	protected override void onEnter()
	{
		if (base.IsInHands)
		{
			GameFactory.FishSpawner.ShowBobberIndicator(base.Float.transform);
			base.RodSlot.Sim.TurnLimitsOn(true);
			base.RodSlot.Reel.IsIndicatorOn = true;
		}
		PhotonConnectionFactory.Instance.ChangeSwitch(GameSwitchType.FloatSwallowed, true, new int?(base.RodSlot.Index));
		if (!base.Float.IsAttackFinished && !base.Float.IsFinishAttackRequested && base.Float.FishTemplate != null)
		{
			if (base.Float.StrikeTimeEnd == 0f)
			{
				base.Float.StrikeTimeEnd = Time.time + FloatSwallowed.ActiveStrikeTimeout(base.Float.FishTemplate, !base.IsInHands);
			}
			base.Float.IsStrikeTimedOut = false;
		}
	}

	protected override Type onUpdate()
	{
		if (base.Float.Fish != null && base.Float.Fish.Behavior == FishBehavior.Hook && base.IsInHands)
		{
			GameFactory.FishSpawner.HideBobberIndicator();
		}
		if (base.AssembledRod.IsRodDisassembled)
		{
			return typeof(FloatBroken);
		}
		if (!base.Float.FishIsLoading && (base.Float.Fish == null || base.Float.Fish.Behavior == FishBehavior.Go) && (base.Float.AttackingFish == null || base.Float.AttackingFish.State != typeof(FishPredatorAttack)))
		{
			return typeof(FloatFloating);
		}
		if (base.Float.IsHitched)
		{
			base.Float.EscapeFish();
			return typeof(FloatHitched);
		}
		if (base.IsInHands && base.RodSlot.Reel.IsReeling && (base.RodSlot.Line.FullLineLength <= base.RodSlot.Line.MinLineLengthWithFish || base.RodSlot.Line.SecuredLineLength <= base.RodSlot.Line.MinLineLength))
		{
			if (base.Float.Fish.Behavior != FishBehavior.Hook)
			{
				this._owner.Adapter.FinishAttack(false, true, false, false, 0f);
				base.Float.EscapeFish();
				this._owner.Adapter.FinishGameAction();
				GameFactory.Message.ShowIncorrectReeling(RodHelper.FindRodInSlot(base.RodSlot.Index, null));
				if (base.player.IsPitching)
				{
					return typeof(FloatIdlePitch);
				}
				return typeof(FloatOnTip);
			}
			else
			{
				base.Float.IsShowing = true;
				if (base.Float.Fish.IsBig)
				{
					return typeof(FloatShowBigFish);
				}
				return typeof(FloatShowSmallFish);
			}
		}
		else
		{
			if (base.Float.Rod.CurrentTipPosition.y > base.RodSlot.Line.SecuredLineLength)
			{
				return typeof(FloatHanging);
			}
			this.DetectStriking();
			if (!base.Float.FishIsLoading && base.Float.IsAttackFinished && base.Float.Fish.Behavior == FishBehavior.Hook)
			{
				if (base.IsInHands)
				{
					base.Float.IsShowing = true;
					if (base.Float.Fish.IsBig)
					{
						return typeof(FloatShowBigFish);
					}
					return typeof(FloatShowSmallFish);
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
			return null;
		}
	}

	protected override void onExit()
	{
		if (base.IsInHands)
		{
			GameFactory.FishSpawner.HideBobberIndicator();
			base.RodSlot.Reel.UpdateFrictionState(0f);
		}
		PhotonConnectionFactory.Instance.ChangeSwitch(GameSwitchType.FloatSwallowed, false, new int?(base.RodSlot.Index));
	}

	public const float StrikeTimeout = 5f;

	private const float StrikeTimeoutOnRodStand = 35f;
}
