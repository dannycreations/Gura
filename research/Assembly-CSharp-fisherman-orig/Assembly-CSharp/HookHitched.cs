using System;

public class HookHitched : HookStateBase
{
	protected override Type onUpdate()
	{
		if (!base.Hook.Tackle.IsHitched)
		{
			return typeof(HookFloating);
		}
		if (!GameFactory.Player.IsRodActive)
		{
			return typeof(HookHidden);
		}
		return null;
	}
}
