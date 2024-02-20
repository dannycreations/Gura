using System;
using ObjectModel;
using UnityEngine;

public class HandLoadingIdle : PlayerStateBase
{
	protected override void onEnter()
	{
		base.Player.ShowSleeves = false;
		this._waitTill = Time.time + 25f;
		this._chum = base.Player.BeginHandLoading();
		base.Player.HudFishingHandler.State = HudState.HandIdle;
		GameFactory.Player.HudFishingHandler.LineHandler.LineLength = (int)MeasuringSystemManager.LineLength(Inventory.ChumHandMaxCastLength);
		ShowHudElements.Instance.LureInfoHandler.ChumLoading(true, PhotonConnectionFactory.Instance.Profile.Inventory.GetChumSplitTimeInMilliseconds(this._chum) * 0.001f);
		base.Player.PlayAnimation("empty", 1f, 1f, 0f);
	}

	protected override Type onUpdate()
	{
		if (ControlsController.ControlsActions.CancelRename.WasReleasedMandatory || base.Player.FastUseRodPod)
		{
			return typeof(HandSkipLoading);
		}
		DateTime? beginSplitTime = this._chum.BeginSplitTime;
		float chumSplitTimeInMilliseconds = PhotonConnectionFactory.Instance.Profile.Inventory.GetChumSplitTimeInMilliseconds(this._chum);
		if (this._waitTill >= Time.time && (beginSplitTime == null || DateTime.UtcNow.Subtract(beginSplitTime.Value).TotalMilliseconds < (double)(chumSplitTimeInMilliseconds + 500f)))
		{
			return null;
		}
		if (base.Player.FinishHandLoading() == null)
		{
			return typeof(HandSkipLoading);
		}
		return typeof(HandLoadingOut);
	}

	protected override void onExit()
	{
		this._chum = null;
		ShowHudElements.Instance.LureInfoHandler.ChumLoading(false, 0f);
	}

	public const float RELOADING_TIME = 25f;

	private float _waitTill;

	private Chum _chum;
}
