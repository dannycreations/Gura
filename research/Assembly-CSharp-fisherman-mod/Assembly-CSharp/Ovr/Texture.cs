using System;

namespace Ovr
{
	public abstract class Texture
	{
		public Texture()
		{
			this.Header.API = RenderAPIType.None;
		}

		internal abstract Texture_Raw ToRaw();

		public TextureHeader Header;
	}
}
