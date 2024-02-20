using System;
using Assets.Scripts.Phy.Simulations;
using UnityEngine;

public class LureFloating : LureStateBase
{
	protected override void onEnter()
	{
		base.Lure.ResetDrag();
		this.timeSpent = 0f;
		if (base.IsInHands)
		{
			base.RodSlot.Sim.TurnLimitsOn(false);
			if (Debug.isDebugBuild)
			{
				base.RodSlot.Sim.TackleMoveCompensationMode = TackleMoveCompensationMode.TackleCasted;
			}
		}
	}

	protected override Type onUpdate()
	{
		if (base.AssembledRod.IsRodDisassembled)
		{
			return typeof(LureBroken);
		}
		if (base.Lure.HasHitTheGround || base.Lure.IsOutOfTerrain)
		{
			this._owner.Adapter.FinishGameAction();
			if (base.IsInHands && base.player.IsPitching)
			{
				return typeof(LureIdlePitch);
			}
			return typeof(LureOnTip);
		}
		else
		{
			if (base.Lure.IsHitched)
			{
				return typeof(LureHitched);
			}
			if (base.Lure.Fish != null)
			{
				if (base.Lure.IsBeingPulled)
				{
					this._owner.Adapter.FinishAttack(false, false, true, false, 0f);
				}
				return typeof(LureSwallowed);
			}
			if (!base.Lure.UnderwaterItemIsLoading && (base.RodSlot.Rod.CurrentTipPosition.y > base.RodSlot.Line.SecuredLineLength || base.RodSlot.Rod.TackleTipMass.GroundHeight > 0f) && base.Lure.IsPulledOut)
			{
				base.Lure.TackleOut(1f);
				return typeof(LureHanging);
			}
			if (base.IsInHands)
			{
				if (base.player.IsPitching && base.RodSlot.Line.SecuredLineLength <= base.RodSlot.Line.MinLineLengthOnPitch)
				{
					if (base.Lure.UnderwaterItem != null)
					{
						return typeof(LureShowItem);
					}
					if (!base.Lure.UnderwaterItemIsLoading)
					{
						this._owner.Adapter.FinishGameAction();
						return typeof(LureIdlePitch);
					}
				}
				if (!base.player.IsPitching && base.RodSlot.Line.SecuredLineLength <= base.RodSlot.Line.MinLineLength)
				{
					if (base.Lure.UnderwaterItem != null)
					{
						return typeof(LureShowItem);
					}
					if (!base.Lure.UnderwaterItemIsLoading)
					{
						this._owner.Adapter.FinishGameAction();
						return typeof(LureOnTip);
					}
				}
				if (this.timeSpent < 2f)
				{
					this.timeSpent += Time.deltaTime;
					if (this.timeSpent > 2f)
					{
						base.RodSlot.Reel.IsIndicatorOn = true;
					}
				}
				base.Lure.UpdateLureDepthStatus();
			}
			base.Lure.CheckSurfaceCollisions();
			this._owner.Adapter.Move(false);
			return null;
		}
	}

	protected override void onExit()
	{
		if (base.IsInHands)
		{
			base.RodSlot.Sim.TurnLimitsOff();
		}
	}

	private float timeSpent;

	private const float IndicatorOnTimeout = 2f;
}
