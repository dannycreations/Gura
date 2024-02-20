using System;

public class ToolDrawOut : PlayerStateBase
{
	protected override void OnCantOpenMenu(MainMenuPage page)
	{
		PlayerStateBase._savedPage = page;
	}

	protected override void onEnter()
	{
		base.onEnter();
		base.Player.Update3dCharMecanimParameter(TPMMecanimBParameter.IsFireworkMode, false);
		this.AnimationState = base.Player.PlayReverseAnimation("drawBox", 1f);
	}

	protected override Type onUpdate()
	{
		if (base.IsAnimationFinished)
		{
			return (base.Player.RequestedFireworkItem == null) ? typeof(PlayerEmpty) : typeof(ToolDrawIn);
		}
		return null;
	}

	protected override void onExit()
	{
		if (base.Player.CurFirework.IsDestroyingRequested)
		{
			base.Player.DestroyFirework();
		}
	}
}
