using System;
using UnityEngine;

public class TakeRodFromPodIn : RodPodInteractionState
{
	protected override void onEnter()
	{
		base.Player.FreezeCamera(true);
		base.Player.AdjustPositionWithPod();
	}

	protected override Type OnLateUpdate()
	{
		base.Player.TakingRod.Behaviour.RodSlot.Sim.AutoTension();
		if (!this._isAnimationStarted)
		{
			base.Player.UpdateThrownFlag(true);
			if (base.Player.IsPositionAdjusted)
			{
				this._isAnimationStarted = true;
				if (base.Player.CurrentBoat != null)
				{
					base.Player.CurrentBoat.DampenRodForce();
				}
				if (base.Player.TakingRod.ReelType == ReelTypes.Baitcasting)
				{
					base.Player.OnChangeLeftHandRod(true);
				}
				else
				{
					base.Player.OnChangeLeftHandRod(false);
				}
				base.Player.PrepareHandForTakingRod(base.Player.TakingHand.IK);
				base.Player.MakeTakingRodRealRequest();
				base.Player.TakingRod.Behaviour.OnRodPodTransitionStart();
				base.PrepareDownMovement(base.Player.RodPodInteractionHeight);
				base.Player.Update3dCharMecanimParameter(TPMMecanimIParameter.MovieAction, 11);
				base.Player.Update3dCharMecanimParameter(TPMMecanimIParameter.ThrowType, 10);
				base.PlayRodInteractionAnimation((base.Player.TakingRod.ReelType != ReelTypes.Baitcasting) ? "TakeRodFromPod" : "TakeRodFromPodCasting", base.Player.Collider.position, RodPodInteractionState._downPlayerPos);
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
			if (base.IsAnimationFinished && !base.Player.TakingRod.Behaviour.RodSlot.IsRodAssembling)
			{
				return typeof(TakeRodFromPodOut);
			}
		}
		return null;
	}

	private bool _isAnimationStarted;
}
