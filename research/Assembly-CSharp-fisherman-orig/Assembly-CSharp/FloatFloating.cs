using System;
using Assets.Scripts.Phy.Simulations;
using ObjectModel;
using Phy;
using UnityEngine;

public class FloatFloating : FloatStateBase
{
	private Mass Motor
	{
		get
		{
			return base.Float.GetHookMass(0);
		}
	}

	private void PrepareNewMovement()
	{
		this._nextMovementAt = Time.time + Random.Range(1.5f, 3f);
		float num = Random.Range(0f, 6.2831855f);
		this._wasAttackFishDetected = base.Float.AttackingFish != null;
		this._liveBaitLerpStartedAt = Time.time;
		float num2 = ((!this._wasAttackFishDetected) ? 0.015f : 0.05f);
		this._targetLiveForce = new Vector3(num2 * Mathf.Cos(num), 0f, num2 * Mathf.Sin(num));
		if (base.Float.BaitItem != null && base.Float.BaitItem.LiveBaitSpeedModifier != null)
		{
			this._targetLiveForce *= base.Float.BaitItem.LiveBaitSpeedModifier.Value;
		}
	}

	protected override void onEnter()
	{
		this.PrepareNextHookPositionCheck();
		if (base.Float.IsLiveBait)
		{
			base.Float.GetHookMass(0).WaterMotor = Vector3.zero;
			this.PrepareNewMovement();
		}
		this.timeSpent = 0f;
		if (base.IsInHands)
		{
			base.Float.LeaderLength = base.Float.UserSetLeaderLength;
			GameFactory.FishSpawner.ShowBobberIndicator(base.Float.transform);
			base.RodSlot.Sim.TurnLimitsOn(false);
			base.RodSlot.Sim.TackleMoveCompensationMode = TackleMoveCompensationMode.TackleCasted;
		}
		this.stateStartTimestamp = Time.time;
	}

	protected override Type onUpdate()
	{
		if (Time.time > this.stateStartTimestamp + 2f)
		{
			this.CheckFishEscape();
		}
		if (base.AssembledRod.IsRodDisassembled)
		{
			return typeof(FloatBroken);
		}
		if (base.Float.HasHitTheGround || base.Float.IsOutOfTerrain || base.Float.IsPitchTooShort)
		{
			this._owner.Adapter.FinishGameAction();
			if (base.IsInHands && base.player.IsPitching)
			{
				return typeof(FloatIdlePitch);
			}
			return typeof(FloatOnTip);
		}
		else
		{
			if (base.Float.IsHitched)
			{
				return typeof(FloatHitched);
			}
			if ((base.Float.Fish != null && (base.Float.Fish.State == typeof(FishSwimAway) || base.Float.Fish.State == typeof(FishHooked))) || (base.Float.AttackingFish != null && base.Float.AttackingFish.State == typeof(FishPredatorAttack)))
			{
				base.Float.Fish = base.Float.Fish ?? base.Float.AttackingFish;
				return typeof(FloatSwallowed);
			}
			if (base.IsInHands && !base.RodSlot.Rod.IsOngoingRodPodTransition)
			{
				if (base.player.IsPitching && (base.RodSlot.Line.FullLineLength <= base.RodSlot.Line.MinLineLengthWithFish || base.RodSlot.Line.SecuredLineLength <= base.RodSlot.Line.MinLineLength))
				{
					if (base.Float.HasUnderwaterItem)
					{
						return typeof(FloatShowItem);
					}
					if (!base.Float.UnderwaterItemIsLoading)
					{
						this._owner.Adapter.FinishGameAction();
						return typeof(FloatIdlePitch);
					}
				}
				if (!base.player.IsPitching && base.RodSlot.Line.SecuredLineLength <= base.RodSlot.Line.MinLineLength)
				{
					if (base.Float.HasUnderwaterItem)
					{
						return typeof(FloatShowItem);
					}
					if (!base.Float.UnderwaterItemIsLoading)
					{
						this._owner.Adapter.FinishGameAction();
						return typeof(FloatOnTip);
					}
				}
			}
			if (!base.Float.UnderwaterItemIsLoading && (base.RodSlot.Rod.CurrentTipPosition.y > base.RodSlot.Line.SecuredLineLength || base.RodSlot.Rod.TackleTipMass.GroundHeight > 0f) && base.Float.IsPulledOut)
			{
				base.Float.TackleOut(1f);
				return typeof(FloatHanging);
			}
			base.Float.CheckSurfaceCollisions();
			this._owner.Adapter.Move(base.Float.HookIsIdle);
			this.DetectEarlyStriking();
			if (base.IsInHands && this.timeSpent < 2f)
			{
				this.timeSpent += Time.deltaTime;
				if (this.timeSpent > 2f)
				{
					base.RodSlot.Reel.IsIndicatorOn = true;
				}
			}
			if (base.Float.IsLiveBait)
			{
				if (this._wasAttackFishDetected != (base.Float.AttackingFish != null) || this._nextMovementAt < Time.time)
				{
					if (this._wasAttackFishDetected != (base.Float.AttackingFish != null))
					{
					}
					this.PrepareNewMovement();
				}
				this.Motor.WaterMotor = Vector3.Slerp(this.Motor.WaterMotor, (!base.RodSlot.Tackle.IsBaitShown) ? Vector3.zero : this._targetLiveForce, Mathf.Clamp01(Time.time - this._liveBaitLerpStartedAt) / 0.75f);
			}
			return null;
		}
	}

	private void DetectEarlyStriking()
	{
		if (base.Float.Fish == null || base.Float.IsAttackFinished || base.Float.IsFinishAttackRequested || (base.IsInHands && GameFactory.Player.State == typeof(TakeRodFromPodOut)))
		{
			return;
		}
		if (base.IsInHands && GameFactory.Player.IsStriking && base.RodSlot.Line.IsTensioned)
		{
			if (base.Float.Fish.State == typeof(FishBite))
			{
				base.Reel1st.BlockLineLengthChange(0.5f);
				if (base.Float.Fish.IsTasting)
				{
					this._owner.Adapter.FinishAttack(true, false, false, true, base.Float.Fish.DistanceToTackle);
					return;
				}
				this._owner.Adapter.FinishAttack(false, true, false, false, base.Float.Fish.DistanceToTackle);
				GameFactory.Message.ShowEarlyStrike(RodHelper.FindRodInSlot(base.RodSlot.Index, null));
				base.Float.Fish.Behavior = FishBehavior.Go;
				return;
			}
			else if (base.Float.Fish.State == typeof(FishPredatorSwim))
			{
				this._owner.Adapter.FinishAttack(false, true, false, false, base.Float.Fish.DistanceToTackle);
				base.Float.Fish.Behavior = FishBehavior.Go;
				return;
			}
		}
	}

	protected void CheckFishEscape()
	{
		IFishController fishController = base.Float.AttackingFish ?? base.Float.Fish;
		float num = 0f;
		if (this._nextPositionCheckAt < Time.time)
		{
			if (fishController != null)
			{
				Vector3? lastHookPosition = this._lastHookPosition;
				if (lastHookPosition != null)
				{
					num = (this._lastHookPosition.Value - base.Float.Rod.TackleTipMass.Position).magnitude;
				}
			}
			this.PrepareNextHookPositionCheck();
		}
		if (fishController == null)
		{
			return;
		}
		bool flag = num / 0.5f > 1f;
		bool flag2 = (base.Float.Rod.TackleTipMass.Position - fishController.MouthPosition).magnitude > 3f;
		if (flag2)
		{
			this._owner.Adapter.FinishAttack(false, true, false, false, 0f);
			GameFactory.Message.ShowTackleWasPulledAwayFromFish(RodHelper.FindRodInSlot(base.RodSlot.Index, null));
			fishController.Behavior = FishBehavior.Go;
		}
	}

	private void PrepareNextHookPositionCheck()
	{
		this._nextPositionCheckAt = Time.time + 0.5f;
		this._lastHookPosition = new Vector3?(base.Float.Rod.TackleTipMass.Position);
	}

	protected override void onExit()
	{
		this.Motor.WaterMotor = Vector3.zero;
		if (base.IsInHands)
		{
			GameFactory.FishSpawner.HideBobberIndicator();
		}
	}

	private const float FishDistanceToHookToEscape = 3f;

	private const float HookSpeedToFishEscape = 1f;

	private const float HookSpeedCheckDelay = 0.5f;

	private Vector3? _lastHookPosition;

	private float _nextPositionCheckAt;

	private const float liveBaitForce = 0.015f;

	private const float liveBaitScaredForce = 0.05f;

	private const float liveBaitLerpTime = 0.75f;

	private const float liveBaitMovementMinDuration = 1.5f;

	private const float liveBaitMovementMaxDuration = 3f;

	private const float IndicatorOnTimeout = 2f;

	private float timeSpent;

	private float _nextMovementAt;

	private float _liveBaitLerpStartedAt;

	private Vector3 _targetLiveForce;

	private bool _wasAttackFishDetected;

	private float stateStartTimestamp;

	private const float skipCheckFishEscape = 2f;

	private float _stopDebugForceAt = -1f;

	private float _applyForceTill = -1f;

	private float _releaseForceFrom = -1f;
}
