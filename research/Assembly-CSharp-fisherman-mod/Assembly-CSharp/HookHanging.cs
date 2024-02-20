using System;

public class HookHanging : HookStateBase
{
	protected override Type onUpdate()
	{
		if (base.Hook.Tackle.IsShowing)
		{
			return typeof(HookShowing);
		}
		if (base.Hook.transform.position.y < 0f)
		{
			base.Hook.Tackle.DisturbWater(base.Hook.transform.position, 0.1f, 10f);
			return typeof(HookFloating);
		}
		if (!GameFactory.Player.IsRodActive)
		{
			return typeof(HookHidden);
		}
		return null;
	}
}
