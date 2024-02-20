using System;

public class LureHitched : LureStateBase
{
	protected override void onEnter()
	{
		base.Lure.Hitch(base.Lure.transform.position);
		if (SettingsManager.SnagsWarnings || PhotonConnectionFactory.Instance.Profile.Level < 5)
		{
			GameFactory.Message.ShowTackleHitched(RodHelper.FindRodInSlot(base.RodSlot.Index, null));
		}
		if (base.IsInHands)
		{
			base.RodSlot.Sim.TurnLimitsOn(false);
			base.RodSlot.Reel.IsIndicatorOn = true;
		}
	}

	protected override Type onUpdate()
	{
		if (base.AssembledRod.IsRodDisassembled)
		{
			base.Lure.IsHitched = false;
			return typeof(LureBroken);
		}
		if (!base.Lure.IsHitched)
		{
			return typeof(LureFloating);
		}
		if (base.IsInHands && !base.RodSlot.Reel.IsReeling)
		{
			base.Reel1st.IncreaseLineLength();
		}
		this._owner.Adapter.UnHitch();
		return null;
	}

	protected override void onExit()
	{
		base.Lure.UnHitch();
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
