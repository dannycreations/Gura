using System;
using ObjectModel;

public class FeederHanging : FeederStateBase
{
	protected override void onEnter()
	{
		IFishController fishController = base.Feeder.AttackingFish ?? base.Feeder.Fish;
		if (fishController != null && (fishController.FishAIBehaviour == FishAiBehavior.Bite || fishController.FishAIBehaviour == FishAiBehavior.PredatorSwim))
		{
			this._owner.Adapter.FinishAttack(false, true, false, false, 0f);
			GameFactory.Message.ShowTackleWasPulledAwayFromFish(RodHelper.FindRodInSlot(base.RodSlot.Index, null));
			fishController.Behavior = FishBehavior.Go;
		}
		if (base.RodSlot.Bell != null)
		{
			base.RodSlot.Bell.Voice(false);
		}
	}

	protected override Type onUpdate()
	{
		if (base.AssembledRod.IsRodDisassembled)
		{
			return typeof(FeederBroken);
		}
		if (base.RodSlot.Rod.CurrentTipPosition.y < base.RodSlot.Line.SecuredLineLength && !base.Feeder.IsPulledOut)
		{
			if (base.Feeder.Fish == null)
			{
				base.Feeder.TackleIn(1f);
				return typeof(FeederFloating);
			}
			return typeof(FeederSwallowed);
		}
		else
		{
			if (base.Feeder.Fish == null && base.IsInHands)
			{
				if (base.player.IsPitching && base.RodSlot.Line.SecuredLineLength <= base.RodSlot.Line.MinLineLengthOnPitch)
				{
					if (base.Feeder.HasUnderwaterItem)
					{
						return typeof(FeederShowItem);
					}
					this._owner.Adapter.FinishGameAction();
					return typeof(FeederIdlePitch);
				}
				else if (!base.player.IsPitching && base.RodSlot.Line.SecuredLineLength <= base.RodSlot.Line.MinLineLength)
				{
					if (base.Feeder.HasUnderwaterItem)
					{
						return typeof(FeederShowItem);
					}
					this._owner.Adapter.FinishGameAction();
					return typeof(FeederOnTip);
				}
			}
			if (base.Feeder.Fish == null || !base.RodSlot.Reel.IsReeling || !base.IsInHands || (base.RodSlot.Line.FullLineLength > base.RodSlot.Line.MinLineLengthWithFish && base.RodSlot.Line.SecuredLineLength > base.RodSlot.Line.MinLineLength))
			{
				base.Feeder.CheckSurfaceCollisions();
				return null;
			}
			if (base.Feeder.Fish.Behavior != FishBehavior.Hook)
			{
				base.Feeder.EscapeFish();
				this._owner.Adapter.FinishGameAction();
				return typeof(FeederOnTip);
			}
			base.Feeder.IsShowing = true;
			if (base.Feeder.Fish.IsBig)
			{
				return typeof(FeederShowBigFish);
			}
			return typeof(FeederShowSmallFish);
		}
	}
}
