using System;

namespace Ovr
{
	public enum StatusBits
	{
		OrientationTracked = 1,
		PositionTracked,
		CameraPoseTracked = 4,
		PositionConnected = 32,
		HmdConnected = 128
	}
}
