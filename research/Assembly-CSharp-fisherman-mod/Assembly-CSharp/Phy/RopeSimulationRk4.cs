using System;
using UnityEngine;

namespace Phy
{
	public class RopeSimulationRk4
	{
		public RopeSimulationRk4(Vector3 connectionPoint, float segmentLength, float length, float segmentMass)
		{
		}

		public Vector3 RopeConnectionPosition { get; set; }

		public Vector3 RopeConnectionVelocity { get; set; }

		protected void Integrate(float dt)
		{
			this.RopeConnectionPosition += this.RopeConnectionVelocity * dt;
			if (this.RopeConnectionPosition.y < 0f)
			{
				this.RopeConnectionPosition = new Vector3(this.RopeConnectionPosition.x, 0f, this.RopeConnectionPosition.z);
				this.RopeConnectionVelocity = new Vector3(this.RopeConnectionVelocity.x, 0f, this.RopeConnectionVelocity.z);
			}
		}

		public virtual void Update(float dt)
		{
			int num = (int)(dt / 0.0012f) + 1;
			if (num != 0)
			{
				dt /= (float)num;
			}
			for (int i = 0; i < num; i++)
			{
				this.Integrate(dt);
			}
		}

		public const float TimeQuant = 0.0012f;

		public const float SpringConstant = 2000f;

		public const float SpringFrictionConstant = 0.2f;
	}
}
