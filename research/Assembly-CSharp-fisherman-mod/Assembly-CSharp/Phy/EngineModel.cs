using System;
using Mono.Simd.Math;
using UnityEngine;

namespace Phy
{
	public class EngineModel : ConnectionBase
	{
		public EngineModel(PointOnRigidBody point)
			: base(point, point.RigidBody, -1)
		{
			this.point = point;
			this.Radius = 1f;
			this.ShaftMass = 1f;
		}

		public EngineModel(Simulation sim, EngineModel source)
			: base(sim.DictMasses[source.Mass1.UID], sim.DictMasses[source.Mass2.UID], source.UID)
		{
			this.point = sim.DictMasses[source.Mass1.UID] as PointOnRigidBody;
			this.Transmission = source.Transmission;
		}

		public float shaftVelocity { get; private set; }

		public float totalTorque { get; private set; }

		public override void UpdateReferences()
		{
			base.UpdateReferences();
			this.Transmission = (base.Mass1.Sim as ConnectedBodiesSystem).DictConnections[this.Transmission.UID] as PointToRigidBodyResistanceTransmission;
		}

		public override void Sync(ConnectionBase source)
		{
			base.Sync(source);
			EngineModel engineModel = source as EngineModel;
			engineModel.shaftVelocity = this.shaftVelocity;
			engineModel.totalTorque = this.totalTorque;
			this.StarterEngaged = engineModel.StarterEngaged;
			this.FuelRate = engineModel.FuelRate;
			this.Radius = engineModel.Radius;
			this.Clutch = engineModel.Clutch;
			this.ShaftMass = engineModel.ShaftMass;
			this.StarterVelocity = engineModel.StarterVelocity;
			this.StarterTqFactor = engineModel.StarterTqFactor;
			this.shaftInertia = this.ShaftMass * this.Radius * this.Radius * 0.5f;
			this.CombustionTqGain = engineModel.CombustionTqGain;
			this.CombustionTqMax = engineModel.CombustionTqMax;
			this.LossesTqLowGain = engineModel.LossesTqLowGain;
			this.LossesTqLowVelocity = engineModel.LossesTqLowVelocity;
			this.LossesTqHighFactor = engineModel.LossesTqHighFactor;
			this.InputMultiplier = engineModel.InputMultiplier;
			this.OutputMultiplier = engineModel.OutputMultiplier;
		}

		public override void Solve()
		{
			float num = this.Transmission.CurrentThrustForce.Magnitude();
			float num2 = num * this.Radius * this.InputMultiplier;
			float num3 = Mathf.Min(this.FuelRate * this.CombustionTqGain, this.CombustionTqMax) * this.shaftVelocity;
			float num4 = Mathf.Max(0f, this.shaftVelocity - this.LossesTqLowVelocity);
			float num5 = Mathf.Min(this.shaftVelocity, this.LossesTqLowVelocity) * this.LossesTqLowGain + num4 * num4 * this.LossesTqHighFactor;
			float num6 = 0f;
			if (this.StarterEngaged)
			{
				num6 = (this.StarterVelocity - this.shaftVelocity) * this.StarterTqFactor;
			}
			this.totalTorque = num3 + num6 - num5 - num2 * this.Clutch;
			this.shaftVelocity += this.totalTorque * this.shaftInertia * 0.0004f;
			if (this.shaftVelocity < 0.0001f)
			{
				this.shaftVelocity = 0f;
			}
			this.Transmission.ThrustVelocity = this.Clutch * this.shaftVelocity * this.Radius * 3.1415927f * 2f * this.OutputMultiplier;
		}

		private float shaftInertia;

		private PointOnRigidBody point;

		public PointToRigidBodyResistanceTransmission Transmission;

		public bool StarterEngaged;

		public float FuelRate;

		public float Clutch;

		public float StarterVelocity;

		public float StarterTqFactor;

		public float InputMultiplier;

		public float OutputMultiplier;

		public float ShaftMass;

		public float Radius;

		public float CombustionTqGain;

		public float CombustionTqMax;

		public float LossesTqLowGain;

		public float LossesTqLowVelocity;

		public float LossesTqHighFactor;
	}
}
