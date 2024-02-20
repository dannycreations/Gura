using System;

namespace Ovr
{
	public struct EyeRenderDesc
	{
		public Eye Eye;

		public FovPort Fov;

		public Recti DistortedViewport;

		public Vector2f PixelsPerTanAngleAtCenter;

		public Vector3f HmdToEyeViewOffset;
	}
}
