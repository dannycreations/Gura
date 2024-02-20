using System;

public class PlayerRoll : PlayerRollBase
{
	protected override void onEnter()
	{
		base.Player.OnActiveState(base.GetType(), true);
		base.Player.Update3dCharMecanimParameter(TPMMecanimBParameter.IsRollActive, true);
		base.Player.ShowSleeves = false;
		float num = base.Player.RodForce.magnitude * 0.1f;
		float rotationAngle = base.Player.GetRotationAngle();
		float num2 = 20f;
		float num3 = 0f;
		float num4 = 0f;
		this.lastCycle = -1f;
		float num5 = base.calcAnimationWeight(rotationAngle, num, num2);
		if (rotationAngle < 0f)
		{
			num3 = num5;
		}
		if (rotationAngle >= 0f)
		{
			num4 = num5;
		}
		base.Player.Update3dCharMecanimParameter(TPMMecanimFParameter.CenterWeight, Math.Max((10f - num5 * 3.5f) / 10f, 0f));
		base.Player.Update3dCharMecanimParameter(TPMMecanimFParameter.LeftWeight, Math.Min(num3 * 3.5f / 10f, 1f));
		base.Player.Update3dCharMecanimParameter(TPMMecanimFParameter.RightWeight, Math.Min(num4 * 3.5f / 10f, 1f));
		ReelTypes reelType = base.Player.ReelType;
		if (reelType != ReelTypes.Spinning)
		{
			if (reelType == ReelTypes.Baitcasting)
			{
				this.AnimationState = base.Player.PlayBlendedRightRoll("BaitRoll", 1f, 7f, 0.3f, base.Player.ReelingPos);
				base.Player.PlayBlendedRightRollNoReel("BaitRollLeft", 1f, num3, 0.3f, base.Player.ReelingPos);
				base.Player.PlayBlendedRightRollNoReel("BaitRollRight", 1f, num4, 0.3f, base.Player.ReelingPos);
			}
		}
		else
		{
			this.AnimationState = base.Player.PlayBlendedLeftRoll("Roll", 1f, 5f, 0.3f, base.Player.ReelingPos);
			base.Player.PlayBlendedLeftRollNoReel("RollLeft", 1f, num3, 0.3f, base.Player.ReelingPos);
			base.Player.PlayBlendedLeftRollNoReel("RollRight", 1f, num4, 0.3f, base.Player.ReelingPos);
		}
		base.Player.Reel.IsReeling = true;
	}

	protected override Type onUpdate()
	{
		if (ControlsController.ControlsActions.CameraZoom.WasPressed && !ControlsController.ControlsActions.AddReelClipGamePadPart1.IsPressed)
		{
			base.Player.ZoomCamera(!base.Player.IsCameraZoomed);
		}
		float num = base.Player.RodForce.magnitude * 0.1f;
		float rotationAngle = base.Player.GetRotationAngle();
		float num2 = 20f;
		float num3 = 0f;
		float num4 = 0f;
		float num5 = base.calcAnimationWeight(rotationAngle, num, num2);
		float num6 = num5;
		if (num6 > 5f)
		{
			num6 = 5f;
		}
		num6 = 5f - num6;
		if (rotationAngle < 0f)
		{
			num3 = num5;
		}
		if (rotationAngle >= 0f)
		{
			num4 = num5;
		}
		base.Player.Update3dCharMecanimParameter(TPMMecanimFParameter.CenterWeight, Math.Max((10f - num5 * 3.5f) / 10f, 0f));
		base.Player.Update3dCharMecanimParameter(TPMMecanimFParameter.LeftWeight, Math.Min(num3 * 3.5f / 10f, 1f));
		base.Player.Update3dCharMecanimParameter(TPMMecanimFParameter.RightWeight, Math.Min(num4 * 3.5f / 10f, 1f));
		base.Player.SetPullAngel(rotationAngle, num5);
		if (StaticUserData.RodInHand.IsRodDisassembled)
		{
			return typeof(PlayerDrawOut);
		}
		float num7 = base.Player.Reel.UpdateLineLengthOnReeling();
		float num8 = 1f;
		if (!ControlsController.ControlsActions.Fire1.IsPressed)
		{
			if (num7 >= 2f)
			{
				num8 = 0.7f;
			}
			if (num7 >= 3f)
			{
				num8 = 0.5f;
			}
			if (num7 >= 4f)
			{
				num8 = 0.4f;
			}
		}
		ReelTypes reelType = base.Player.ReelType;
		if (reelType != ReelTypes.Spinning)
		{
			if (reelType == ReelTypes.Baitcasting)
			{
				base.Player.SetAnimationWeight("BaitRollLeft4Blend", num3);
				base.Player.SetAnimationWeight("BaitRollRight4Blend", num4);
				base.Player.SetAnimationWeight("BaitRollLeft", num3 * 10f);
				base.Player.SetAnimationWeight("BaitRollRight", num4 * 10f);
				float num9 = num7 * 0.5f * num8;
				base.Player.Update3dCharMecanimParameter(TPMMecanimFParameter.RollSpeed, num9);
				base.Player.SetAnimationSpeed("BaitRoll", num9);
				base.Player.SetAnimationSpeed("BaitRollLeft", num9);
				base.Player.SetAnimationSpeed("BaitRollRight", num9);
			}
		}
		else
		{
			base.Player.SetAnimationWeight("RollLeft4Blend", num3);
			base.Player.SetAnimationWeight("RollRight4Blend", num4);
			base.Player.SetAnimationWeight("RollLeft", num3 * 10f);
			base.Player.SetAnimationWeight("RollRight", num4 * 10f);
			float num9 = num7 * num8;
			base.Player.Update3dCharMecanimParameter(TPMMecanimFParameter.RollSpeed, num9);
			base.Player.SetAnimationSpeed("Roll", num9);
			base.Player.SetAnimationSpeed("RollLeft", num9);
			base.Player.SetAnimationSpeed("RollRight", num9);
		}
		if (base.Player.Tackle.IsShowing)
		{
			if (base.Player.Tackle.Fish != null)
			{
				if (base.Player.Tackle.Fish.IsBig)
				{
					return typeof(PlayerShowFishIn);
				}
				return typeof(PlayerShowFishLineIn);
			}
			else if (base.Player.Tackle.UnderwaterItem != null)
			{
				return typeof(PlayerShowFishLineIn);
			}
		}
		if (base.Player.Tackle.IsOnTip)
		{
			return typeof(PlayerIdleThrownToIdle);
		}
		if (!ControlsController.ControlsActions.Fire1.IsPressed)
		{
			base.Player.ReelingPos = this.AnimationState.time;
			ReelTypes reelType2 = base.Player.ReelType;
			if (reelType2 != ReelTypes.Spinning)
			{
				if (reelType2 == ReelTypes.Baitcasting)
				{
					base.Player.StopAnimation("BaitRollLeft");
					base.Player.StopAnimation("BaitRollRight");
					base.Player.StopAnimation("BaitRoll");
				}
			}
			else
			{
				base.Player.StopAnimation("RollLeft");
				base.Player.StopAnimation("RollRight");
				base.Player.StopAnimation("Roll");
			}
			return typeof(PlayerIdleThrown);
		}
		return null;
	}

	protected override void onExit()
	{
		base.Player.OnActiveState(base.GetType(), false);
		if (base.Player.RodSlot.SimThread != null)
		{
			base.Player.RodSlot.SimThread.StopLine();
		}
		base.Player.Update3dCharMecanimParameter(TPMMecanimBParameter.IsRollActive, false);
		base.Player.Reel.IsReeling = false;
		base.Player.Reel.UpdateFrictionState(0f);
		base.Player.Reel.SetNormalSpeedMode();
	}

	private const float SIDE_WEIGHT_K = 3.5f;

	private const float MAX_WEIGHT = 10f;
}
