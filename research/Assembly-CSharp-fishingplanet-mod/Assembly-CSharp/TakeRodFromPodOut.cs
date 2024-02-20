using System;
using UnityEngine;

public class TakeRodFromPodOut : RodPodInteractionState
{
	protected override void onEnter()
	{
		GameFactory.Player.HudFishingHandler.State = HudState.Fight;
		PlayerController.HandData takingHand = base.Player.TakingHand;
		Transform target = takingHand.IK.solver.target;
		Vector3 vector = Math3d.ProjectOXZ(target.forward);
		this._fromRotation = target.rotation;
		this._toRotation = Quaternion.FromToRotation(Vector3.forward, vector);
		if (base.Player.TakingRod.ReelType == ReelTypes.Baitcasting)
		{
			this._toRotation *= Quaternion.AngleAxis(180f, Vector3.forward);
		}
		float num = Mathf.Abs(Math3d.ClampAngleTo180(Vector3.Angle(target.forward, vector)));
		float num2 = num / 180f;
		base.StartRodInteraction(num2, takingHand.RodBone, base.Player.TakingRod.Behaviour);
	}

	protected override Type OnLateUpdate()
	{
		base.Player.TakingRod.Behaviour.RodSlot.Sim.AutoTension();
		base.CalcSmothPrc();
		Transform transform = base.Player.TakingRod.transform;
		transform.parent = base.Player.TakingHand.RodBone;
		if (!this._isAnimationStarted)
		{
			if (!base.Player.IsSailing)
			{
				base.Player.Collider.position = RodPodInteractionState._downPlayerPos;
			}
			transform.rotation = Quaternion.Slerp(this._fromRotation, this._toRotation, this._smothPrc);
			transform.localPosition = Vector3.Lerp(this._movementFrom, this._movementTo, this._smothPrc);
			if (Mathf.Approximately(this._smothPrc, 1f))
			{
				this._isAnimationStarted = true;
				transform.localPosition = Vector3.zero;
				this._fromRotation = transform.rotation;
				base.PlayRodInteractionAnimation((base.Player.TakingRod.ReelType != ReelTypes.Baitcasting) ? "StandUpWithRod" : "StandUpWithRodCasting", base.Player.Collider.position, base.Player.SavedPosition.Value);
			}
		}
		else
		{
			if (!base.Player.IsSailing)
			{
				base.Player.Collider.position = Vector3.Lerp(this._movementFrom, this._movementTo, this._smothPrc);
			}
			PlayerController.HandData takingHand = base.Player.TakingHand;
			takingHand.IK.solver.IKPositionWeight = 1f - this._smothPrc;
			transform.rotation = this._fromRotation;
			transform.localPosition = Vector3.zero;
			if (base.IsAnimationFinished && !GameFactory.IsRodAssembling && !base.Player.TakingRod.Behaviour.RodSlot.PendingServerOp)
			{
				if (UIHelper.IsWaiting)
				{
					UIHelper.Waiting(false, null);
				}
				takingHand.IK.enabled = false;
				Object.Destroy(takingHand.IK.solver.target.gameObject);
				base.Player.ReelType = base.Player.TakingRod.ReelType;
				if (base.Player.TakingRod.ReelType == ReelTypes.Baitcasting)
				{
					base.Player.SmoothSetRodRootLeft();
				}
				else
				{
					base.Player.SmoothSetRodRootRight();
				}
				base.Player.OnDrawIn();
				base.Player.FreezeCamera(false);
				base.Player.Rod.FreezeRodTipMoveSpeedUpdate(0.5f);
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

	protected override void onExit()
	{
		base.Player.TakingRod.Behaviour.OnRodPodTransitionFinish();
		base.Player.Update3dCharMecanimParameter(TPMMecanimIParameter.MovieAction, 0);
		base.Player.OnTakingRodReadyToBeInstantiated();
		base.FinishDownMovement();
	}

	private const float FreezeRodTipMoveSpeedUpdateDuration = 0.5f;

	protected Quaternion _fromRotation;

	protected Quaternion _toRotation;

	protected bool _isAnimationStarted;
}
