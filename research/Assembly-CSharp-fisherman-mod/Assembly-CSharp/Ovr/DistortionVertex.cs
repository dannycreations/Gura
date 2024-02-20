using System;

namespace Ovr
{
	public struct DistortionVertex
	{
		public Vector2f ScreenPosNDC;

		public float TimeWarpFactor;

		public float VignetteFactor;

		public Vector2f TanEyeAnglesR;

		public Vector2f TanEyeAnglesG;

		public Vector2f TanEyeAnglesB;
	}
}
