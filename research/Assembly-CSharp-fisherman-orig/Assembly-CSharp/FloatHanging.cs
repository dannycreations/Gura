using System;
using ObjectModel;

public class FloatHanging : FloatStateBase
{
	protected override void onEnter()
	{
		IFishController fishController = base.Float.AttackingFish ?? base.Float.Fish;
		if (fishController != null && (fishController.FishAIBehaviour == FishAiBehavior.Bite || fishController.FishAIBehaviour == FishAiBehavior.PredatorSwim))
		{
			this._owner.Adapter.FinishAttack(false, true, false, false, 0f);
			GameFactory.Message.ShowTackleWasPulledAwayFromFish(RodHelper.FindRodInSlot(base.RodSlot.Index, null));
			fishController.Behavior = FishBehavior.Go;
		}
	}

	protected override Type onUpdate()
	{
		if (base.AssembledRod.IsRodDisassembled)
		{
			return typeof(FloatBroken);
		}
		if (base.RodSlot.Rod.CurrentTipPosition.y < base.RodSlot.Line.SecuredLineLength && !base.Float.IsPulledOut)
		{
			if (base.Float.Fish == null)
			{
				base.Float.TackleIn(1f);
				return typeof(FloatFloating);
			}
			return typeof(FloatSwallowed);
		}
		else
		{
			if (base.Float.Fish == null && base.IsInHands && !base.RodSlot.Rod.IsOngoingRodPodTransition)
			{
				if (base.player.IsPitching && base.RodSlot.Line.SecuredLineLength <= base.RodSlot.Line.MinLineLengthOnPitch)
				{
					if (base.Float.HasUnderwaterItem)
					{
						return typeof(FloatShowItem);
					}
					this._owner.Adapter.FinishGameAction();
					return typeof(FloatIdlePitch);
				}
				else if (!base.player.IsPitching && base.RodSlot.Line.SecuredLineLength <= base.RodSlot.Line.MinLineLength)
				{
					if (base.Float.HasUnderwaterItem)
					{
						return typeof(FloatShowItem);
					}
					this._owner.Adapter.FinishGameAction();
					return typeof(FloatOnTip);
				}
			}
			if (base.Float.Fish == null || !base.RodSlot.Reel.IsReeling || !base.IsInHands || (base.RodSlot.Line.FullLineLength > base.RodSlot.Line.MinLineLengthWithFish && base.RodSlot.Line.SecuredLineLength > base.RodSlot.Line.MinLineLength))
			{
				base.Float.CheckSurfaceCollisions();
				return null;
			}
			if (base.Float.Fish.Behavior != FishBehavior.Hook)
			{
				base.Float.EscapeFish();
				this._owner.Adapter.FinishGameAction();
				return typeof(FloatOnTip);
			}
			base.Float.IsShowing = true;
			if (base.Float.Fish.IsBig)
			{
				return typeof(FloatShowBigFish);
			}
			return typeof(FloatShowSmallFish);
		}
	}
}
