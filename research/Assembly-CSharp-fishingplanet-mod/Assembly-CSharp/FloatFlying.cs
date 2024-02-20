using System;
using Phy;
using UnityEngine;

public class FloatFlying : FloatStateBase
{
	protected override void onEnter()
	{
		base.Float.HasHitTheGround = false;
		TackleThrowManager tackleThrowManager = new TackleThrowManager(base.Float.ThrowData);
		tackleThrowManager.Init(base.RodSlot.Rod.CurrentTipPosition.y);
		base.RodSlot.Sim.CurrentTacklePosition = base.Float.transform.position;
		base.RodSlot.Sim.FinalTacklePosition = base.Float.transform.position;
		base.RodSlot.Sim.TurnLimitsOff();
		base.Float.Thrown = true;
		base.Line1st.LineObj.SetMotionDamping(0.99f);
		base.Line1st.LeaderObj.SetMotionDamping(0.99f);
		for (int i = 0; i < base.RodSlot.Sim.Connections.Count; i++)
		{
			Spring spring = base.RodSlot.Sim.Connections[i] as Spring;
			if (spring != null && spring.IsRepulsive)
			{
				this.bobberLength = spring.SpringLength;
				break;
			}
		}
		VerticalParabola path = tackleThrowManager.GetPath(WeatherController.Instance.SceneFX.WindVelocity);
		path.DebugDraw(Color.red);
		this.kvp = new KinematicVerticalParabola(base.RodSlot.Sim.TackleTipMass, base.RodSlot.Sim.TackleTipMass, tackleThrowManager.FlightDuration, 1f, path);
		base.RodSlot.Sim.TackleTipMass.IsKinematic = true;
		this.kvp.DestinationPoint = tackleThrowManager.DestinationPoint;
		base.RodSlot.Sim.Connections.Add(this.kvp);
		base.RodSlot.Sim.RefreshObjectArrays(true);
		this.bobberAirDragBackup = base.Float.GetBobberMainMass.AirDragConstant;
		base.Float.GetBobberMainMass.AirDragConstant = 0.5f;
		GameFactory.Player.UpdateTackleThrowData(new Vector3?(tackleThrowManager.DestinationPoint), new float?(tackleThrowManager.angleCast));
	}

	protected override Type onUpdate()
	{
		if (base.AssembledRod.IsRodDisassembled)
		{
			return typeof(FloatBroken);
		}
		if (!GameFactory.Player.IsRodActive)
		{
			return typeof(FloatHidden);
		}
		Vector3 position = base.RodSlot.Sim.TackleTipMass.Position;
		float magnitude = (base.RodSlot.Sim.TackleTipMass.Position - base.RodSlot.Rod.CurrentUnbendTipPosition).magnitude;
		base.RodSlot.Sim.FinalTacklePosition = position;
		base.RodSlot.Sim.FinalLineLength = magnitude * 1f + 1f / Mathf.Max(1f, magnitude) - base.RodSlot.Sim.CurrentLeaderLength - this.bobberLength;
		if (this.kvp.TimeSpent >= this.kvp.Duration || (position.y < 0f && this.kvp.TimeSpent >= 0.75f * this.kvp.Duration))
		{
			return typeof(FloatFloating);
		}
		return null;
	}

	protected override void onExit()
	{
		base.Float.IsKinematic = false;
		base.RodSlot.Sim.TackleTipMass.IsKinematic = false;
		base.Float.GetBobberMainMass.AirDragConstant = this.bobberAirDragBackup;
		base.RodSlot.Sim.TurnLimitsOn(false);
		base.Line1st.LineObj.SetMotionDamping(0f);
		base.Line1st.LeaderObj.SetMotionDamping(0f);
		if (base.Float.CheckGroundHit())
		{
			this._owner.Adapter.Water(0f);
			base.Line1st.ResetToMinLength();
		}
		else
		{
			this._owner.Adapter.Water(base.CastLength());
			base.Float.TackleIn(3f);
		}
		base.RodSlot.Sim.RemoveConnection(this.kvp);
		base.RodSlot.Sim.RefreshObjectArrays(true);
		GameFactory.Player.UpdateTackleThrowData(null, null);
	}

	private const float ThrowLineLengthCorrection = 1f;

	private const float BobberAirDrag = 0.5f;

	private KinematicVerticalParabola kvp;

	private float bobberAirDragBackup;

	private float bobberLength;
}
