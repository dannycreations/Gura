using System;

namespace Ovr
{
	public struct SensorData
	{
		public Vector3f Accelerometer;

		public Vector3f Gyro;

		public Vector3f Magnetometer;

		public float Temperature;

		public float TimeInSeconds;
	}
}
