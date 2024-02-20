using System;

namespace Ovr
{
	public struct TextureHeader
	{
		public RenderAPIType API;

		public Sizei TextureSize;

		public Recti RenderViewport;
	}
}
