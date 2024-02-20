using System;

public class HookFlying : HookStateBase
{
	protected override Type onUpdate()
	{
		if (base.Hook.transform.position.y <= 0f)
		{
			return typeof(HookFloating);
		}
		return null;
	}
}
