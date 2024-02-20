using System;

namespace Ovr
{
	public enum HmdCaps
	{
		Present = 1,
		Available,
		Captured = 4,
		ExtendDesktop = 8,
		NoMirrorToWindow = 8192,
		DisplayOff = 64,
		LowPersistence = 128,
		DynamicPrediction = 512,
		NoVSync = 4096,
		WritableMask = 13296,
		ServiceMask = 9200
	}
}
