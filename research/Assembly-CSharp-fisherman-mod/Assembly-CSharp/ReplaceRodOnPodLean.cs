using System;
using UnityEngine;

public class ReplaceRodOnPodLean : RodPodInteractionState
{
	protected override void onEnter()
	{
		base.Player.FreezeCamera(true);
		base.Player.Rod.FreezeRodTipMoveSpeedUpdate(2f);
		base.Player.Rod.OnRodPodTransitionStart();
		base.Player.AdjustPositionWithPod();
	}

	protected override Type OnLateUpdate()
	{
		base.Player.Rod.RodSlot.Sim.AutoTension();
		if (!this._isAnimationStarted)
		{
			if (base.Player.IsPositionAdjusted)
			{
				this._isAnimationStarted = true;
				base.Player.ClearRodPodsHighlighting();
				base.Player.PrepareHandForTakingRod(base.Player.TakingHand.IK);
				base.PrepareDownMovement(base.Player.RodPodInteractionHeight);
				base.Player.Update3dCharMecanimParameter(TPMMecanimIParameter.MovieAction, 12);
				base.Player.Update3dCharMecanimParameter(TPMMecanimIParameter.ThrowType, 10);
				base.PlayRodInteractionAnimation((base.Player.ReelType != ReelTypes.Baitcasting) ? "RPSLeanWithSpin" : "RPSLeanWithCasting", base.Player.Collider.position, RodPodInteractionState._downPlayerPos);
			}
		}
		else
		{
			base.CalcSmothPrc();
			if (!base.Player.IsSailing)
			{
				base.Player.Collider.position = Vector3.Lerp(this._movementFrom, this._movementTo, this._smothPrc);
			}
			base.Player.TakingHand.IK.solver.IKPositionWeight = this._smothPrc;
			if (base.IsAnimationFinished)
			{
				return typeof(ReplaceRodOnPodTakeAndPut);
			}
		}
		return null;
	}

	private const float FreezeRodTipMoveSpeedUpdateDuration = 2f;

	private bool _isAnimationStarted;
}
