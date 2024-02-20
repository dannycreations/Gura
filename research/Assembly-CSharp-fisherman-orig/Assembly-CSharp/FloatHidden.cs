using System;

public class FloatHidden : FloatStateBase
{
	protected override void onEnter()
	{
		if (base.player.State != typeof(TakeRodFromPodOut) && base.player.State != typeof(TakeRodFromPodIn))
		{
			this._owner.Adapter.FinishGameAction();
			base.Float.EscapeFish();
			base.Float.SetActive(false);
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
			return typeof(FloatFloating);
		}
		if (base.player.IsPitching)
		{
			return typeof(FloatIdlePitch);
		}
		return typeof(FloatOnTip);
	}

	protected override void onExit()
	{
		if (base.Float.gameObject != null)
		{
			base.Float.gameObject.SetActive(true);
		}
	}
}
