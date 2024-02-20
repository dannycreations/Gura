using System;
using ObjectModel;
using UnityEngine;

public class FishHooked : FishStateBase
{
	public override TPMFishState State
	{
		get
		{
			return TPMFishState.Hooked;
		}
	}

	protected override void onEnter()
	{
		GameFactory.Player.EUpdateTargetState(false);
		base.Fish.Hook(Random.Range(3f, 5f));
		base.Fish.Tackle.Fish = base.Fish;
		this.timeSpentInTechHook = 0f;
		if (base.IsInHands)
		{
			if (base.RodSlot.Bell != null)
			{
				base.RodSlot.Bell.Voice(false);
			}
			if (base.RodSlot.Rod.IsQuiver && GameFactory.QuiverIndicator != null)
			{
				GameFactory.QuiverIndicator.Hide();
			}
		}
		else if (base.RodSlot.Bell != null)
		{
			base.RodSlot.Bell.Voice(true);
		}
	}

	protected override Type onUpdate()
	{
		if (base.Fish.Behavior == FishBehavior.Go)
		{
			return typeof(FishEscape);
		}
		if (base.Fish.Tackle == null)
		{
			return typeof(FishEscape);
		}
		if (base.Fish.Behavior == FishBehavior.Undefind)
		{
			this.timeSpentInTechHook += Time.deltaTime;
			if (this.timeSpentInTechHook > 5f)
			{
				base.Fish.Behavior = FishBehavior.Go;
				return typeof(FishEscape);
			}
		}
		if (base.Fish.Tackle.IsShowing)
		{
			if (base.Fish.IsBig)
			{
				return typeof(FishShowBig);
			}
			return typeof(FishShowSmall);
		}
		else
		{
			if (base.Fish.Tackle.IsOnTip && base.Fish.Behavior != FishBehavior.Undefind)
			{
				return typeof(FishDestroy);
			}
			return null;
		}
	}

	protected override void onExit()
	{
		if (base.RodSlot.Bell != null)
		{
			base.RodSlot.Bell.Voice(false);
		}
	}

	private float timeSpentInTechHook;

	private const float TechHookTimeout = 5f;
}
