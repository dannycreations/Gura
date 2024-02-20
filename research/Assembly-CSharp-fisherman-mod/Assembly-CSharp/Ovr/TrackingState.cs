using System;

namespace Ovr
{
	public struct TrackingState
	{
		public PoseStatef HeadPose;

		public Posef CameraPose;

		public Posef LeveledCameraPose;

		public SensorData RawSensorData;

		public uint StatusFlags;

		public double LastVisionProcessingTime;

		public double LastVisionFrameLatency;

		public uint LastCameraFrameCounter;
	}
}
