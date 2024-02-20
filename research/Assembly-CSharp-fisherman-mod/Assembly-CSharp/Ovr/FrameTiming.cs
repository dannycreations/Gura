using System;

namespace Ovr
{
	public struct FrameTiming
	{
		internal FrameTiming(FrameTiming_Raw raw)
		{
			this.DeltaSeconds = raw.DeltaSeconds;
			this.ThisFrameSeconds = raw.ThisFrameSeconds;
			this.TimewarpPointSeconds = raw.TimewarpPointSeconds;
			this.NextFrameSeconds = raw.NextFrameSeconds;
			this.ScanoutMidpointSeconds = raw.ScanoutMidpointSeconds;
			this.EyeScanoutSeconds = new double[] { raw.EyeScanoutSeconds_0, raw.EyeScanoutSeconds_1 };
		}

		public float DeltaSeconds;

		public double ThisFrameSeconds;

		public double TimewarpPointSeconds;

		public double NextFrameSeconds;

		public double ScanoutMidpointSeconds;

		public double[] EyeScanoutSeconds;
	}
}
