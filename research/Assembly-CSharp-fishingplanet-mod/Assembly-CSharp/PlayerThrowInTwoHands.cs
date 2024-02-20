using System;
using UnityEngine;

public class PlayerThrowInTwoHands : PlayerStateBase
{
	protected override void onEnter()
	{
		base.PlayOpenReelSound();
		ShowHudElements.Instance.SetCanvasAlpha(1f);
		if (base.Player.CastType == CastTypes.Simple)
		{
			GameFactory.Player.HudFishingHandler.State = HudState.CastSimple;
			GameFactory.Player.HudFishingHandler.CastSimpleHandler.MaxValue = base.Player.Rod.MaxCastLength;
			if (base.Player.RodSlot != null && base.Player.RodSlot.LineClips != null && base.Player.RodSlot.LineClips.Count > 0)
			{
				base.Player.HudFishingHandler.CastSimpleHandler.SetClipLength(base.Player.Line.MaxLineLength);
			}
		}
		ReelTypes reelType = base.Player.ReelType;
		if (reelType != ReelTypes.Spinning)
		{
			if (reelType == ReelTypes.Baitcasting)
			{
				base.Player.Update3dCharMecanimParameter(TPMMecanimIParameter.ThrowType, 2);
				base.Player.Update3dCharMecanimParameter(TPMMecanimBParameter.ASimpleThrow, true);
				this.AnimationState = base.Player.PlayAnimation("BaitThrow2HandsIn", 1f, 1f, 0f);
			}
		}
		else
		{
			base.Player.Update3dCharMecanimParameter(TPMMecanimIParameter.ThrowType, 2);
			base.Player.Update3dCharMecanimParameter(TPMMecanimBParameter.ASimpleThrow, true);
			this.AnimationState = base.Player.PlayAnimation("throwIn", 1f, 1f, 0f);
		}
		base.Player.DisableMovement();
		base.Player.BeginThrowing();
		base.Player.Invoke("RaiseCanThrow", this.AnimationState.length);
	}

	protected override Type onUpdate()
	{
		if (base.Player.CastType == CastTypes.Simple)
		{
			if (this.canThrow != base.Player.CanThrow && base.Player.CanThrow)
			{
				this.canThrow = true;
				base.Player.BeginThrowPowerGainProcess();
			}
			float num;
			if (this.isForwardProgress)
			{
				num = base.Player.GetThrowPowerGainProgress(4f, 12f);
				if ((double)Math.Abs(num - 1f) < 0.001)
				{
					this.isForwardProgress = false;
					base.Player.BeginThrowTime = Time.time;
				}
			}
			else
			{
				num = base.Player.GetThrowPowerGainProgressBack(12f, 4f);
				if ((double)Math.Abs(num) < 0.001)
				{
					this.isForwardProgress = true;
					base.Player.BeginThrowTime = Time.time;
				}
			}
			if (!ControlsController.ControlsActions.Fire1.IsPressed && this.canThrow)
			{
				base.Player.Tackle.ThrowData.CastLength = Mathf.Max((float)base.Player.Rod.AssembledRod.Rod.Length.Value, base.Player.HudFishingHandler.CastSimpleHandler.CurrentValue);
				base.Player.Tackle.ThrowData.AccuracyRatio = 0f;
				base.Player.Tackle.ThrowData.ThrowForce = num;
				return typeof(PlayerThrowOutTwoHands);
			}
			GameFactory.Player.HudFishingHandler.CastSimpleHandler.CurrentValue = num * base.Player.Rod.MaxCastLength;
		}
		if (base.Player.CastType == CastTypes.Target)
		{
			if (this.canThrow != base.Player.CanThrow && base.Player.CanThrow)
			{
				this.canThrow = true;
			}
			if (this.canThrow)
			{
				if (!StaticUserData.RodInHand.IsRodDisassembled)
				{
					return typeof(PlayerThrowOutTwoHands);
				}
				return typeof(PlayerIdle);
			}
		}
		return null;
	}

	protected override void onExit()
	{
		base.Player.Update3dCharMecanimParameter(TPMMecanimBParameter.ASimpleThrow, false);
	}

	private bool canThrow;

	private bool isForwardProgress = true;
}
