using System;

namespace Ovr
{
	public class D3D10Config : RenderAPIConfig
	{
		public D3D10Config(Sizei rtSize, int multisample, IntPtr ID3D10RenderTargetView_pBackBufferRT, IntPtr IDXGISwapChain_pSwapChain)
		{
			this.Header.API = RenderAPIType.D3D10;
			this.Header.RTSize = rtSize;
			this.Header.Multisample = multisample;
			this._ID3D10RenderTargetView_pBackBufferRT = ID3D10RenderTargetView_pBackBufferRT;
			this._IDXGISwapChain_pSwapChain = IDXGISwapChain_pSwapChain;
		}

		internal override RenderAPIConfig_Raw ToRaw()
		{
			return new RenderAPIConfig_Raw
			{
				Header = this.Header,
				PlatformData0 = IntPtr.Zero,
				PlatformData1 = this._ID3D10RenderTargetView_pBackBufferRT,
				PlatformData2 = this._IDXGISwapChain_pSwapChain
			};
		}

		private IntPtr _ID3D10RenderTargetView_pBackBufferRT;

		private IntPtr _IDXGISwapChain_pSwapChain;
	}
}
