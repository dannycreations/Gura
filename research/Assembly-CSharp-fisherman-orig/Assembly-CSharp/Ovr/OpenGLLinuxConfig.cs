using System;

namespace Ovr
{
	public class OpenGLLinuxConfig : RenderAPIConfig
	{
		public OpenGLLinuxConfig(Sizei rtSize, int multisample, IntPtr optionalXDisplay, IntPtr optionalWindow)
		{
			this.Header.API = RenderAPIType.OpenGL;
			this.Header.RTSize = rtSize;
			this.Header.Multisample = multisample;
			this._OptionalXDisplay = optionalXDisplay;
			this._OptionalWindow = optionalWindow;
		}

		internal override RenderAPIConfig_Raw ToRaw()
		{
			return new RenderAPIConfig_Raw
			{
				Header = this.Header,
				PlatformData0 = this._OptionalXDisplay,
				PlatformData1 = this._OptionalWindow
			};
		}

		private IntPtr _OptionalXDisplay;

		private IntPtr _OptionalWindow;
	}
}
