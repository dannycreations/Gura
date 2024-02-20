using System;
using System.Linq;
using ObjectModel;
using UnityEngine;

public class PlayerDrawIn : PlayerStateBase
{
	protected override void OnCantOpenMenu(MainMenuPage page)
	{
		PlayerStateBase._savedPage = page;
	}

	protected override void onEnter()
	{
		base.onEnter();
		if (base.Player.Rod != null)
		{
			base.Player.SetHandsVisibility(true);
			base.Player.Reel.SetVisibility(true);
		}
		base.Player.Update3dCharMecanimParameter(TPMMecanimIParameter.ThrowType, 0);
		base.Player.ShowSleeves = false;
		GameFactory.Player.HudFishingHandler.State = HudState.Fight;
		if (!base.Player.Rod.SimulationValid)
		{
			base.Player.Rod.SetVisibility(false);
			base.Player.Tackle.SetVisibility(false);
			base.Player.Line.SetVisibility(false);
		}
		base.PlaySound(PlayerStateBase.Sounds.RodDrawIn);
		if (!StaticUserData.IS_IN_TUTORIAL)
		{
			PhotonConnectionFactory.Instance.ChangeSwitch(GameSwitchType.Pitching, base.Player.IsPitching, null);
		}
		base.PlayDrawInAnimation(0f);
		if (StaticUserData.RodInHand.Feeder != null)
		{
			Chum chum = FeederHelper.FindPreparedChumActiveRodAll().FirstOrDefault<Chum>();
			base.Player.Feeder.SetFilled(chum != null && chum.Weight != null && !Mathf.Approximately((float)chum.Weight.Value, 0f));
		}
	}

	protected override Type onUpdate()
	{
		if (this.frameCounter == 0)
		{
			base.Player.RodSlot.Sim.RodTransformResetFlag = true;
			if (!base.Player.Rod.SimulationValid)
			{
				base.Player.Rod.ReinitializeSimulation(null);
			}
		}
		else if (this.frameCounter == 1)
		{
			base.Player.Rod.SetVisibility(true);
			base.Player.Tackle.SetVisibility(true);
			base.Player.Line.SetVisibility(true);
		}
		this.frameCounter++;
		if (!base.IsAnimationFinished || this.frameCounter <= 1)
		{
			return null;
		}
		if (base.Player.IsPitching)
		{
			return typeof(PlayerIdlePitch);
		}
		return typeof(PlayerIdle);
	}

	protected override void onExit()
	{
		base.Player.RodSlot.Sim.TurnLimitsOff();
		base.Player.ShowSleeves = true;
	}

	private int frameCounter;
}
