using System;

namespace Ovr
{
	public class AndroidGLESConfig : RenderAPIConfig
	{
		public AndroidGLESConfig(Sizei rtSize, int multisample)
		{
			this.Header.API = RenderAPIType.Android_GLES;
			this.Header.RTSize = rtSize;
			this.Header.Multisample = multisample;
		}

		internal override RenderAPIConfig_Raw ToRaw()
		{
			return new RenderAPIConfig_Raw
			{
				Header = this.Header
			};
		}
	}
}
