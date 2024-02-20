using System;

namespace Ovr
{
	internal struct FrameTiming_Raw
	{
		public float DeltaSeconds;

		public double ThisFrameSeconds;

		public double TimewarpPointSeconds;

		public double NextFrameSeconds;

		public double ScanoutMidpointSeconds;

		public double EyeScanoutSeconds_0;

		public double EyeScanoutSeconds_1;
	}
}
