using System;
using UnityEngine;

namespace Phy
{
	public class RopeSimulation : ConnectedBodiesSystem
	{
		public RopeSimulation(Vector3 connectionPoint, float segmentLength, float length, float segmentMass)
			: base("RopeSimulation")
		{
			int num = (int)(length / segmentLength) + 1;
			Mass mass = null;
			for (int i = 0; i < num; i++)
			{
				Mass mass2 = new Mass(GameFactory.Player.RodSlot.Sim, segmentMass, connectionPoint + new Vector3(0f, 0f, segmentLength * (float)i), Mass.MassType.Unknown);
				base.Masses.Add(mass2);
				if (mass != null)
				{
					Spring spring = new Spring(mass, mass2, 2000f, segmentLength, 0.2f);
					base.Connections.Add(spring);
					mass.NextSpring = spring;
					mass2.PriorSpring = spring;
				}
				mass = mass2;
			}
			this.RopeConnectionPosition = connectionPoint;
			this.RopeConnectionVelocity = Vector3.zero;
		}

		public Vector3 RopeConnectionPosition { get; set; }

		public Vector3 RopeConnectionVelocity { get; set; }

		protected override void Simulate(float dt)
		{
			base.Simulate(dt);
			this.RopeConnectionPosition += this.RopeConnectionVelocity * dt;
			if (this.RopeConnectionPosition.y < 0f)
			{
				this.RopeConnectionPosition = new Vector3(this.RopeConnectionPosition.x, 0f, this.RopeConnectionPosition.z);
				this.RopeConnectionVelocity = new Vector3(this.RopeConnectionVelocity.x, 0f, this.RopeConnectionVelocity.z);
			}
			base.Masses[0].Position = this.RopeConnectionPosition;
			base.Masses[0].Velocity = this.RopeConnectionVelocity;
		}

		public override void SetVelocity(Vector3 connectionVelocity)
		{
			this.RopeConnectionVelocity = connectionVelocity;
		}

		public override void SetAngularVelocity(Vector3 connectionVelocity)
		{
		}

		public const float SpringConstant = 2000f;

		public const float SpringFrictionConstant = 0.2f;
	}
}
