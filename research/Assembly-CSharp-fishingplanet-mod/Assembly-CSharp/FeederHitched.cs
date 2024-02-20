using System;

public class FeederHitched : FeederStateBase
{
	protected override void onEnter()
	{
		base.Feeder.Hitch(base.Feeder.Rod.TackleTipMass.Position);
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
			base.Feeder.IsHitched = false;
			base.Feeder.DestroyFeeding();
			return typeof(FeederBroken);
		}
		if (!base.Feeder.IsHitched)
		{
			return typeof(FeederFloating);
		}
		base.Feeder.UpdateFeeding(base.Feeder.RigidBody.Position);
		this._owner.Adapter.UnHitch();
		if (base.IsInHands && !base.RodSlot.Reel.IsReeling)
		{
			base.Reel1st.IncreaseLineLength();
		}
		return null;
	}

	protected override void onExit()
	{
		base.Feeder.UnHitch();
		if (!base.AssembledRod.IsRodDisassembled && (SettingsManager.SnagsWarnings || PhotonConnectionFactory.Instance.Profile.Level < 5))
		{
			GameFactory.Message.ShowTackleUnhitched();
		}
		if (base.IsInHands)
		{
			base.RodSlot.Reel.UpdateFrictionState(0f);
			base.RodSlot.Sim.TurnLimitsOn(false);
			base.RodSlot.Sim.RealignLineAfterHitch(true);
		}
	}
}
