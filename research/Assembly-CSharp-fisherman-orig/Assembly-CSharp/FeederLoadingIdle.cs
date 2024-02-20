using System;
using System.Linq;
using ObjectModel;
using UnityEngine;

public class FeederLoadingIdle : PlayerStateBase
{
	protected override void onEnter()
	{
		base.Player.ShowSleeves = false;
		this._waitTill = Time.time + 2.5f;
		this.chum = base.Player.BeginFeederLoading();
		ShowHudElements.Instance.LureInfoHandler.ChumLoading(true, PhotonConnectionFactory.Instance.Profile.Inventory.GetChumSplitTimeInMilliseconds(this.chum.First<Chum>()) * 0.001f);
		this.AnimationState = base.Player.GetAnimation(base.GetDrawInAnimationName(), 1f, 0f);
	}

	protected override Type onUpdate()
	{
		if (this.AnimationState != null)
		{
			this.AnimationState.time = 0f;
		}
		if (ControlsController.ControlsActions.CancelRename.WasReleasedMandatory || base.Player.FastUseRodPod)
		{
			return typeof(FeederSkipLoading);
		}
		DateTime? dateTime = this.chum.Max((Chum c) => c.BeginSplitTime);
		float chumSplitTimeInMilliseconds = PhotonConnectionFactory.Instance.Profile.Inventory.GetChumSplitTimeInMilliseconds(this.chum.First<Chum>());
		if (this._waitTill >= Time.time && (dateTime == null || DateTime.UtcNow.Subtract(dateTime.Value).TotalMilliseconds < (double)(chumSplitTimeInMilliseconds + 500f)))
		{
			return null;
		}
		if (base.Player.FinishFeederLoading() == null)
		{
			return typeof(FeederSkipLoading);
		}
		return typeof(FeederLoadingOut);
	}

	protected override void onExit()
	{
		this.chum = null;
		ShowHudElements.Instance.LureInfoHandler.ChumLoading(false, 0f);
	}

	public const float RELOADING_TIME = 2.5f;

	private float _waitTill = -1f;

	private Chum[] chum;
}
