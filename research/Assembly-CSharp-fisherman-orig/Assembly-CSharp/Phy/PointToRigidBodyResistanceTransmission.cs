using System;
using Mono.Simd;
using Mono.Simd.Math;
using UnityEngine;

namespace Phy
{
	public sealed class PointToRigidBodyResistanceTransmission : ConnectionBase
	{
		public PointToRigidBodyResistanceTransmission(PointOnRigidBody point)
			: base(point, point.RigidBody, -1)
		{
			this.point = point;
			this.SinkDutyRatio = 1f;
		}

		public PointToRigidBodyResistanceTransmission(Simulation sim, PointToRigidBodyResistanceTransmission source)
			: base(sim.DictMasses[source.Mass1.UID], sim.DictMasses[source.Mass2.UID], source.UID)
		{
			this.point = sim.DictMasses[source.Mass1.UID] as PointOnRigidBody;
		}

		public Vector4f CurrentThrustForce { get; private set; }

		public override void Sync(ConnectionBase source)
		{
			base.Sync(source);
			PointToRigidBodyResistanceTransmission pointToRigidBodyResistanceTransmission = source as PointToRigidBodyResistanceTransmission;
			this.Radius = pointToRigidBodyResistanceTransmission.Radius;
			this.Area = pointToRigidBodyResistanceTransmission.Area;
			this.ThrustLocalDirection = pointToRigidBodyResistanceTransmission.ThrustLocalDirection;
			this.SimulatedThrustVelocity = pointToRigidBodyResistanceTransmission.SimulatedThrustVelocity;
			if (this.SimulatedThrustVelocity)
			{
				pointToRigidBodyResistanceTransmission.ThrustVelocity = this.ThrustVelocity;
			}
			else
			{
				this.ThrustVelocity = pointToRigidBodyResistanceTransmission.ThrustVelocity;
			}
			pointToRigidBodyResistanceTransmission.CurrentThrustForce = this.CurrentThrustForce;
			this.BodyResistance = pointToRigidBodyResistanceTransmission.BodyResistance;
			if (!Mathf.Approximately(this.Area, 0f))
			{
				this.compensatedThrustFactor = 1f + Mathf.Sqrt(this.BodyResistance / (this.point.WaterDragConstant * this.Area));
			}
			else
			{
				this.compensatedThrustFactor = 0f;
			}
			this.WingFactor = pointToRigidBodyResistanceTransmission.WingFactor;
			this.wingFactor4f = new Vector4f(this.WingFactor);
			this.SinkDutyRatio = pointToRigidBodyResistanceTransmission.SinkDutyRatio;
		}

		public override void Solve()
		{
			this.localVelocity = this.ThrustLocalDirection * this.ThrustVelocity * this.compensatedThrustFactor;
			Vector3 vector = this.localVelocity;
			Vector4f vector4f = this.point.FlowVelocity - this.point.RigidBody.PointWorldVelocity4f(this.point.LocalPosition);
			Vector4f vector4f2 = vector4f - (this.point.RigidBody.Rotation * vector).AsVector4f();
			float num = vector4f2.Magnitude();
			if (!Mathf.Approximately(num, 0f))
			{
				float num2 = Mathf.Clamp01((this.Radius - this.point.Position4f.Y) / (2f * this.Radius));
				float num3 = this.point.WaterDragConstant * Mathf.Abs(num2) / this.SinkDutyRatio;
				Vector4f vector4f3 = vector4f2 * new Vector4f(num3 * this.Area * num);
				vector4f3 += this.point.MassValue4f * Vector4fExtensions.down * ConnectedBodiesSystem.GravityAcceleration4f * new Vector4f(1f - num2);
				this.point.RigidBody.ApplyForce(vector4f3, this.point.LocalPosition, false);
				this.CurrentThrustForce = vector4f3;
			}
			else
			{
				this.CurrentThrustForce = Vector4f.Zero;
			}
		}

		public float Radius;

		public float Area;

		public float ThrustVelocity;

		public Vector3 ThrustLocalDirection;

		public float BodyResistance;

		public float WingFactor;

		public float SinkDutyRatio;

		public bool SimulatedThrustVelocity;

		private PointOnRigidBody point;

		private float compensatedThrustFactor;

		private Vector3 localVelocity;

		private Vector4f wingFactor4f;
	}
}
