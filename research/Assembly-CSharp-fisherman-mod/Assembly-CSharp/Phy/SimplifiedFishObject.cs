using System;
using UnityEngine;

namespace Phy
{
	public class SimplifiedFishObject : PhyObject, IFishObject
	{
		public SimplifiedFishObject(ConnectedBodiesSystem sim, float mass, Vector3 position, float length, float maxForce, float maxSpeed)
			: base(PhyObjectType.Fish, sim)
		{
			this.TotalMass = mass;
			base.Masses.Add(new Mass(sim, mass * 0.5f, position, Mass.MassType.Fish)
			{
				Buoyancy = 0f
			});
			base.Masses.Add(new Mass(sim, mass * 0.5f, position + length * Vector3.forward, Mass.MassType.Fish)
			{
				Buoyancy = 0f
			});
			base.Connections.Add(new Spring(this.Mouth, this.Root, 0f, length, 0f)
			{
				IsRepulsive = false
			});
			this.ComputeWaterDrag(maxForce, maxSpeed);
			this.UpdateSim();
		}

		public float TotalMass { get; private set; }

		public Mass Mouth
		{
			get
			{
				return base.Masses[0];
			}
		}

		public Mass Root
		{
			get
			{
				return base.Masses[1];
			}
		}

		public void SetThrust(Vector3 force, bool reverse)
		{
			if (!reverse)
			{
				this.Mouth.WaterMotor = force * 1.1f;
				this.Root.WaterMotor = -force * 0.1f;
			}
			else
			{
				this.Root.WaterMotor = force * 1.1f;
				this.Mouth.WaterMotor = -force * 0.1f;
			}
		}

		public void ComputeWaterDrag(float NominalForce, float MaxSpeed)
		{
			float num = NominalForce / (MaxSpeed * MaxSpeed * this.TotalMass);
			this.Mouth.WaterDragConstant = num;
			this.Root.WaterDragConstant = num;
		}

		public float ComputeSpeedForce(float targetSpeed)
		{
			return targetSpeed * targetSpeed * this.TotalMass * this.Mouth.WaterDragConstant;
		}
	}
}
