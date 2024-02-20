using System;

namespace Ovr
{
	public class D3D11Config : RenderAPIConfig
	{
		public D3D11Config(Sizei rtSize, int multisample, IntPtr ID3D11Device_pDevice, IntPtr ID3D11DeviceContext_pDeviceContext, IntPtr ID3D11RenderTargetView_pBackBufferRT, IntPtr IDXGISwapChain_pSwapChain)
		{
			this.Header.API = RenderAPIType.D3D11;
			this.Header.RTSize = rtSize;
			this.Header.Multisample = multisample;
			this._ID3D11Device_pDevice = ID3D11Device_pDevice;
			this._ID3D11DeviceContext_pDeviceContext = ID3D11DeviceContext_pDeviceContext;
			this._ID3D11RenderTargetView_pBackBufferRT = ID3D11RenderTargetView_pBackBufferRT;
			this._IDXGISwapChain_pSwapChain = IDXGISwapChain_pSwapChain;
		}

		internal override RenderAPIConfig_Raw ToRaw()
		{
			return new RenderAPIConfig_Raw
			{
				Header = this.Header,
				PlatformData0 = this._ID3D11Device_pDevice,
				PlatformData1 = this._ID3D11DeviceContext_pDeviceContext,
				PlatformData2 = this._ID3D11RenderTargetView_pBackBufferRT,
				PlatformData3 = this._IDXGISwapChain_pSwapChain
			};
		}

		private IntPtr _ID3D11Device_pDevice;

		private IntPtr _ID3D11DeviceContext_pDeviceContext;

		private IntPtr _ID3D11RenderTargetView_pBackBufferRT;

		private IntPtr _IDXGISwapChain_pSwapChain;
	}
}
