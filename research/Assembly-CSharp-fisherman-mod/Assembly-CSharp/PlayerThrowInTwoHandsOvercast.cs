using System;

public class PlayerThrowInTwoHandsOvercast : PlayerStateBase
{
	protected override void onEnter()
	{
		ShowHudElements.Instance.SetCanvasAlpha(1f);
		if (base.Player.CastType == CastTypes.Simple)
		{
			GameFactory.Player.HudFishingHandler.State = HudState.CastSimple;
			GameFactory.Player.HudFishingHandler.CastSimpleHandler.MaxValue = base.Player.Rod.MaxCastLength;
			if (base.Player.RodSlot != null && base.Player.RodSlot.LineClips != null && base.Player.RodSlot.LineClips.Count > 0)
			{
				base.Player.HudFishingHandler.CastSimpleHandler.SetClipLength(base.Player.Line.MaxLineLength);
			}
		}
	}

	protected override Type onUpdate()
	{
		return (!base.Player.CanThrowOvercast) ? null : typeof(PlayerThrowInTwoHands);
	}
}
