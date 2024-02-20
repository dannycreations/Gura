using System;
using UnityEngine;

public class LurePitching : LureStateBase
{
	protected override void onEnter()
	{
		base.Lure.HasHitTheGround = false;
		base.Lure.ThrowData.IsThrowing = false;
		base.Line1st.ResetLineWidthChange(0.003f);
		base.RodSlot.Sim.TurnLimitsOff();
	}

	protected override Type onUpdate()
	{
		if (base.Lure.IsLying)
		{
			return typeof(LureIdlePitch);
		}
		if (base.Lure.transform.position.y < 0f)
		{
			return typeof(LureFloating);
		}
		float num = Vector3.Angle(Vector3.down, base.RodSlot.Rod.transform.forward);
		if (!this.increaseLineLength)
		{
			float? num2 = this.priorRodAngle;
			if (num2 != null && this.priorRodAngle < num)
			{
				this.increaseLineLength = true;
				base.RodSlot.Sim.RodTipMass.NextSpring.SpringConstant = 0f;
			}
		}
		this.priorRodAngle = new float?(num);
		base.RodSlot.Line.TransitToNewLineWidth();
		return null;
	}

	protected override void onExit()
	{
		if (base.Lure.CheckGroundHit())
		{
			this._owner.Adapter.Water(0f);
			this._owner.Adapter.FinishGameAction();
		}
		else
		{
			this._owner.Adapter.Water(base.CastLength());
			base.Lure.TackleIn(3f);
		}
		base.RodSlot.Sim.TurnLimitsOn(false);
		base.RodSlot.Sim.RodTipMass.NextSpring.SpringConstant = 500f;
		float num = Vector3.Distance(base.Lure.transform.position, base.Lure.Rod.CurrentUnbendTipPosition);
		base.RodSlot.Sim.FinalLineLength = num * 1.1f;
	}

	private float? priorRodAngle;

	private bool increaseLineLength;
}
