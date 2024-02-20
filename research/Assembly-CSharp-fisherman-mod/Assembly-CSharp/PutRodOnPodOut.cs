using System;
using UnityEngine;

public class PutRodOnPodOut : RodPodInteractionState
{
	protected override void OnCantOpenMenu(MainMenuPage page)
	{
		PlayerStateBase._savedPage = page;
	}

	protected override void onEnter()
	{
		Transform target = base.Player.CurHand.IK.solver.target;
		base.Player.RodObject.transform.rotation = target.rotation;
		if (base.Player.ReelType == ReelTypes.Baitcasting)
		{
			base.Player.RodObject.transform.rotation *= Quaternion.AngleAxis(180f, Vector3.forward);
		}
		base.StartRodInteraction(0f, target, base.Player.Rod);
		this.puttingRod = base.Player.Rod;
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
			if (Mathf.Approximately(this._smothPrc, 1f))
			{
				base.Player.RodObject.transform.localPosition = Vector3.zero;
				base.Player.PutRodOnPod();
				base.Player.OnDrawOut();
				this._isAnimationStarted = true;
				base.PlayRodInteractionAnimation("StandUpWithoutRod", RodPodInteractionState._downPlayerPos, base.Player.SavedPosition.Value);
			}
			else
			{
				base.Player.RodObject.transform.localPosition = Vector3.Lerp(this._movementFrom, this._movementTo, this._smothPrc);
			}
		}
		else
		{
			base.Player.CurHand.IK.solver.IKPositionWeight = 1f - this._smothPrc;
			if (!base.Player.IsSailing)
			{
				base.Player.Collider.position = Vector3.Lerp(this._movementFrom, this._movementTo, this._smothPrc);
			}
			if (base.IsAnimationFinished)
			{
				base.Player.CurHand.IK.enabled = false;
				base.Player.FreezeCamera(false);
				if (base.Player.IsBoatFishing)
				{
					base.Player.IsPutTrollingRod = true;
				}
				return typeof(PlayerEmpty);
			}
		}
		return null;
	}

	protected override void onExit()
	{
		base.Player.IsEmptyHandsMode = true;
		base.Player.UpdateThrownFlag(false);
		base.Player.Update3dCharMecanimParameter(TPMMecanimBParameter.IsInGame, false);
		base.Player.Update3dCharMecanimParameter(TPMMecanimIParameter.MovieAction, 0);
		base.FinishDownMovement();
		this.puttingRod.OnRodPodTransitionFinish();
	}

	protected bool _isAnimationStarted;

	protected Rod1stBehaviour puttingRod;
}
