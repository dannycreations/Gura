using System;

namespace Ovr
{
	public class GLTextureData : Texture
	{
		public GLTextureData(Sizei textureSize, Recti renderViewport, IntPtr texId)
		{
			this.Header.API = RenderAPIType.OpenGL;
			this.Header.TextureSize = textureSize;
			this.Header.RenderViewport = renderViewport;
			this._texId = texId;
		}

		internal override Texture_Raw ToRaw()
		{
			return new Texture_Raw
			{
				Header = this.Header,
				PlatformData_0 = this._texId
			};
		}

		public IntPtr _texId;
	}
}
