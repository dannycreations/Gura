using System;

public class LureHidden : LureStateBase
{
	protected override void onEnter()
	{
		if (base.player.State != typeof(TakeRodFromPodOut))
		{
			this._owner.Adapter.FinishGameAction();
			base.Lure.EscapeFish();
			base.Lure.SetActive(false);
			if (base.RodSlot.Line != null)
			{
				base.RodSlot.Line.SetActive(false);
			}
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
			return typeof(LureFloating);
		}
		if (base.player.IsPitching)
		{
			return typeof(LureIdlePitch);
		}
		return typeof(LureOnTip);
	}

	protected override void onExit()
	{
		if (base.Lure.gameObject != null)
		{
			base.Lure.gameObject.SetActive(true);
		}
	}
}
