using System;

namespace Ovr
{
	public class D3D9TextureData : Texture
	{
		public D3D9TextureData(Sizei textureSize, Recti renderViewport, IntPtr IDirect3DTexture9_pTexture)
		{
			this.Header.API = RenderAPIType.D3D9;
			this.Header.TextureSize = textureSize;
			this.Header.RenderViewport = renderViewport;
			this._IDirect3DTexture9_pTexture = IDirect3DTexture9_pTexture;
		}

		internal override Texture_Raw ToRaw()
		{
			return new Texture_Raw
			{
				Header = this.Header,
				PlatformData_0 = this._IDirect3DTexture9_pTexture
			};
		}

		public IntPtr _IDirect3DTexture9_pTexture;
	}
}
