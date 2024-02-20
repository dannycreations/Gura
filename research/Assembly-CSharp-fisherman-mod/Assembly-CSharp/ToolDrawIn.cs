using System;

public class ToolDrawIn : PlayerStateBase
{
	protected override void OnCantOpenMenu(MainMenuPage page)
	{
		PlayerStateBase._savedPage = page;
	}

	protected override void onEnter()
	{
		base.onEnter();
		this.AnimationState = base.Player.PlayAnimation("drawBox", 1f, 1f, 0f);
		base.Player.OnToolDrawIn();
	}

	protected override Type onUpdate()
	{
		if (base.IsAnimationFinished)
		{
			return typeof(ToolIdle);
		}
		return null;
	}
}
