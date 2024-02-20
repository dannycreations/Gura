using System;
using UnityEngine;

public class PutRodOnPodIn : RodPodInteractionState
{
	protected override void OnCantOpenMenu(MainMenuPage page)
	{
		PlayerStateBase._savedPage = page;
	}

	protected override void onEnter()
	{
		base.Player.FreezeCamera(true);
		base.Player.AdjustPositionWithPod();
		base.Player.CreatePutTargetForCurrentRod();
		base.Player.MakePuttingRodRealRequest();
		this.puttingRod = base.Player.Rod;
		this.puttingRod.FreezeRodTipMoveSpeedUpdate(this.FreezeRodTipMoveSpeedUpdateDuration);
		this.puttingRod.OnRodPodTransitionStart();
	}

	protected override Type OnLateUpdate()
	{
		this.puttingRod.RodSlot.Sim.AutoTension();
		if (!this._isAnimationStarted)
		{
			if (base.Player.IsPositionAdjusted)
			{
				this._isAnimationStarted = true;
				base.PrepareDownMovement(base.Player.RodPodInteractionHeight);
				base.Player.Update3dCharMecanimParameter(TPMMecanimIParameter.MovieAction, 11);
				base.PlayRodInteractionAnimation((base.Player.ReelType != ReelTypes.Baitcasting) ? "PutRodOnPod" : "PutRodOnPodCasting", base.Player.Collider.position, RodPodInteractionState._downPlayerPos);
				this._rod = base.Player.RodObject.transform;
				this._fromRotation = this._rod.rotation;
				this._toRotation = base.Player.CurHand.IK.solver.target.rotation;
				if (base.Player.ReelType == ReelTypes.Baitcasting)
				{
					this._toRotation *= Quaternion.AngleAxis(180f, Vector3.forward);
				}
			}
		}
		else
		{
			base.CalcSmothPrc();
			if (!base.Player.IsSailing)
			{
				base.Player.Collider.position = Vector3.Lerp(this._movementFrom, this._movementTo, this._smothPrc);
			}
			base.Player.CurHand.IK.solver.IKPositionWeight = this._smothPrc;
			this._rod.rotation = Quaternion.Slerp(this._fromRotation, this._toRotation, this._smothPrc);
			this._rod.localPosition = Vector3.zero;
			if (base.IsAnimationFinished)
			{
				return typeof(PutRodOnPodOut);
			}
		}
		return null;
	}

	private float FreezeRodTipMoveSpeedUpdateDuration = 3f;

	private bool _isAnimationStarted;

	protected Quaternion _fromRotation;

	protected Quaternion _toRotation;

	private Transform _rod;

	protected Rod1stBehaviour puttingRod;
}
