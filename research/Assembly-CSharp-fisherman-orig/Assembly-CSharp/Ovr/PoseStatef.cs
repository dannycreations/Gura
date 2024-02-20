using System;

namespace Ovr
{
	public struct PoseStatef
	{
		public Posef ThePose;

		public Vector3f AngularVelocity;

		public Vector3f LinearVelocity;

		public Vector3f AngularAcceleration;

		public Vector3f LinearAcceleration;

		public double TimeInSeconds;
	}
}
