using System;

namespace Ovr
{
	public class D3D9Config : RenderAPIConfig
	{
		public D3D9Config(Sizei rtSize, int multisample, IntPtr IDirect3DDevice9_pDevice, IntPtr IDirect3DSwapChain9_pSwapChain)
		{
			this.Header.API = RenderAPIType.D3D9;
			this.Header.RTSize = rtSize;
			this.Header.Multisample = multisample;
			this._IDirect3DDevice9_pDevice = IDirect3DDevice9_pDevice;
			this._IDirect3DSwapChain9_pSwapChain = IDirect3DSwapChain9_pSwapChain;
		}

		internal override RenderAPIConfig_Raw ToRaw()
		{
			return new RenderAPIConfig_Raw
			{
				Header = this.Header,
				PlatformData0 = this._IDirect3DDevice9_pDevice,
				PlatformData1 = this._IDirect3DSwapChain9_pSwapChain
			};
		}

		private IntPtr _IDirect3DDevice9_pDevice;

		private IntPtr _IDirect3DSwapChain9_pSwapChain;
	}
}
