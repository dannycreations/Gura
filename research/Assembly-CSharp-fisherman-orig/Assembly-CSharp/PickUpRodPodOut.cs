using System;
using UnityEngine;

public class PickUpRodPodOut : RodPodInteractionState
{
	protected override void OnCantOpenMenu(MainMenuPage page)
	{
		PlayerStateBase._savedPage = page;
	}

	protected override void onEnter()
	{
		base.FromToMovement(0.5f, RodPodInteractionState._downPlayerPos, base.Player.SavedPosition.Value);
	}

	protected override Type onUpdate()
	{
		base.CalcSmothPrc();
		base.Player.Collider.position = Vector3.Lerp(this._movementFrom, this._movementTo, this._smothPrc);
		if (this._actionTill < Time.time)
		{
			base.Player.FreezeCamera(false);
			return typeof(RodPodIdle);
		}
		return null;
	}

	protected override void onExit()
	{
		base.FinishDownMovement();
	}
}
