using System;

namespace Ovr
{
	public enum DistortionCaps
	{
		Chromatic = 1,
		TimeWarp,
		Vignette = 8,
		NoRestore = 16,
		FlipInput = 32,
		SRGB = 64,
		Overdrive = 128,
		HqDistortion = 256,
		ProfileNoTimewarpSpinWaits = 65536
	}
}
