using System;
using UnityEngine;

public class ReplaceRodOnPodOut : RodPodInteractionState
{
	protected override void onEnter()
	{
		base.Player.Update3dCharMecanimParameter(TPMMecanimIParameter.MovieAction, 0);
		base.StartRodInteraction(0f, base.Player.CurHand.IK.solver.target, base.Player.Rod);
		this.puttingRod = base.Player.Rod;
		base.Player.TakingRod.Behaviour.Tackle.Adapter.FinishAttackOnPod(true, false, false, true, 0f);
	}

	protected override Type OnLateUpdate()
	{
		this.puttingRod.RodSlot.Sim.AutoTension();
		base.CalcSmothPrc();
		if (!this._isAnimationStarted)
		{
			if (!base.Player.IsSailing)
			{
				base.Player.Collider.position = RodPodInteractionState._downPlayerPos;
			}
			base.Player.CurHand.IK.solver.IKPositionWeight = 1f - this._smothPrc;
			base.Player.RodObject.transform.localPosition = Vector3.Lerp(this._movementFrom, this._movementTo, this._smothPrc);
			if (Mathf.Approximately(this._smothPrc, 1f))
			{
				if (base.Player.CurrentBoat != null)
				{
					base.Player.CurrentBoat.DampenRodForce();
				}
				this._isAnimationStarted = true;
				base.Player.CurHand.IK.enabled = false;
				base.Player.RodObject.transform.localPosition = Vector3.zero;
				base.Player.PutRodOnPod();
				base.Player.MakeTakingRodRealRequest();
				Object.Destroy(base.Player.CurHand.IK.solver.target.gameObject);
				if (base.Player.WasLastRodCasting)
				{
					base.Player.SetRodRootRight();
					if (base.Player.TakingRod.ReelType == ReelTypes.Baitcasting)
					{
						this.AnimationState = base.Player.PlayAnimation("RPSStandUpWithCastingSwap", 3f, 1f, 0.5f);
						base.Player.Invoke("SetRodRootLeft", 0.35f);
					}
					else
					{
						this.AnimationState = base.Player.PlayAnimation("RPSStandUpWithSpin", 3f, 1f, 0.5f);
					}
				}
				else
				{
					base.Player.SetRodRootLeft();
					if (base.Player.TakingRod.ReelType == ReelTypes.Baitcasting)
					{
						this.AnimationState = base.Player.PlayAnimation("RPSStandUpWithCasting", 3f, 1f, 0.5f);
					}
					else
					{
						this.AnimationState = base.Player.PlayAnimation("RPSStandUpWithSpinSwap", 3f, 1f, 0.5f);
						base.Player.Invoke("SetRodRootRight", 0.17f);
					}
				}
				base.Player.ReelType = base.Player.TakingRod.ReelType;
				base.FromToMovement(this.AnimationState.length / 3f, RodPodInteractionState._downPlayerPos, base.Player.SavedPosition.Value);
			}
		}
		else
		{
			if (!base.Player.IsSailing)
			{
				base.Player.Collider.position = Vector3.Lerp(this._movementFrom, this._movementTo, this._smothPrc);
			}
			this.UpdateTakenRod();
			if (base.IsAnimationFinished && !GameFactory.IsRodAssembling && !base.Player.TakingRod.Behaviour.RodSlot.PendingServerOp)
			{
				if (UIHelper.IsWaiting)
				{
					UIHelper.Waiting(false, null);
				}
				base.Player.OnDrawIn();
				base.Player.FreezeCamera(false);
				base.Player.Rod.FreezeRodTipMoveSpeedUpdate(0.5f);
				if (base.Player.TakingRod.ReelType == ReelTypes.Baitcasting)
				{
					base.Player.SmoothSetRodRootLeft();
				}
				else
				{
					base.Player.SmoothSetRodRootRight();
				}
				base.Player.UpdateReelIkController();
				return typeof(PlayerIdleThrown);
			}
			if (base.IsAnimationFinished)
			{
				if (this._animationFinishedTimeStamp == 0f)
				{
					this._animationFinishedTimeStamp = Time.time;
				}
				else if (Time.time > this._animationFinishedTimeStamp + 2f && !UIHelper.IsWaiting)
				{
					UIHelper.Waiting(true, null);
				}
			}
		}
		return null;
	}

	private void UpdateTakenRod()
	{
		Transform transform = base.Player.TakingRod.transform;
		transform.parent = base.Player.CurHand.RodBone;
		transform.rotation = base.Player.TakenRodRotation;
		transform.localPosition = Vector3.zero;
	}

	protected override void onExit()
	{
		base.Player.TakingRod.Behaviour.OnRodPodTransitionFinish();
		this.puttingRod.OnRodPodTransitionFinish();
		base.Player.OnTakingRodReadyToBeInstantiated();
		base.FinishDownMovement();
	}

	private const float FreezeRodTipMoveSpeedUpdateDuration = 0.5f;

	private bool _isAnimationStarted;

	private Rod1stBehaviour puttingRod;
}
