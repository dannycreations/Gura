using System;
using ObjectModel;

public class LureHanging : LureStateBase
{
	protected override Type onUpdate()
	{
		if (base.AssembledRod.IsRodDisassembled)
		{
			return typeof(LureBroken);
		}
		if (base.RodSlot.Rod.CurrentTipPosition.y < base.RodSlot.Line.SecuredLineLength && !base.Lure.IsPulledOut)
		{
			if (base.Lure.Fish == null)
			{
				base.Lure.TackleIn(1f);
				return typeof(LureFloating);
			}
			return typeof(LureSwallowed);
		}
		else
		{
			if (base.Lure.Fish == null && base.IsInHands)
			{
				if (base.player.IsPitching && base.RodSlot.Line.SecuredLineLength <= base.RodSlot.Line.MinLineLengthOnPitch)
				{
					if (base.Lure.UnderwaterItem != null)
					{
						return typeof(LureShowItem);
					}
					this._owner.Adapter.FinishGameAction();
					return typeof(LureIdlePitch);
				}
				else if (!base.player.IsPitching && base.RodSlot.Line.SecuredLineLength <= base.RodSlot.Line.MinLineLength)
				{
					if (base.Lure.UnderwaterItem != null)
					{
						return typeof(LureShowItem);
					}
					this._owner.Adapter.FinishGameAction();
					return typeof(LureOnTip);
				}
			}
			if (!base.IsInHands || base.Lure.Fish == null || !base.RodSlot.Reel.IsReeling || base.RodSlot.Line.SecuredLineLength > base.RodSlot.Line.MinLineLengthWithFish)
			{
				base.Lure.CheckSurfaceCollisions();
				return null;
			}
			if (base.Lure.Fish.Behavior != FishBehavior.Hook)
			{
				base.Lure.EscapeFish();
				this._owner.Adapter.FinishGameAction();
				return typeof(LureOnTip);
			}
			base.Lure.IsShowing = true;
			if (base.Lure.Fish.IsBig)
			{
				return typeof(LureShowBigFish);
			}
			return typeof(LureShowSmallFish);
		}
	}
}
