using System;
using Phy;
using UnityEngine;

public class FeederPitching : FeederStateBase
{
	protected override void onEnter()
	{
		base.Feeder.HasHitTheGround = false;
		base.Feeder.IsThrowing = false;
		base.Line1st.ResetLineWidthChange(0.003f);
		base.RodSlot.Sim.StopAllBodies();
		base.RodSlot.Sim.TurnLimitsOff();
		for (int i = 0; i < base.RodSlot.Sim.Connections.Count; i++)
		{
			Spring spring = base.RodSlot.Sim.Connections[i] as Spring;
			if (spring != null && spring.IsRepulsive)
			{
				this.dampedMass = spring.Mass1;
				this.bobberSpring = spring;
				this.kinematicMass = spring.Mass2;
				break;
			}
		}
		this.dampedMass.MotionDamping = 0.9f;
		base.Feeder.RigidBody.AirDragConstant = 0.1f;
		this.bobberLength = this.bobberSpring.SpringLength;
		this.kinematicMass.StopMass();
		this.kinematicMass.IsKinematic = true;
		this.timeSpent = 0f;
		Vector3 forward = base.RodSlot.Rod.transform.forward;
		forward.y = 0f;
		forward.Normalize();
		Vector3 vector = this.kinematicMass.Position + forward * (base.RodSlot.Rod.Length + 2f);
		vector.y = -0.1f;
		this.flightCurve = new VerticalParabola(this.kinematicMass.Position, vector, this.kinematicMass.Position.y + 1f);
		base.Line1st.SetLineSpringConstant(50f);
		base.Feeder.Thrown = true;
		GameFactory.Player.UpdateTackleThrowData(new Vector3?(vector), new float?(Mathf.Atan(this.flightCurve.GetDerivative(0f))));
		this.kvp = new KinematicVerticalParabola(this.kinematicMass, this.kinematicMass, 1.5f, 1.25f, this.flightCurve)
		{
			DestinationPoint = vector
		};
		base.RodSlot.Sim.Connections.Add(this.kvp);
		base.RodSlot.Sim.RefreshObjectArrays(true);
	}

	protected override Type onUpdate()
	{
		if (this.kvp.TimeSpent >= 0.75f * this.kvp.Duration)
		{
			if (base.Feeder.IsLying && base.Feeder.transform.position.y > 0f)
			{
				return typeof(FeederIdlePitch);
			}
			if (base.Feeder.transform.position.y < 0f)
			{
				return typeof(FeederFloating);
			}
		}
		if (this.kvp.TimeSpent >= this.kvp.Duration)
		{
			return typeof(FeederFloating);
		}
		float num = this.timeSpent / 1.5f;
		if (num > 0.5f)
		{
			num += Mathf.Pow((num - 0.5f) * 2f, 2f) * 0.2f;
		}
		Vector3 point = this.flightCurve.GetPoint(num);
		Debug.DrawLine(point, this.kinematicMass.Position, Color.green, 2f);
		base.RodSlot.Line.TransitToNewLineWidth();
		this.timeSpent += Time.deltaTime;
		return null;
	}

	protected override void onExit()
	{
		base.Line1st.ReleaseKinematicMass(this.kinematicMass);
		this.bobberSpring.SpringLength = this.bobberLength;
		base.Feeder.RigidBody.AirDragConstant = 0.001f;
		base.Line1st.SetLineSpringConstant(500f);
		base.RodSlot.Sim.RodTipMass.NextSpring.SpringConstant = 500f;
		float num = Vector3.Distance(base.Feeder.transform.position, base.Feeder.Rod.CurrentUnbendTipPosition);
		float num2 = Mathf.Max(base.Line1st.GetLinePhysicsLength() * 1.25f, base.RodSlot.Line.MinLineLengthWithFish + 0.5f);
		if (num2 > base.RodSlot.Line.MinLineLengthOnPitch)
		{
			base.RodSlot.Sim.FinalLineLength = num2;
		}
		if (base.Feeder.CheckGroundHit() || base.Feeder.CheckPitchIsTooShort())
		{
			this._owner.Adapter.Water(0f);
			this._owner.Adapter.FinishGameAction();
		}
		else
		{
			this._owner.Adapter.Water(base.CastLength());
			base.Feeder.TackleIn(3f);
		}
		base.RodSlot.Sim.TurnLimitsOn(false);
		base.RodSlot.Sim.RemoveConnection(this.kvp);
		base.RodSlot.Sim.RefreshObjectArrays(true);
		GameFactory.Player.UpdateTackleThrowData(null, null);
	}

	private const float duration = 1.5f;

	private const float distanceFactor = 2.2f;

	private const float midYDelta = 1f;

	private float? priorRodAngle;

	private bool increaseLineLength;

	private Mass kinematicMass;

	private Mass dampedMass;

	private Spring bobberSpring;

	private VerticalParabola flightCurve;

	private KinematicVerticalParabola kvp;

	private float timeSpent;

	private float bobberLength;
}
