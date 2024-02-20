using System;

namespace Ovr
{
	public abstract class RenderAPIConfig
	{
		public RenderAPIConfig()
		{
			this.Header.API = RenderAPIType.None;
		}

		internal abstract RenderAPIConfig_Raw ToRaw();

		public RenderAPIConfigHeader Header;
	}
}
