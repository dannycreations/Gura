using System;

public abstract class HookStateBase : FsmBaseState<HookController>
{
	protected HookController Hook
	{
		get
		{
			return this._owner;
		}
	}
}
