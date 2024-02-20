using System;

public class FeederHidden : FeederStateBase
{
	protected override void onEnter()
	{
		if (base.player.State != typeof(TakeRodFromPodOut) && base.player.State != typeof(TakeRodFromPodIn))
		{
			this._owner.Adapter.FinishGameAction();
			base.Feeder.EscapeFish();
			base.Feeder.SetActive(false);
			base.RodSlot.Line.SetActive(false);
		}
	}

	protected override Type onUpdate()
	{
		if (!GameFactory.Player.IsRodActive)
		{
			return null;
		}
		if (base.player.State == typeof(TakeRodFromPodOut))
		{
			return null;
		}
		if (base.player.State == typeof(PlayerIdleThrown))
		{
			return typeof(FeederFloating);
		}
		if (base.player.IsPitching)
		{
			return typeof(FeederIdlePitch);
		}
		return typeof(FeederOnTip);
	}

	protected override void onExit()
	{
		if (base.Feeder.gameObject != null)
		{
			base.Feeder.gameObject.SetActive(true);
		}
	}
}
