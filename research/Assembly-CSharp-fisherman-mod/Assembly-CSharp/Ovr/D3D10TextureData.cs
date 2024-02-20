using System;

namespace Ovr
{
	public class D3D10TextureData : Texture
	{
		public D3D10TextureData(Sizei textureSize, Recti renderViewport, IntPtr ID3D10Texture2D_pTexture, IntPtr ID3D10ShaderResourceView_pSRView)
		{
			this.Header.API = RenderAPIType.D3D10;
			this.Header.TextureSize = textureSize;
			this.Header.RenderViewport = renderViewport;
			this._ID3D10Texture2D_pTexture = ID3D10Texture2D_pTexture;
			this._ID3D10ShaderResourceView_pSRView = ID3D10ShaderResourceView_pSRView;
		}

		internal override Texture_Raw ToRaw()
		{
			return new Texture_Raw
			{
				Header = this.Header,
				PlatformData_0 = this._ID3D10Texture2D_pTexture,
				PlatformData_1 = this._ID3D10ShaderResourceView_pSRView
			};
		}

		public IntPtr _ID3D10Texture2D_pTexture;

		public IntPtr _ID3D10ShaderResourceView_pSRView;
	}
}
