using System;

public class HintCastTarget : HintCastSimple
{
	protected override float CalcArrowTarget()
	{
		float targetValue = GameFactory.Player.HudFishingHandler.CastTargetHandler.TargetValue;
		float maxValue = GameFactory.Player.HudFishingHandler.CastTargetHandler.MaxValue;
		return targetValue / maxValue;
	}
}
