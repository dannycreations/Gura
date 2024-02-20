using System;
using ObjectModel;
using UnityEngine;

public class PlayerIdleTarget : PlayerStateBase
{
	protected override void onEnter()
	{
		base.PlaySound(PlayerStateBase.Sounds.AccurateCast);
		base.Player.HudFishingHandler.State = HudState.CastTarget;
		base.Player.HudFishingHandler.CastTargetHandler.CurrentValue = 0f;
		base.Player.HudFishingHandler.CastTargetHandler.MaxValue = base.Player.Rod.MaxCastLength;
		if (base.Player.RodSlot != null && base.Player.RodSlot.LineClips != null && base.Player.RodSlot.LineClips.Count > 0)
		{
			base.Player.HudFishingHandler.CastTargetHandler.SetClipLength(base.Player.Line.MaxLineLength);
		}
		base.Player.IsFailThrowing = false;
		base.Player.CreateAndActivateTarget();
	}

	protected override Type onUpdate()
	{
		if (ControlsController.ControlsActions.Fire2.WasPressed || ControlsController.ControlsActions.CancelRename.WasClicked)
		{
			base.Player.RodSlot.Tackle.ThrowData.Target = null;
			base.Player.DestroyTarget(true);
			base.PlaySound(PlayerStateBase.Sounds.InaccurateCast);
			base.Player.FinishThrowPowerGainProcess();
			base.Player.FinishThrowPowerGainProcessSwing();
			return typeof(PlayerIdle);
		}
		if (ControlsController.ControlsActions.Fire1.WasPressed && Math.Abs(this.startTime) < 0.01f)
		{
			this.startTime = Time.time;
			base.Player.CastType = CastTypes.Target;
			base.Player.RaiseCanThrow();
			if (this.canThrow != base.Player.CanThrow && base.Player.CanThrow)
			{
				this.canThrow = true;
				base.Player.BeginThrowPowerGainProcess();
				base.Player.FreezeCamera(true);
			}
		}
		if (!ControlsController.ControlsActions.Fire1.WasPressed || Time.time - this.startTime <= 0.1f)
		{
			if (this.canThrow)
			{
				float num;
				if (this.isForwardProgress)
				{
					num = base.Player.GetThrowPowerGainProgress(1f, 4f);
					if ((double)Math.Abs(num - 1f) < 0.001)
					{
						this.isForwardProgress = false;
						base.Player.BeginThrowTime = Time.time;
					}
				}
				else
				{
					num = base.Player.GetThrowPowerGainProgressBack(4f, 1f);
					if ((double)Math.Abs(num) < 0.001)
					{
						this.isForwardProgress = true;
						base.Player.BeginThrowTime = Time.time;
					}
				}
				GameFactory.Player.HudFishingHandler.CastTargetHandler.CurrentValue = num * base.Player.Rod.MaxCastLength;
			}
			return null;
		}
		float num2 = (float)StaticUserData.RodInHand.Rod.Length.Value * 2f;
		float num3;
		CastTargetResult targetResult = GameFactory.Player.HudFishingHandler.CastTargetHandler.GetTargetResult(out num3);
		base.Player.RodSlot.Tackle.ThrowData.ThrowForce = GameFactory.Player.HudFishingHandler.CastTargetHandler.CastMultiplier;
		if (targetResult == CastTargetResult.Overcast)
		{
			base.Player.Tackle.ThrowData.CastLength = num2;
			base.Player.Tackle.ThrowData.IsOvercasting = true;
			base.Player.Tackle.ThrowData.Target = base.Player.ThrowTargetPoint;
			GameActionAdapter.Instance.Throw(base.Player.Tackle.ThrowData.ThrowForce);
			base.Player.FinishThrowPowerGainProcessSwing();
			base.Player.IsFailThrowing = true;
			return typeof(PlayerThrowInTwoHandsOvercast);
		}
		if (targetResult != CastTargetResult.Undercast)
		{
			if (targetResult == CastTargetResult.Hitcast)
			{
				num2 = GameFactory.Player.HudFishingHandler.CastTargetHandler.CurrentValue;
			}
		}
		base.Player.Tackle.ThrowData.Target = base.Player.ThrowTargetPoint;
		base.Player.RaiseCanNotThrow();
		base.Player.Tackle.ThrowData.CastLength = num2;
		base.Player.Tackle.ThrowData.AccuracyRatio = num3;
		if (num2 >= base.Player.Rod.MaxCastLength / 2f || PhotonConnectionFactory.Instance.Profile.Inventory.GetRodTemplate(base.Player.Rod.AssembledRod.Rod) == RodTemplate.Float)
		{
			return typeof(PlayerThrowInTwoHands);
		}
		if (base.Player.Tackle is FeederBehaviour)
		{
			return typeof(PlayerThrowInOneHand);
		}
		float value = Random.value;
		if (value < 0.33f)
		{
			return typeof(PlayerThrowInOneHand);
		}
		if (value >= 0.33f && (double)value < 0.66)
		{
			return typeof(PlayerThrowInOneHandLeft);
		}
		return typeof(PlayerThrowInOneHandRight);
	}

	protected override void onExit()
	{
		base.Player.FreezeCamera(false);
		base.Player.DestroyTarget(false);
		GameFactory.Player.HudFishingHandler.State = HudState.Fight;
	}

	private float startTime;

	private bool canThrow;

	private bool isForwardProgress = true;
}
