using System;

public class HookFloating : HookStateBase
{
	protected override Type onUpdate()
	{
		if (base.Hook.Tackle.Rod.CurrentTipPosition.y > base.Hook.Tackle.RodSlot.Line.SecuredLineLength && base.Hook.transform.position.y >= 0f)
		{
			base.Hook.Tackle.DisturbWater(base.Hook.transform.position, 0.1f, 10f);
			return typeof(HookHanging);
		}
		if (base.Hook.Tackle.IsShowing)
		{
			return typeof(HookShowing);
		}
		if (!GameFactory.Player.IsRodActive)
		{
			return typeof(HookHidden);
		}
		return null;
	}
}
