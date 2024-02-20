using System;
using ObjectModel;

public class FishAttack : FishStateBase
{
	protected override void onEnter()
	{
		base.Fish.Tackle = base.RodSlot.Tackle;
		base.Fish.Attack();
		base.Adapter.ResetPeriod();
	}

	protected override Type onUpdate()
	{
		if (base.RodSlot.Reel.HasMaxSpeed && base.RodSlot.Reel.IsReeling)
		{
			base.Adapter.FinishAttack(false, true, false, false, 0f);
			base.Fish.Escape();
			return typeof(FishEscape);
		}
		if (base.Fish.Tackle == null || !base.Fish.Tackle.IsInWater)
		{
			return typeof(FishEscape);
		}
		if (base.Fish.Behavior == FishBehavior.Go)
		{
			return typeof(FishEscape);
		}
		if (base.Fish.IsPathCompleted && !base.Fish.Tackle.IsLying && !base.Fish.IsAttackDelayed)
		{
			return typeof(FishHooked);
		}
		return null;
	}
}
