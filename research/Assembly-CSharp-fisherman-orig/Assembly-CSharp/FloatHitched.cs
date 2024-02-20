using System;

public class FloatHitched : FloatStateBase
{
	protected override void onEnter()
	{
		base.Float.Hitch(base.Float.Rod.TackleTipMass.Position);
		if (SettingsManager.SnagsWarnings || PhotonConnectionFactory.Instance.Profile.Level < 5)
		{
			GameFactory.Message.ShowTackleHitched(RodHelper.FindRodInSlot(base.RodSlot.Index, null));
		}
		if (base.IsInHands)
		{
			base.Float.LeaderLength = base.Float.UserSetLeaderLength;
			base.RodSlot.Sim.TurnLimitsOn(false);
			base.RodSlot.Reel.IsIndicatorOn = true;
		}
	}

	protected override Type onUpdate()
	{
		if (base.AssembledRod.IsRodDisassembled)
		{
			base.Float.IsHitched = false;
			return typeof(FloatBroken);
		}
		if (!base.Float.IsHitched)
		{
			return typeof(FloatFloating);
		}
		this._owner.Adapter.UnHitch();
		if (base.IsInHands && !base.RodSlot.Reel.IsReeling)
		{
			base.Reel1st.IncreaseLineLength();
		}
		return null;
	}

	protected override void onExit()
	{
		base.Float.UnHitch();
		if (!base.AssembledRod.IsRodDisassembled && (SettingsManager.SnagsWarnings || PhotonConnectionFactory.Instance.Profile.Level < 5))
		{
			GameFactory.Message.ShowTackleUnhitched();
		}
		if (base.IsInHands)
		{
			base.RodSlot.Reel.UpdateFrictionState(0f);
			base.RodSlot.Sim.TurnLimitsOn(false);
			base.RodSlot.Sim.RealignLineAfterHitch(false);
		}
	}
}
