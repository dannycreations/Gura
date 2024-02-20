using System;

public class HookHidden : HookStateBase
{
	protected override void onEnter()
	{
		if (base.Hook.gameObject != null)
		{
			base.Hook.gameObject.SetActive(false);
		}
	}

	protected override Type onUpdate()
	{
		if (GameFactory.Player.IsRodActive)
		{
			return typeof(HookHanging);
		}
		return null;
	}

	protected override void onExit()
	{
		if (base.Hook.gameObject != null)
		{
			base.Hook.gameObject.SetActive(true);
		}
	}
}
