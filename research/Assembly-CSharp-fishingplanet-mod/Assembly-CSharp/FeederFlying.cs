using System;
using Phy;
using UnityEngine;

public class FeederFlying : FeederStateBase
{
	protected override void onEnter()
	{
		base.Feeder.HasHitTheGround = false;
		TackleThrowManager tackleThrowManager = new TackleThrowManager(base.Feeder.ThrowData);
		tackleThrowManager.Init(base.RodSlot.Rod.CurrentTipPosition.y);
		base.RodSlot.Sim.CurrentTacklePosition = base.Feeder.transform.position;
		base.RodSlot.Sim.FinalTacklePosition = base.Feeder.transform.position;
		base.RodSlot.Sim.TurnLimitsOff();
		base.Feeder.Thrown = true;
		VerticalParabola path = tackleThrowManager.GetPath(WeatherController.Instance.SceneFX.WindVelocity);
		path.DebugDraw(Color.red);
		this.tackleMass = base.Feeder.RigidBody;
		this.kvp = new KinematicVerticalParabola(this.tackleMass, this.tackleMass, tackleThrowManager.FlightDuration, 1f, path);
		this.tackleMass.IsKinematic = true;
		this.kvp.DestinationPoint = tackleThrowManager.DestinationPoint;
		base.RodSlot.Sim.Connections.Add(this.kvp);
		base.RodSlot.Sim.RefreshObjectArrays(true);
		base.RodSlot.Sim.LineTipMass.NextSpring.AffectMass2Factor = 0.2f;
		this.feederAirDragBackup = base.Feeder.RigidBody.AirDragConstant;
		base.Feeder.RigidBody.AirDragConstant = this.feederAirDragBackup * 5f;
		GameFactory.Player.UpdateTackleThrowData(new Vector3?(tackleThrowManager.DestinationPoint), new float?(tackleThrowManager.angleCast));
		this.hookAirDrag = base.RodSlot.Sim.TackleTipMass.AirDragConstant;
		base.RodSlot.Sim.TackleTipMass.AirDragConstant = this.hookAirDrag * 10f;
	}

	protected override Type onUpdate()
	{
		if (base.AssembledRod.IsRodDisassembled)
		{
			return typeof(FeederBroken);
		}
		if (!GameFactory.Player.IsRodActive)
		{
			return typeof(FeederHidden);
		}
		Vector3 position = this.tackleMass.Position;
		float magnitude = (this.tackleMass.Position - base.RodSlot.Sim.RodTipMass.Position).magnitude;
		base.RodSlot.Sim.FinalTacklePosition = position;
		base.RodSlot.Sim.FinalLineLength = magnitude * 1f + 1f / Mathf.Max(1f, magnitude);
		float num = Mathf.Max(magnitude * 1f, base.RodSlot.Sim.CurrentLineLength);
		base.RodSlot.Sim.FinalLineLength = Mathf.Min(num, base.RodSlot.Line.MaxLineLength);
		this.prevTackleVelocity = base.Feeder.RigidBody.Velocity;
		if (num > base.RodSlot.Line.MaxLineLength && this.tackleMass.IsKinematic)
		{
			base.Feeder.IsClipStrike = true;
			base.RodSlot.Reel.IsIndicatorOn = true;
			this.tackleMass.IsKinematic = false;
			base.Feeder.IsKinematic = false;
			this.tackleMass.Velocity = this.prevTackleVelocity;
			base.RodSlot.Sim.TackleTipMass.AirDragConstant = this.hookAirDrag;
			base.Feeder.RigidBody.AirDragConstant = this.feederAirDragBackup;
		}
		if ((this.tackleMass.IsKinematic && this.kvp.TimeSpent >= this.kvp.Duration) || (position.y < 0f && this.kvp.TimeSpent >= 0.75f * this.kvp.Duration))
		{
			return typeof(FeederFloating);
		}
		return null;
	}

	protected override void onExit()
	{
		base.RodSlot.Sim.TackleTipMass.AirDragConstant = this.hookAirDrag;
		base.Feeder.IsClipStrike = false;
		if (this.tackleMass.IsKinematic)
		{
			base.Feeder.IsKinematic = false;
			this.tackleMass.IsKinematic = false;
			this.tackleMass.Velocity = this.prevTackleVelocity;
		}
		base.Feeder.RigidBody.AirDragConstant = this.feederAirDragBackup;
		base.RodSlot.Sim.TurnLimitsOn(false);
		if (base.Feeder.CheckGroundHit())
		{
			this._owner.Adapter.Water(0f);
			base.RodSlot.Sim.FinalLineLength = base.RodSlot.Line.MinLineLength;
			base.RodSlot.Sim.HorizontalRealignLineMasses();
		}
		else
		{
			this._owner.Adapter.Water(base.CastLength());
			base.Feeder.TackleIn(3f);
		}
		base.RodSlot.Sim.RemoveConnection(this.kvp);
		base.RodSlot.Sim.RefreshObjectArrays(true);
		GameFactory.Player.UpdateTackleThrowData(null, null);
	}

	private const float ThrowLineLengthCorrection = 1f;

	private const float FeederSpringAffectFactor = 0.2f;

	private TackleThrowManager throwManager;

	private KinematicVerticalParabola kvp;

	private Vector3 prevTackleVelocity;

	private Mass tackleMass;

	private float hookAirDrag;

	private float feederAirDragBackup;
}
