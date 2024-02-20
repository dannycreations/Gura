using System;
using ObjectModel;
using UnityEngine;

public class HandThrowIn : PlayerStateBase
{
	protected override void onEnter()
	{
		base.onEnter();
		base.Player.Update3dCharMecanimParameter(TPMMecanimBParameter.ASimpleThrow, true);
		this.AnimationState = base.Player.PlayAnimation("LureThrowIn", 1f, 1f, 0f);
		ShowHudElements.Instance.SetCanvasAlpha(1f);
		base.Player.HudFishingHandler.State = HudState.HandCastingSimple;
		base.Player.HudFishingHandler.CastSimpleHandler.MaxValue = Inventory.ChumHandMaxCastLength;
		base.Player.HudFishingHandler.CastSimpleHandler.SetClipLength(Inventory.ChumHandMaxCastLength);
		base.Player.BeginThrowing();
		base.Player.Invoke("RaiseCanThrow", this.AnimationState.length);
	}

	protected override Type onUpdate()
	{
		if (base.Player.CanThrow)
		{
			if (!this._isAnimationFinished)
			{
				this._isAnimationFinished = true;
				base.Player.BeginThrowPowerGainProcess();
			}
			float num;
			if (this.isForwardProgress)
			{
				num = base.Player.GetThrowPowerGainProgress(4f, 12f);
				if (Mathf.Approximately(num, 1f))
				{
					this.isForwardProgress = false;
					base.Player.BeginThrowTime = Time.time;
				}
			}
			else
			{
				num = base.Player.GetThrowPowerGainProgressBack(12f, 4f);
				if (Mathf.Approximately(num, 0f))
				{
					this.isForwardProgress = true;
					base.Player.BeginThrowTime = Time.time;
				}
			}
			if (!ControlsController.ControlsActions.Fire1.IsPressed)
			{
				GameFactory.Player.ThrowData.CastLength = Mathf.Lerp(5f, Inventory.ChumHandMaxCastLength, num);
				GameFactory.Player.ThrowData.AccuracyRatio = 0f;
				GameFactory.Player.ThrowData.ThrowForce = num;
				return typeof(HandThrowOut);
			}
			base.Player.HudFishingHandler.CastSimpleHandler.CurrentValue = num * Inventory.ChumHandMaxCastLength;
		}
		return null;
	}

	protected override void onExit()
	{
		base.Player.FinishThrowing();
		base.Player.EnableMovement();
	}

	private const float MIN_DIST = 5f;

	private bool isForwardProgress = true;

	private bool _isAnimationFinished;
}
