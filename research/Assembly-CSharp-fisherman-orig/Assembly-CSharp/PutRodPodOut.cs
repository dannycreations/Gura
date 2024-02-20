using System;
using UnityEngine;

public class PutRodPodOut : RodPodInteractionState
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
		if (this._actionTill >= Time.time)
		{
			return null;
		}
		if (base.Player.IsOneMoreRodPodPresent())
		{
			return typeof(RodPodIn);
		}
		base.Player.IsWithRodPodMode = false;
		return typeof(PlayerEmpty);
	}

	protected override void onExit()
	{
		base.FinishDownMovement();
		base.Player.FreezeCamera(false);
	}
}
