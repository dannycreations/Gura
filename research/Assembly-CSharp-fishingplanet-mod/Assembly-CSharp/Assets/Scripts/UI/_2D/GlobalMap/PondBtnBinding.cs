using System;

namespace Assets.Scripts.UI._2D.GlobalMap
{
	public class PondBtnBinding
	{
		public PondBtnBinding(int l, int r, int u, int d)
		{
			this.Left = l;
			this.Right = r;
			this.Up = u;
			this.Down = d;
		}

		public int Left { get; private set; }

		public int Right { get; private set; }

		public int Up { get; private set; }

		public int Down { get; private set; }
	}
}
