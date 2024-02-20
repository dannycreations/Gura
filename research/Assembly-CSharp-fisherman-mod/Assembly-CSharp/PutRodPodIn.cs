using System;
using UnityEngine;

public class PutRodPodIn : RodPodInteractionState
{
	protected override void OnCantOpenMenu(MainMenuPage page)
	{
		PlayerStateBase._savedPage = page;
	}

	protected override void onEnter()
	{
		base.Player.FreezeCamera(true);
		base.PrepareDownMovement(base.Player.Collider.position.y - 0.7f);
		base.FromToMovement(0.5f, base.Player.Collider.position, RodPodInteractionState._downPlayerPos);
	}

	protected override Type onUpdate()
	{
		base.CalcSmothPrc();
		base.Player.Collider.position = Vector3.Lerp(this._movementFrom, this._movementTo, this._smothPrc);
		if (this._actionTill < Time.time)
		{
			base.Player.PutRodPod();
			return typeof(PutRodPodOut);
		}
		return null;
	}
}
