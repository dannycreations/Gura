using System;
using Phy;
using UnityEngine;

public class LureFlying : LureStateBase
{
	protected override void onEnter()
	{
		base.Lure.HasHitTheGround = false;
		base.Lure.ThrowData.Windage = base.Lure.Windage;
		this.throwManager = new TackleThrowManager(base.Lure.ThrowData);
		this.throwManager.Init(base.RodSlot.Rod.CurrentTipPosition.y);
		base.RodSlot.Sim.CurrentTacklePosition = base.Lure.transform.position;
		base.RodSlot.Sim.FinalTacklePosition = base.Lure.transform.position;
		base.RodSlot.Sim.TurnLimitsOff();
		this.tackleMass = base.RodSlot.Sim.TackleTipMass;
		PointOnRigidBody pointOnRigidBody = this.tackleMass as PointOnRigidBody;
		if (pointOnRigidBody != null)
		{
			this.tackleMass = pointOnRigidBody.RigidBody;
		}
		VerticalParabola path = this.throwManager.GetPath(WeatherController.Instance.SceneFX.WindVelocity);
		path.DebugDraw(Color.red);
		this.kvp = new KinematicVerticalParabola(this.tackleMass, this.tackleMass, this.throwManager.FlightDuration, 1f, path);
		this.tackleMass.IsKinematic = true;
		this.kvp.DestinationPoint = this.throwManager.DestinationPoint;
		base.RodSlot.Sim.Connections.Add(this.kvp);
		base.RodSlot.Sim.RefreshObjectArrays(true);
		GameFactory.Player.UpdateTackleThrowData(new Vector3?(this.throwManager.DestinationPoint), new float?(this.throwManager.angleCast));
	}

	protected override Type onUpdate()
	{
		if (base.AssembledRod.IsRodDisassembled)
		{
			return typeof(LureBroken);
		}
		if (!GameFactory.Player.IsRodActive)
		{
			return typeof(LureHidden);
		}
		Vector3 position = this.tackleMass.Position;
		Vector3 vector = position - base.RodSlot.Rod.CurrentUnbendTipPosition;
		base.RodSlot.Sim.FinalTacklePosition = position;
		base.RodSlot.Sim.FinalLineLength = Mathf.Max(vector.magnitude * 1f, base.RodSlot.Sim.CurrentLineLength);
		if (this.kvp.TimeSpent >= this.kvp.Duration || (position.y < 0f && this.kvp.TimeSpent > 0.5f * this.kvp.Duration))
		{
			return typeof(LureFloating);
		}
		return null;
	}

	protected override void onExit()
	{
		base.Lure.IsKinematic = false;
		this.tackleMass.IsKinematic = false;
		if (base.Lure.CheckGroundHit())
		{
			this._owner.Adapter.Water(0f);
			base.Line1st.ResetToMinLength();
		}
		else
		{
			this._owner.Adapter.Water(base.CastLength());
			base.Lure.TackleIn(3f);
			base.Lure.RealignMasses();
			base.RodSlot.Sim.TurnLimitsOn(false);
		}
		base.RodSlot.Sim.RemoveConnection(this.kvp);
		base.RodSlot.Sim.RefreshObjectArrays(true);
		GameFactory.Player.UpdateTackleThrowData(null, null);
	}

	private const float ThrowLineLengthCorrection = 1f;

	private TackleThrowManager throwManager;

	private KinematicVerticalParabola kvp;

	private Mass tackleMass;
}
