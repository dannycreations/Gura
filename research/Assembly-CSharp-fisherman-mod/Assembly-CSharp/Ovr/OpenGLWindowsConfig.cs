using System;

namespace Ovr
{
	public class OpenGLWindowsConfig : RenderAPIConfig
	{
		public OpenGLWindowsConfig(Sizei rtSize, int multisample, IntPtr hwnd, IntPtr HDCDeviceContext)
		{
			this.Header.API = RenderAPIType.OpenGL;
			this.Header.RTSize = rtSize;
			this.Header.Multisample = multisample;
			this._hwnd = hwnd;
			this._HDCDeviceContext = HDCDeviceContext;
		}

		internal override RenderAPIConfig_Raw ToRaw()
		{
			return new RenderAPIConfig_Raw
			{
				Header = this.Header,
				PlatformData0 = this._hwnd,
				PlatformData1 = this._HDCDeviceContext
			};
		}

		public IntPtr _hwnd;

		public IntPtr _HDCDeviceContext;
	}
}
