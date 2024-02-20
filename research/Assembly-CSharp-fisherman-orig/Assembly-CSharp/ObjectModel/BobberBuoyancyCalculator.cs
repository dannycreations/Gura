using System;

namespace ObjectModel
{
	public static class BobberBuoyancyCalculator
	{
		public static bool IsBobberFloating(float buoyancy, float mass, float sinkerMass, float baitMass)
		{
			float num = buoyancy * mass;
			float num2 = -sinkerMass;
			float num3 = -baitMass;
			float num4 = -0.002f;
			float num5 = num + num2 + num3 + num4;
			return num5 > 0.001f;
		}

		private const float BuoyancyThreshold = 0.001f;

		private const float HookMass = 0.001f;
	}
}
