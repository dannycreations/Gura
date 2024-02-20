using System;
using Assets.Scripts.Phy.Simulations;
using ObjectModel;
using Phy;
using UnityEngine;

public class FeederFloating : FeederStateBase
{
	private Mass Motor
	{
		get
		{
			return base.Feeder.GetHookMass(0);
		}
	}

	private void PrepareNewMovement()
	{
		this._nextMovementAt = Time.time + Random.Range(1.5f, 3f);
		float num = Random.Range(0f, 6.2831855f);
		this._wasAttackFishDetected = base.Feeder.AttackingFish != null;
		this._liveBaitLerpStartedAt = Time.time;
		float num2 = ((!this._wasAttackFishDetected) ? 0.015f : 0.05f);
		this._targetLiveForce = new Vector3(num2 * Mathf.Cos(num), -num2 * Mathf.Clamp((this.Motor.Position.y + 0.3f) / 0.1f, -1f, 1f), num2 * Mathf.Sin(num));
		if (base.Feeder.BaitItem != null && base.Feeder.BaitItem.LiveBaitSpeedModifier != null)
		{
			this._targetLiveForce *= base.Feeder.BaitItem.LiveBaitSpeedModifier.Value;
		}
	}

	protected override void onEnter()
	{
		this.PrepareNextHookPositionCheck();
		if (base.Feeder.IsLiveBait)
		{
			base.Feeder.GetHookMass(0).WaterMotor = Vector3.zero;
			this.PrepareNewMovement();
		}
		this.timeSpent = 0f;
		base.Feeder.InitFeeding();
		base.Feeder.UpdateFeeding(base.Feeder.transform.position);
		if (base.IsInHands)
		{
			base.Feeder.LeaderLength = base.Feeder.UserSetLeaderLength;
			base.RodSlot.Sim.TurnLimitsOn(false);
			base.RodSlot.Sim.TackleMoveCompensationMode = TackleMoveCompensationMode.TackleCasted;
			if (base.Feeder.RigidBody.GroundHeight < 0f && base.Feeder.Rod.TackleTipMass.GroundHeight < 0f)
			{
				if (base.player.Rod.IsQuiver && GameFactory.QuiverIndicator != null && SettingsManager.FishingIndicator)
				{
					GameFactory.QuiverIndicator.HighSensitivity(false);
					GameFactory.QuiverIndicator.Show();
				}
				if (GameFactory.BottomIndicator != null && SettingsManager.FishingIndicator && base.AssembledRod.RodTemplate != RodTemplate.Spod)
				{
					GameFactory.BottomIndicator.Show();
				}
			}
			base.Feeder.FeederObject.OnEnterWater();
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
			return typeof(FeederBroken);
		}
		if (base.Feeder.HasHitTheGround || base.Feeder.IsOutOfTerrain || base.Feeder.IsPitchTooShort)
		{
			this._owner.Adapter.FinishGameAction();
			if (base.IsInHands && base.player.IsPitching)
			{
				return typeof(FeederIdlePitch);
			}
			return typeof(FeederOnTip);
		}
		else
		{
			if (base.Feeder.IsFilled)
			{
				base.Feeder.SetFilled(false);
			}
			if (base.Feeder.IsHitched)
			{
				return typeof(FeederHitched);
			}
			if ((base.Feeder.Fish != null && (base.Feeder.Fish.State == typeof(FishSwimAway) || base.Feeder.Fish.State == typeof(FishHooked))) || (base.Feeder.AttackingFish != null && base.Feeder.AttackingFish.State == typeof(FishPredatorAttack)))
			{
				base.Feeder.Fish = base.Feeder.Fish ?? base.Feeder.AttackingFish;
				return typeof(FeederSwallowed);
			}
			if (base.IsInHands)
			{
				if (base.player.IsPitching && (base.RodSlot.Line.FullLineLength <= base.RodSlot.Line.MinLineLengthWithFish || base.RodSlot.Line.SecuredLineLength <= base.RodSlot.Line.MinLineLength))
				{
					if (base.Feeder.HasUnderwaterItem)
					{
						return typeof(FeederShowItem);
					}
					if (!base.Feeder.UnderwaterItemIsLoading)
					{
						this._owner.Adapter.FinishGameAction();
						return typeof(FeederIdlePitch);
					}
				}
				if (!base.player.IsPitching && base.RodSlot.Line.SecuredLineLength <= base.RodSlot.Line.MinLineLength)
				{
					if (base.Feeder.HasUnderwaterItem)
					{
						return typeof(FeederShowItem);
					}
					if (!base.Feeder.UnderwaterItemIsLoading)
					{
						this._owner.Adapter.FinishGameAction();
						return typeof(FeederOnTip);
					}
				}
			}
			if (!base.Feeder.UnderwaterItemIsLoading && base.RodSlot.Rod.CurrentTipPosition.y > base.RodSlot.Line.SecuredLineLength && base.Feeder.IsPulledOut)
			{
				base.Feeder.TackleOut(1f);
				return typeof(FeederHanging);
			}
			if (base.IsInHands)
			{
				base.Feeder.UpdateLureDepthStatus();
				base.UpdateBottomIndicator();
				base.UpdateQuiverIndicator();
			}
			base.Feeder.CheckSurfaceCollisions();
			this._owner.Adapter.Move(base.Feeder.HookIsIdle);
			this.DetectEarlyStriking();
			base.Feeder.UpdateFeeding(base.Feeder.RigidBody.Position);
			if (base.IsInHands && this.timeSpent < 2f)
			{
				this.timeSpent += Time.deltaTime;
				if (this.timeSpent > 2f)
				{
					base.RodSlot.Reel.IsIndicatorOn = true;
				}
			}
			if (base.IsInHands && base.RodSlot.Sim.TackleTipMass.Position.y <= 0f)
			{
				base.RodSlot.Sim.LineTipMass.NextSpring.AffectMass2Factor = 1f;
			}
			if (base.Feeder.IsLiveBait)
			{
				if (this._wasAttackFishDetected != (base.Feeder.AttackingFish != null) || this._nextMovementAt < Time.time)
				{
					if (this._wasAttackFishDetected != (base.Feeder.AttackingFish != null))
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
		if (base.Feeder.Fish == null || base.Feeder.IsAttackFinished || base.Feeder.IsFinishAttackRequested || (base.IsInHands && GameFactory.Player.State == typeof(TakeRodFromPodOut)))
		{
			return;
		}
		if (base.IsInHands && GameFactory.Player.IsStriking && base.RodSlot.Line.IsTensioned)
		{
			if (base.Feeder.Fish.State == typeof(FishBite))
			{
				base.Reel1st.BlockLineLengthChange(0.5f);
				if (base.Feeder.Fish.IsTasting)
				{
					this._owner.Adapter.FinishAttack(true, false, false, true, base.Feeder.Fish.DistanceToTackle);
					return;
				}
				this._owner.Adapter.FinishAttack(false, true, false, false, base.Feeder.Fish.DistanceToTackle);
				GameFactory.Message.ShowEarlyStrike(RodHelper.FindRodInSlot(base.RodSlot.Index, null));
				base.Feeder.Fish.Behavior = FishBehavior.Go;
				return;
			}
			else if (base.Feeder.Fish.State == typeof(FishPredatorSwim))
			{
				this._owner.Adapter.FinishAttack(false, true, false, false, base.Feeder.Fish.DistanceToTackle);
				base.Feeder.Fish.Behavior = FishBehavior.Go;
				return;
			}
		}
	}

	protected void CheckFishEscape()
	{
		IFishController fishController = base.Feeder.AttackingFish ?? base.Feeder.Fish;
		float num = 0f;
		if (this._nextPositionCheckAt < Time.time)
		{
			if (fishController != null)
			{
				Vector3? lastHookPosition = this._lastHookPosition;
				if (lastHookPosition != null)
				{
					num = (this._lastHookPosition.Value - base.Feeder.Rod.TackleTipMass.Position).magnitude;
				}
			}
			this.PrepareNextHookPositionCheck();
		}
		if (fishController == null)
		{
			return;
		}
		bool flag = num / 0.5f > 1f;
		bool flag2 = (base.Feeder.Rod.TackleTipMass.Position - fishController.MouthPosition).magnitude > 3f;
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
		this._lastHookPosition = new Vector3?(base.Feeder.Rod.TackleTipMass.Position);
	}

	protected override void onExit()
	{
		this.Motor.WaterMotor = Vector3.zero;
		if (base.NextState != typeof(FeederHitched))
		{
			base.Feeder.DestroyFeeding();
		}
		if (base.IsInHands)
		{
			base.Feeder.FeederObject.OnLeaveWater();
			if (GameFactory.BottomIndicator != null)
			{
				GameFactory.BottomIndicator.Hide();
			}
			if (base.NextState != typeof(FeederSwallowed) && base.player.Rod.IsQuiver && GameFactory.QuiverIndicator != null)
			{
				GameFactory.QuiverIndicator.Hide();
			}
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
