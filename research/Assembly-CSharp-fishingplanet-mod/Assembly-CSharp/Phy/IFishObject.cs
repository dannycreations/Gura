using System;
using UnityEngine;

namespace Phy
{
	public interface IFishObject
	{
		float TotalMass { get; }

		Mass Mouth { get; }

		Mass Root { get; }

		void SetThrust(Vector3 force, bool reverse);

		float ComputeSpeedForce(float targetSpeed);

		void ComputeWaterDrag(float NominalForce, float MaxSpeed);
	}
}
