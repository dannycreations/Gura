using System;

public class HookShowing : HookStateBase
{
	protected override Type onUpdate()
	{
		if (!base.Hook.Tackle.IsShowing)
		{
			return typeof(HookHanging);
		}
		return null;
	}
}
